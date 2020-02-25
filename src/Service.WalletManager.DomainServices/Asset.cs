using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Lykke.Service.BlockchainApi.Client.Models;
using Service.WalletManager.Domain.Services;

namespace Service.WalletManager.DomainServices
{
    public class Asset
    {
        public string BlockchainId { get; set; }

        public string BlockchainAssetId { get; set; }
    }

    public class AssetService
    {
        private readonly IBlockchainApiClientProvider _blockchainApiClientProvider;

        private readonly IReadOnlyDictionary<string, Asset> _assets = new ReadOnlyDictionary<string, Asset>(
            new Dictionary<string, Asset>()
            {
                {"BTC", new Asset() { BlockchainAssetId = "BTC", BlockchainId = "Bitcoin"}},
                {"ETH", new Asset() { BlockchainAssetId = "ETH", BlockchainId = "Ethereum"}}
            });

        //private ConcurrentDictionary<string, ConcurrentDictionary<string, BlockchainAsset>> _dictionary = 
        //    new ConcurrentDictionary<string, ConcurrentDictionary<string, BlockchainAsset>>();

        public AssetService(IBlockchainApiClientProvider blockchainApiClientProvider)
        {
            _blockchainApiClientProvider = blockchainApiClientProvider;
        }

        public async Task<BlockchainAsset> GetBlockchainAsset(string blockchainId, string blockchainAssetId)
        {
            if (!_assets.TryGetValue(blockchainAssetId, out var asset))
            {
                return null;
            }

            //if (_dictionary.TryGetValue(asset.BlockchainId, out var value))
            //{
            //    if (value.TryGetValue(blockchainAssetId, out var blockchainAsset))
            //    {
            //        return blockchainAsset;
            //    }
            //}

            var client = _blockchainApiClientProvider.Get(blockchainId);
            var assets = await client.GetAllAssetsAsync(100);

            if (!assets.TryGetValue(asset.BlockchainAssetId, out var blockchainAsset))
            {
                return null;
            }

            return blockchainAsset;
        }

        public async Task<(string blockchainId, BlockchainAsset)> GetBlockchainAssetForDepositWallet(string blockchainAssetId)
        {
            if (!_assets.TryGetValue(blockchainAssetId, out var asset))
            {
                return (null, null);
            }

            var client = _blockchainApiClientProvider.Get(asset.BlockchainId);
            var assets = await client.GetAllAssetsAsync(100);

            if (!assets.TryGetValue(asset.BlockchainAssetId, out var blockchainAsset))
            {
                return (null, null);
            }

            return (asset.BlockchainId, blockchainAsset);
        }
    }
}
