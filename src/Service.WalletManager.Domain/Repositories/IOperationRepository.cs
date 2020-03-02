using System.Collections.Generic;
using System.Threading.Tasks;
using Service.WalletManager.Domain.Models;

namespace Service.WalletManager.Domain.Repositories
{
    public interface IOperationRepository
    {
        Task SetAsync(CreateOperation operation);

        Task<IEnumerable<Operation>> GetAsync(DepositWalletKey key, int skip, int take);

        Task<IEnumerable<Operation>> GetAsync(string blockchainId, string walletAddress, int skip, int take);

        Task<IEnumerable<Operation>> GetAllForBlockchainAsync(string blockchainId, int skip, int take);
    }
}
