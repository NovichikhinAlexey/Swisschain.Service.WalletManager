using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Common;
using Lykke.Service.BlockchainApi.Client;
using Lykke.Service.BlockchainApi.Client.Models;
using Microsoft.Extensions.Logging;
using Service.WalletManager.Domain.Models;
using Service.WalletManager.Domain.Repositories;
using Service.WalletManager.Domain.Services;

namespace Service.WalletManager.DomainServices
{
    public class BalanceProcessorService : IBalanceProcessorService
    {
        private readonly string _blockchainId;
        private readonly ILogger<BalanceProcessorService> _log;
        private readonly IBlockchainApiClient _blockchainApiClient;
        private readonly IEnrolledBalanceRepository _enrolledBalanceRepository;
        private readonly HashSet<string> _warningAssets;
        private readonly IReadOnlyDictionary<string, Asset> _assets = new ReadOnlyDictionary<string, Asset>(
            new Dictionary<string, Asset>()
            {
                {"BTC", new Asset() { BlockchainAssetId = "BTC", BlockchainId = "Bitcoin"}},
                {"ETH", new Asset() { BlockchainAssetId = "ETH", BlockchainId = "Ethereum"}}
            });

        private IReadOnlyDictionary<string, BlockchainAsset> _blockchainAssets;
        private readonly IOperationRepository _operationRepository;

        public BalanceProcessorService(
            string blockchainId,
            ILogger<BalanceProcessorService> log,
            IBlockchainApiClient blockchainApiClient,
            IEnrolledBalanceRepository enrolledBalanceRepository,
            IOperationRepository operationRepository,
            IReadOnlyDictionary<string, BlockchainAsset> blockchainAssets)
        {
            _blockchainId = blockchainId;
            _log = log;
            _blockchainApiClient = blockchainApiClient;
            _enrolledBalanceRepository = enrolledBalanceRepository;
            _blockchainAssets = blockchainAssets;
            _operationRepository = operationRepository;

            _warningAssets = new HashSet<string>();
        }

        public Task ProcessAsync(int batchSize)
        {
            return _blockchainApiClient.EnumerateWalletBalanceBatchesAsync(
                batchSize,
                assetId => GetAssetAccuracy(assetId, batchSize),
                async batch =>
                {
                    await ProcessBalancesBatchAsync(batch, batchSize);
                    return true;
                });
        }

        private async Task ProcessBalancesBatchAsync(IReadOnlyList<WalletBalance> batch, int batchSize)
        {
            var enrolledBalances = await GetEnrolledBalancesAsync(batch);

            foreach (var balance in batch)
            {
                await ProcessBalance(balance, enrolledBalances, batchSize);
            }
        }

        private async Task ProcessBalance(
            WalletBalance depositWallet,
            IReadOnlyDictionary<string, EnrolledBalance> enrolledBalances,
            int batchSize)
        {
            if (!_assets.TryGetValue(depositWallet.AssetId, out var asset))
            {
                if (!_warningAssets.Contains(depositWallet.AssetId))
                {
                    _log.LogWarning("Asset is not found {depositWallet}", depositWallet);

                    _warningAssets.Add(depositWallet.AssetId);
                }

                return;
            }

            if (!_blockchainAssets.TryGetValue(asset.BlockchainAssetId, out var blockchainAsset))
            {
                if (!_warningAssets.Contains(depositWallet.AssetId))
                {
                    _log.LogWarning("Blockchain asset is not found {depositWallet}}", depositWallet);

                    _warningAssets.Add(depositWallet.AssetId);
                }

                return;
            }

            var key = new DepositWalletKey(blockchainAsset.AssetId,
                asset.BlockchainId,
                depositWallet.Address);

            if (!enrolledBalances.TryGetValue(
                GetEnrolledBalancesDictionaryKey(depositWallet.Address.ToLower(CultureInfo.InvariantCulture), depositWallet.AssetId),
                out var enrolledBalance))
            {
                enrolledBalance = EnrolledBalance.Create(key, 0, 0);
            }

            var balanceStr = ConverterExtensions.ConvertToString(depositWallet.Balance, blockchainAsset.Accuracy, blockchainAsset.Accuracy);
            var balance = BigInteger.Parse(balanceStr);

            var cashinCouldBeStarted = CouldBeStarted(
                balance,
                depositWallet.Block,
                enrolledBalance?.Balance ?? 0,
                enrolledBalance?.Block ?? 0,
                blockchainAsset.Accuracy,
                out var operationAmount);

            if (!cashinCouldBeStarted)
            {
                return;
            }

            await _enrolledBalanceRepository.SetBalanceAsync(
                key,
                balance,
                depositWallet.Block);
            await _operationRepository.SetAsync(CreateOperation.Create(key, operationAmount, depositWallet.Block));
        }

        private static bool CouldBeStarted(
            BigInteger balanceAmount,
            BigInteger balanceBlock,
            BigInteger enrolledBalanceAmount,
            BigInteger enrolledBalanceBlock,
            int assetAccuracy,
            out BigInteger operationAmount)
        {
            operationAmount = 0;

            if (balanceBlock < enrolledBalanceBlock)
            {
                // This balance was already processed
                return false;
            }

            operationAmount = balanceAmount - enrolledBalanceAmount;

            if (operationAmount <= 0)
            {
                // No visbible changes have happened since the last check
                return false;
            }

            return true;
        }

        private async Task<IReadOnlyDictionary<string, EnrolledBalance>> GetEnrolledBalancesAsync(IEnumerable<WalletBalance> balances)
        {
            var walletKeys = balances.Select(x => new DepositWalletKey
            (
                blockchainAssetId: x.AssetId,
                blockchainId: _blockchainId,
                depositWalletAddress: x.Address
            ));

            return (await _enrolledBalanceRepository.GetAsync(walletKeys))
                .ToDictionary(
                    x => GetEnrolledBalancesDictionaryKey(x.Key.WalletAddress, x.Key.BlockchainAssetId),
                    y => y);
        }

        private int GetAssetAccuracy(string assetId, int batchSize)
        {
            if (!_blockchainAssets.TryGetValue(assetId, out var asset))
            {
                // Unknown asset, tries to refresh cached assets

                _blockchainAssets = _blockchainApiClient
                    .GetAllAssetsAsync(batchSize)
                    .GetAwaiter()
                    .GetResult();

                if (!_blockchainAssets.TryGetValue(assetId, out asset))
                {
                    throw new InvalidOperationException($"Asset {assetId} not found");
                }
            }

            return asset.Accuracy;
        }

        private string GetEnrolledBalancesDictionaryKey(string address, string assetId)
        {
            return $"{address.ToLower(CultureInfo.InvariantCulture)}:{assetId}";
        }
    }
}
