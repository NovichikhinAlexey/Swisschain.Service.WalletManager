using Lykke.Service.BlockchainApi.Client;

namespace Service.WalletManager.Domain.Services
{
    public interface IBlockchainApiClientProvider
    {
        IBlockchainApiClient Get(string blockchainType);
    }
}
