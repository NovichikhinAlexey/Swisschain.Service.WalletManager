using System.Numerics;

namespace Service.WalletManager.Domain.Models
{
    public sealed class EnrolledBalance
    {
        public DepositWalletKey Key { get; }
        public BigInteger Balance { get; set; }
        public long Block { get; set; }

        private EnrolledBalance(
            DepositWalletKey key,
            BigInteger balance,
            long block)
        {
            Balance = balance;
            Key = key;
            Block = block;
        }

        public static EnrolledBalance Create(DepositWalletKey key, BigInteger balance, long block)
        {
            return new EnrolledBalance(key, balance, block);
        }
    }
}
