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
using Lykke.Service.BlockchainApi.Contract.Balances;
using Microsoft.Extensions.Logging;
using Service.WalletManager.Domain.Models;
using Service.WalletManager.Domain.Repositories;
using Service.WalletManager.Domain.Services;
using Service.WalletManager.Domain.Util;

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

        public async Task ProcessAsync(int batchSize)
        {
            int skip = 0;
            var balancesFromDatabase = new List<EnrolledBalance>();
            do
            {
                var received =
                    (await _enrolledBalanceRepository.GetAllForBlockchainAsync(_blockchainId, 0, 100))
                    ?.ToList();

                if (received == null || !received.Any())
                    break;

                balancesFromDatabase.AddRange(received);

                skip += received.Count();

                if (received.Count() < 100)
                    break;

            } while (true);

            var balancesFromApi = new Dictionary<(string AssetId, string Address),WalletBalance>();
            await _blockchainApiClient.EnumerateWalletBalanceBatchesAsync(
                batchSize,
                assetId => GetAssetAccuracy(assetId, batchSize),
                batch =>
                {
                    if (batch != null && batch.Any())
                    {
                        foreach (var item in batch)
                        {
                            balancesFromApi[(item.AssetId, item.Address.ToLower(CultureInfo.InvariantCulture))] = item;
                        }
                    }

                    return Task.FromResult(true);
                });

            await ProcessBalancesBatchAsync(balancesFromApi, balancesFromDatabase);
        }

        private async Task ProcessBalancesBatchAsync(IDictionary<(string AssetId, string Address), WalletBalance> fromApi, 
            List<EnrolledBalance> fromDatabase)
        {
            foreach (var balance in fromDatabase)
            {
                await ProcessBalance(balance, fromApi);
            }
        }

        private async Task ProcessBalance(
            EnrolledBalance enrolledBalance,
            IDictionary<(string AssetId, string Address), WalletBalance> depositWallets)
        {
            //if (!_assets.TryGetValue(enrolledBalance.AssetId, out var asset))
            //{
            //    if (!_warningAssets.Contains(enrolledBalance.AssetId))
            //    {
            //        _log.LogWarning("Asset is not found {depositWallet}", enrolledBalance);

            //        _warningAssets.Add(enrolledBalance.AssetId);
            //    }

            //    return;
            //}

            var key = enrolledBalance.Key;

            if (!_blockchainAssets.TryGetValue(key.BlockchainAssetId, out var blockchainAsset))
            {
                //if (!_warningAssets.Contains(enrolledBalance.))
                //{
                 _log.LogWarning("Blockchain asset is not found {depositWallet}}", enrolledBalance);
                //
                //    _warningAssets.Add(enrolledBalance.AssetId);
                //}

                // TODO:
                return;
            }

            depositWallets.TryGetValue(
                (enrolledBalance.Key.BlockchainAssetId,
                    enrolledBalance.Key.WalletAddress.ToLower(CultureInfo.InvariantCulture)),
                out var depositWallet);
            //{
            //    depositWallet = new WalletBalance(new WalletBalanceContract()
            //    {
            //        AssetId = blockchainAsset.AssetId,
            //        Address = enrolledBalance.Key.WalletAddress,
            //        Balance = "0",
            //        Block = enrolledBalance.Block,
            //    }, blockchainAsset.Accuracy);
            //}

            var balanceStr = ConverterExtensions.ConvertToString(depositWallet?.Balance ?? 0m, 
                blockchainAsset.Accuracy, blockchainAsset.Accuracy);
            var balance = BigInteger.Parse(balanceStr);
            var balanceBlock = depositWallet?.Block ?? enrolledBalance.Block;

            var cashinCouldBeStarted = CouldBeStarted(
                balance,
                balanceBlock,
                enrolledBalance.Balance,
                enrolledBalance.Block,
                blockchainAsset.Accuracy,
                out var operationAmount);

            if (!cashinCouldBeStarted)
            {
                return;
            }

            await RetryPolicy.RetryWithExpBackoff(async () =>
            {
                await _enrolledBalanceRepository.SetBalanceAsync(
                    key,
                    balance,
                    balanceBlock);
                await _operationRepository.SetAsync(CreateOperation.Create(key, operationAmount, enrolledBalance.Block));
            }, _log);
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

            if (operationAmount == 0)
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
