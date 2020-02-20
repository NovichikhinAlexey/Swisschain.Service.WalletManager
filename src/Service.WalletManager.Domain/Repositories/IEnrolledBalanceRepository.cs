using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Service.WalletManager.Domain.Models;

namespace Service.WalletManager.Domain.Repositories
{
    public interface IEnrolledBalanceRepository
    {
        Task<IEnumerable<EnrolledBalance>> GetAsync(IEnumerable<DepositWalletKey> keys);

        Task SetBalanceAsync(DepositWalletKey key, BigInteger balance, long balanceBlock);

        Task ResetBalanceAsync(DepositWalletKey key, long transactionBlock);

        Task DeleteBalanceAsync(DepositWalletKey key);

        Task<EnrolledBalance> TryGetAsync(DepositWalletKey key);

        Task<IEnumerable<EnrolledBalance>> GetAllAsync(int skip, int count);
    }
}
