using System.Numerics;

namespace Service.WalletManager.Domain.Models
{
    public class CreateOperation
    {
        public DepositWalletKey Key { get; }
        public BigInteger BalanceChange { get; set; }
        public long Block { get; set; }

        protected CreateOperation(
            DepositWalletKey key,
            BigInteger balance,
            long block)
        {
            BalanceChange = balance;
            Key = key;
            Block = block;
        }

        public static CreateOperation Create(DepositWalletKey key, BigInteger balanceChange, long block)
        {
            return new CreateOperation(key, balanceChange, block);
        }
    }

    public class Operation : CreateOperation
    {
        public long OperationId { get; set; }
        protected Operation(
            DepositWalletKey key,
            BigInteger balance,
            long block,
            long operationId) : base(key, balance, block)
        {
            OperationId = operationId;
        }

        public static Operation Create(DepositWalletKey key, BigInteger balanceChange, long block, long operationId)
        {
            return new Operation(key, balanceChange, block, operationId);
        }
    }
}
