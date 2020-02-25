using System;
using System.Threading.Tasks;
using Service.WalletManager.Domain.Models;

namespace Service.WalletManager.Domain.Services
{
    public interface ITransferService
    {
        Task<string> BuildTransactionAsync(
            Guid operationId,
            string blockchainId,
            string blockchainAssetId,
            string fromAddress,
            string fromAddressContext,
            string toAddress,
            string amount,
            bool includeFee
        );

        Task<TransactionBroadcastResult> SendTransactionAsync(
            Guid operationId, 
            string blockchainId, 
            string signedTransaction);
    }
}
