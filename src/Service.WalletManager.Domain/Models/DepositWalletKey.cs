﻿using System;
using System.Globalization;
using JetBrains.Annotations;

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
            WalletAddress = depositWalletAddress;
        }

        public override bool Equals([CanBeNull] object obj)
        {
            if (obj is DepositWalletKey depositWalletKey)
            {
                return this.BlockchainAssetId == depositWalletKey.BlockchainAssetId &&
                       this.BlockchainId == depositWalletKey.BlockchainId &&
                       this.WalletAddress.Equals(depositWalletKey.WalletAddress, StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.BlockchainAssetId.GetHashCode() + 
                   this.BlockchainId.GetHashCode() +
                   this.WalletAddress.ToLower(CultureInfo.InvariantCulture).GetHashCode();
        }
    }
}
