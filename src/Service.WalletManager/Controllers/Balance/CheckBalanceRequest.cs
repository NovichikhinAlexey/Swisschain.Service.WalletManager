using System.Numerics;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Service.WalletManager.Controllers.Balance
{
    [BindProperties(SupportsGet = true)]
    public class CheckBalanceRequest
    {
        [JsonPropertyName("blockchainId")]
        public string BlockchainId { get; set; }

        [JsonPropertyName("blockchainAssetId")]
        public string BlockchainAssetId { get; set; }

        [JsonPropertyName("walletAddress")]
        public string WalletAddress { get; set; }
    }

    public class CheckBalanceResponse
    {
        [JsonPropertyName("balance")]
        public string Balance { get; set; }

        [JsonPropertyName("block")]
        public long Block { get; set; }
    }
}