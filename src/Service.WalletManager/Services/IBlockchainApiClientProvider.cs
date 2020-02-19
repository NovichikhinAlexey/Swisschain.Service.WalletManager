using Lykke.Service.BlockchainApi.Client;

namespace Service.WalletManager.Services
{
    public interface IBlockchainApiClientProvider
    {
        IBlockchainApiClient Get(string blockchainType);
    }
}