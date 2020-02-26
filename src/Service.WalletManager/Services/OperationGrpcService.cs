using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using Service.WalletManager.Domain.Models;
using Service.WalletManager.Domain.Repositories;
using Service.WalletManager.Domain.Services;
using Swisschain.Sdk.Server.Common;
using Service.WalletManager.Protos;
using Operation = Service.WalletManager.Domain.Models.Operation;

namespace Service.WalletManager.Services
{
    public class OperationGrpcService : Operations.OperationsBase
    {
        private readonly IOperationRepository _operationRepository;
        
        public OperationGrpcService(IOperationRepository operationRepository)
        {
            _operationRepository = operationRepository;
        }

        public override async Task<GetOperationResponse> GetOperations(GetOperationRequest request, ServerCallContext context)
        {
            var operations = await _operationRepository.GetAsync(new DepositWalletKey(
                request.WalletKey.BlockchainAssetId,
                request.WalletKey.BlockchainId,
                request.WalletKey.WalletAddress), request.Skip, request.Take);

            var response = new GetOperationResponse();

            if (operations != null && operations.Any())
            {
                response.Operations.AddRange(operations.Select( x => 
                    new OperationResponse()
                    {
                        WalletKey = request.WalletKey,
                        BalanceChange = x.BalanceChange.ToString(),
                        OperationId = x.OperationId,
                        Block = x.Block
                    }));
            }

            return response;
        }

        public override async Task<GetOperationResponse> GetOperationsForBlockchain(GetOperationsForBlockchainRequest request, ServerCallContext context)
        {
            var operations = await _operationRepository.GetAllForBlockchainAsync(
                request.BlockchainId, 
                request.Skip,
                request.Take);

            var response = new GetOperationResponse();

            if (operations != null && operations.Any())
            {
                response.Operations.AddRange(operations.Select(x =>
                    new OperationResponse()
                    {
                        WalletKey = new WalletKey()
                        {
                            BlockchainAssetId = x.Key.BlockchainAssetId,
                            BlockchainId = x.Key.BlockchainId,
                            WalletAddress = x.Key.WalletAddress
                        },
                        BalanceChange = x.BalanceChange.ToString(),
                        OperationId = x.OperationId,
                        Block = x.Block
                    }));
            }

            return response;
        }
    }
}
