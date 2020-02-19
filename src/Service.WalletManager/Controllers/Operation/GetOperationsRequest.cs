using System.Numerics;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Service.WalletManager.Controllers.Operation
{
    [BindProperties(SupportsGet = true)]
    public class GetOperationsRequest
    {
        [JsonPropertyName("blockchainId")]
        public string BlockchainId { get; set; }

        [JsonPropertyName("blockchainAssetId")]
        public string BlockchainAssetId { get; set; }

        [JsonPropertyName("walletAddress")]
        public string WalletAddress { get; set; }

        [JsonPropertyName("skip")]
        public int Skip { get; set; }

        [JsonPropertyName("take")]
        public int Take { get; set; }
    }

    public class GetOperationsResponse
    {
        [JsonPropertyName("operations")]
        public OperationResponse[] Operations { get; set; }
    }

    public class OperationResponse
    {
        [JsonPropertyName("blockchainId")]
        public string BlockchainId { get; set; }

        [JsonPropertyName("blockchainAssetId")]
        public string BlockchainAssetId { get; set; }

        [JsonPropertyName("walletAddress")]
        public string WalletAddress { get; set; }

        [JsonPropertyName("operationId")]
        public long OperationId { get; set; }

        [JsonPropertyName("block")]
        public long Block { get; set; }

        [JsonPropertyName("balanceChange")]
        public string BalanceChange { get; set; }
    }
}