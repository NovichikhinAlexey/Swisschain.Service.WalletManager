using System.Text.Json.Serialization;

namespace Service.WalletManager.Controllers.Wallet
{
    public class RegisterWalletRequest
    {
        [JsonPropertyName("blockchainId")]
        public string BlockchainId { get; set; }

        [JsonPropertyName("walletAddress")]
        public string WalletAddress { get; set; }
    }
}