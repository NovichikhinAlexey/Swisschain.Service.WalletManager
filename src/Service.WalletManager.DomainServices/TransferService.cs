using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Lykke.Service.BlockchainApi.Client.Models;
using Service.WalletManager.Domain.Models;
using Service.WalletManager.Domain.Services;

namespace Service.WalletManager.DomainServices
{
    public class TransferService : ITransferService
    {
        private readonly IBlockchainApiClientProvider _blockchainApiClientProvider;
        private readonly AssetService _assetService;

        public TransferService(IBlockchainApiClientProvider blockchainApiClientProvider,
            AssetService assetService)
        {
            _blockchainApiClientProvider = blockchainApiClientProvider;
            _assetService = assetService;
        }

        public async Task<string> BuildTransactionAsync(
            Guid operationId,
            string blockchainId,
            string blockchainAssetId,
            string fromAddress,
            string fromAddressContext,
            string toAddress,
            string amount,
            bool includeFee
        )
        {
            var client = _blockchainApiClientProvider.Get(blockchainId);
            var asset = await _assetService.GetBlockchainAsset(blockchainId, blockchainAssetId);
            var amountDecimal = ConverterExtensions.ConvertFromString(amount, asset.Accuracy, asset.Accuracy);
            var buildedTransaction = await client.BuildSingleTransactionAsync(
                operationId,
                fromAddress,
                fromAddressContext,
                toAddress,
                asset,
                amountDecimal,
                includeFee
            );

            return buildedTransaction.TransactionContext;
        }

        public async Task<TransactionBroadcastResult> SendTransactionAsync(Guid operationId, string blockchainId, string signedTransaction)
        {
            var client = _blockchainApiClientProvider.Get(blockchainId);
            var result = await client.BroadcastTransactionAsync(operationId, signedTransaction);

            var mapped = result switch
            {
                TransactionBroadcastingResult.AlreadyBroadcasted => TransactionBroadcastResult.AlreadyBroadcasted,
                TransactionBroadcastingResult.AmountIsTooSmall=> TransactionBroadcastResult.AmountIsTooSmall,
                TransactionBroadcastingResult.BuildingShouldBeRepeated=> TransactionBroadcastResult.BuildingShouldBeRepeated,
                TransactionBroadcastingResult.NotEnoughBalance=> TransactionBroadcastResult.NotEnoughBalance,
                TransactionBroadcastingResult.Success => TransactionBroadcastResult.Success,
            };

            return mapped;
        }
    }
}
