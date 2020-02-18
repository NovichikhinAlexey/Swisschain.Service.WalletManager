using System;
using System.Collections.Generic;
using System.Text;

namespace Service.WalletManager.Domain.Models
{
    public sealed class DepositWalletKey
    {
        public string BlockchainAssetId { get; }
        public string BlockchainId { get; }
        public string DepositWalletAddress { get; }

        public DepositWalletKey(string blockchainAssetId, string blockchainId, string depositWalletAddress)
        {
            BlockchainAssetId = blockchainAssetId;
            BlockchainId = blockchainId;
            DepositWalletAddress = depositWalletAddress;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is DepositWalletKey depositWalletKey))
                return false;


            return this.BlockchainAssetId == depositWalletKey.BlockchainAssetId &&
                   this.BlockchainId == depositWalletKey.BlockchainId &&
                   this.DepositWalletAddress == depositWalletKey.DepositWalletAddress;
        }

        public override int GetHashCode()
        {
            return this.BlockchainAssetId.GetHashCode() + 
                   this.BlockchainId.GetHashCode() +
                   this.DepositWalletAddress.GetHashCode();
        }
    }
}
