using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using Service.WalletManager.Domain.Models;
using Service.WalletManager.Domain.Repositories;
using Service.WalletManager.Domain.Services;
using Swisschain.Sdk.Server.Common;
using Service.WalletManager.Protos;
using Service.WalletManager.Protos.Common;

namespace Service.WalletManager.Services
{
    public class BalanceGrpcService : Balances.BalancesBase
    {
        private readonly IEnrolledBalanceRepository _enrolledBalanceRepository;

        public BalanceGrpcService(IEnrolledBalanceRepository enrolledBalanceRepository)
        {
            _enrolledBalanceRepository = enrolledBalanceRepository;
        }

        public override async Task<BalanceResponses> GetAllBalances(GetAllBalanceRequest request, ServerCallContext context)
        {
            var balances = await _enrolledBalanceRepository.GetAllAsync(request.Skip, request.Skip);
            var response = new BalanceResponses();

            if (balances != null && balances.Any())
                response.Balances.AddRange(balances.Select(x => new BalanceResponse()
                {
                    Block = x.Block,
                    Balance = x.Balance.ToString(),
                    WalletKey = new WalletKey()
                    {
                        BlockchainAssetId = x.Key.BlockchainAssetId,
                        BlockchainId = x.Key.BlockchainId,
                        WalletAddress = x.Key.WalletAddress
                    }
                }));

            return response;
        }

        public override async Task<BalanceResponse> GetBalance(GetBalanceRequest request, ServerCallContext context)
        {
            var balance = await _enrolledBalanceRepository.TryGetAsync(
                new DepositWalletKey(
                    request.WalletKey.BlockchainAssetId,
                    request.WalletKey.BlockchainId,
                    request.WalletKey.WalletAddress));

            return new BalanceResponse()
            {
                 Block = balance?.Block ?? 0,
                 Balance = balance?.Balance.ToString(),
                 WalletKey = request.WalletKey
            };
        }
    }
}
