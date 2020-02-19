using System.Globalization;

namespace Service.WalletManager.Domain.Models
{
    public sealed class DepositWalletKey
    {
        public string BlockchainAssetId { get; }
        public string BlockchainId { get; }
        public string WalletAddress { get; }

        public DepositWalletKey(string blockchainAssetId, string blockchainId, string depositWalletAddress)
        {
            BlockchainAssetId = blockchainAssetId;
            BlockchainId = blockchainId;
            WalletAddress = depositWalletAddress?.ToLower(CultureInfo.InvariantCulture);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is DepositWalletKey depositWalletKey))
                return false;


            return this.BlockchainAssetId == depositWalletKey.BlockchainAssetId &&
                   this.BlockchainId == depositWalletKey.BlockchainId &&
                   this.WalletAddress == depositWalletKey.WalletAddress;
        }

        public override int GetHashCode()
        {
            return this.BlockchainAssetId.GetHashCode() + 
                   this.BlockchainId.GetHashCode() +
                   this.WalletAddress.GetHashCode();
        }
    }
}
