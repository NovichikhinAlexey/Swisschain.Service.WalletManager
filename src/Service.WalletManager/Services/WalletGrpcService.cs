using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using Service.WalletManager.Domain.Models;
using Service.WalletManager.Domain.Services;
using Swisschain.Sdk.Server.Common;
using Service.WalletManager.Protos;

namespace Service.WalletManager.Services
{
    public class WalletGrpcService : Wallets.WalletsBase
    {
        private readonly IWalletService _walletService;

        public WalletGrpcService(IWalletService walletService)
        {
            _walletService = walletService;
        }

        public override async Task<EmptyResponse> RegisterWallet(RegisterWalletRequest request, ServerCallContext context)
        {
            var key = new DepositWalletKey(
                request.WalletKey.BlockchainAssetId,
                request.WalletKey.BlockchainId,
                request.WalletKey.WalletAddress);

            await _walletService.RegisterWalletAsync(key);

            return new EmptyResponse();
        }

        public override async Task<EmptyResponse> DeleteWallet(DeleteWalletRequest request, ServerCallContext context)
        {
            var key = new DepositWalletKey(
                request.WalletKey.BlockchainAssetId,
                request.WalletKey.BlockchainId,
                request.WalletKey.WalletAddress);
            await _walletService.DeleteWalletAsync(key);

            return new EmptyResponse();
        }
    }
}
