using System.Threading.Tasks;
using Lykke.Service.BlockchainApi.Client.Models;
using Service.WalletManager.Domain.Models;
using Service.WalletManager.Domain.Repositories;
using Service.WalletManager.Domain.Services;

namespace Service.WalletManager.DomainServices
{
    public class WalletService : IWalletService
    {
        private readonly IBlockchainApiClientProvider _blockchainApiClientProvider;
        private readonly IEnrolledBalanceRepository _enrolledBalanceRepository;
        private readonly AssetService _assetService;

        public WalletService(
            IBlockchainApiClientProvider blockchainApiClientProvider,
            IEnrolledBalanceRepository enrolledBalanceRepository,
            AssetService assetService)
        {
            _blockchainApiClientProvider = blockchainApiClientProvider;
            _enrolledBalanceRepository = enrolledBalanceRepository;
            _assetService = assetService;
        }

        public async Task RegisterWalletAsync(DepositWalletKey key)
        {
            var client = _blockchainApiClientProvider.Get(key.BlockchainId);
            await _enrolledBalanceRepository.SetBalanceAsync(key, 0, 0);
            await client.StartBalanceObservationAsync(key.WalletAddress);
        }

        public async Task DeleteWalletAsync(DepositWalletKey key)
        {
            var client = _blockchainApiClientProvider.Get(key.BlockchainId);
            //await _enrolledBalanceRepository.DeleteBalanceAsync(key, 0, 0);
            await client.StopBalanceObservationAsync(key.WalletAddress);
        }
    }
}
