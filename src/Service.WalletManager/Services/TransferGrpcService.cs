using System;
using System.Threading.Tasks;
using Grpc.Core;
using Service.WalletManager.Domain.Models;
using Service.WalletManager.Domain.Services;
using Service.WalletManager.Protos;

namespace Service.WalletManager.Services
{
    public class TransferGrpcService : Transfers.TransfersBase
    {
        private readonly ITransferService _transferService;

        public TransferGrpcService(ITransferService transferService)
        {
            _transferService = transferService;
        }

        public override async Task<BuiltTransactionResponse> BuildTransaction(BuildTransactionRequest request, ServerCallContext context)
        {
            var res = await _transferService.BuildTransactionAsync(
                Guid.Parse(request.OperationId),
                request.BlockchainId,
                request.BlockchainAssetId,
                request.FromAddress,
                request.FromAddressContext,
                request.ToAddress,
                request.Amount,
                request.IncludeFee);

            var response = new BuiltTransactionResponse()
            {
                TransactionContext = res
            };

            return response;
        }

        public override async Task<BroadcastTransactionResponse> BroadcastTransaction(BroadcastTransactionRequest request, ServerCallContext context)
        {
            var operation = Guid.Parse(request.OperationId);
            var res = await _transferService.SendTransactionAsync(operation, request.BlockchainId, request.SignedTransaction);

            var response = new BroadcastTransactionResponse()
            {
                TransactionBroadcastResult = res switch
                {
                    TransactionBroadcastResult.AlreadyBroadcasted => BroadcastTransactionResponse.Types.TransactionBroadcastResult.AlreadyBroadcasted,
                    TransactionBroadcastResult.AmountIsTooSmall=> BroadcastTransactionResponse.Types.TransactionBroadcastResult.AmountIsTooSmall,
                    TransactionBroadcastResult.BuildingShouldBeRepeated => BroadcastTransactionResponse.Types.TransactionBroadcastResult.BuildingShouldBeRepeated,
                    TransactionBroadcastResult.NotEnoughBalance=> BroadcastTransactionResponse.Types.TransactionBroadcastResult.NotEnoughBalance,
                    TransactionBroadcastResult.Success => BroadcastTransactionResponse.Types.TransactionBroadcastResult.Success,
                }
            };

            return response;
        }
    }
}
