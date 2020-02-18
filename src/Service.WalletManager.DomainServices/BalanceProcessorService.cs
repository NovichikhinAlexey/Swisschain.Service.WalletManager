using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.BlockchainApi.Client;
using Lykke.Service.BlockchainApi.Client.Models;
using Service.WalletManager.Domain.Models;
using Service.WalletManager.Domain.Repositories;

namespace Service.WalletManager.DomainServices
{
    public class Asset
    {
        public string BlockchainId { get; set; }

        public string BlockchainAssetId { get; set; }
    }

    public class BalanceProcessorService
    {
        private readonly string _blockchainType;
        private readonly ILog _log;
        private readonly IBlockchainApiClient _blockchainApiClient;
        private readonly IEnrolledBalanceRepository _enrolledBalanceRepository;
        private readonly HashSet<string> _warningAssets;
        private readonly IReadOnlyDictionary<string, Asset> _assets = new ReadOnlyDictionary<string, Asset>(
            new Dictionary<string, Asset>()
            {
                {"Bitcoin", new Asset() { BlockchainAssetId = "Bitcoin", BlockchainId = "Bitcoin"}},
                {"Ethereum", new Asset() { BlockchainAssetId = "Ethereum", BlockchainId = "Ethereum"}}
            });

        private IReadOnlyDictionary<string, BlockchainAsset> _blockchainAssets;

        public BalanceProcessorService(
            string blockchainType,
            ILogFactory logFactory,
            IBlockchainApiClient blockchainApiClient,
            IEnrolledBalanceRepository enrolledBalanceRepository,
            IReadOnlyDictionary<string, BlockchainAsset> blockchainAssets)
        {
            _blockchainType = blockchainType;
            _log = logFactory.CreateLog(this);
            _blockchainApiClient = blockchainApiClient;
            _enrolledBalanceRepository = enrolledBalanceRepository;
            _blockchainAssets = blockchainAssets;

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
                    _log.Warning(nameof(ProcessBalance), "Asset is not found", context: depositWallet);

                    _warningAssets.Add(depositWallet.AssetId);
                }

                return;
            }

            if (!_blockchainAssets.TryGetValue(asset.BlockchainAssetId, out var blockchainAsset))
            {
                if (!_warningAssets.Contains(depositWallet.AssetId))
                {
                    _log.Warning(nameof(ProcessBalance), "Blockchain asset is not found", context: depositWallet);

                    _warningAssets.Add(depositWallet.AssetId);
                }

                return;
            }

            if (!enrolledBalances.TryGetValue(
                GetEnrolledBalancesDictionaryKey(depositWallet.Address, depositWallet.AssetId),
                out var enrolledBalance))
            {
                _log.Warning(nameof(ProcessBalance), "Can't get balances for address", context: depositWallet);

                return;
            }

            var balanceStr = ConverterExtensions.ConvertToString(depositWallet.Balance, blockchainAsset.Accuracy, blockchainAsset.Accuracy);
            var balance = BigInteger.Parse(balanceStr);

            var cashinCouldBeStarted = CouldBeStarted(
                balance,
                depositWallet.Block,
                enrolledBalance?.Balance ?? 0,
                enrolledBalance?.Block ?? 0,
                blockchainAsset.Accuracy,
                out var _, 
                out var _);

            if (!cashinCouldBeStarted)
            {
                return;
            }

            //Detect operation
            await _enrolledBalanceRepository.SetBalanceAsync(
                new DepositWalletKey(blockchainAsset.AssetId,
                    asset.BlockchainId, 
                    depositWallet.Address), 
                enrolledBalance.Balance,
                enrolledBalance.Block);
        }

        private static bool CouldBeStarted(
            BigInteger balanceAmount,
            BigInteger balanceBlock,
            BigInteger enrolledBalanceAmount,
            BigInteger enrolledBalanceBlock,
            int assetAccuracy,
            out BigInteger operationAmount,
            out double matchingEngineOperationAmount)
        {
            operationAmount = 0;
            matchingEngineOperationAmount = 0;

            if (balanceBlock < enrolledBalanceBlock)
            {
                // This balance was already processed
                return false;
            }

            operationAmount = balanceAmount - enrolledBalanceAmount;

            if (operationAmount <= 0)
            {
                // Nothing to transfer
                return false;
            }

            matchingEngineOperationAmount = ((double)operationAmount).TruncateDecimalPlaces(assetAccuracy);

            if (matchingEngineOperationAmount <= 0)
            {
                // Nothing to enroll to the ME
                return false;
            }

            return true;
        }

        private async Task<IReadOnlyDictionary<string, EnrolledBalance>> GetEnrolledBalancesAsync(IEnumerable<WalletBalance> balances)
        {
            var walletKeys = balances.Select(x => new DepositWalletKey
            (
                blockchainAssetId: x.AssetId,
                blockchainId: _blockchainType,
                depositWalletAddress: x.Address
            ));

            return (await _enrolledBalanceRepository.GetAsync(walletKeys))
                .ToDictionary(
                    x => GetEnrolledBalancesDictionaryKey(x.Key.DepositWalletAddress, x.Key.BlockchainAssetId),
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
            return $"{address}:{assetId}";
        }
    }
}
