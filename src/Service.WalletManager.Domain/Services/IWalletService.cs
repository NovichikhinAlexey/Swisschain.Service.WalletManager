using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Service.WalletManager.Domain.Models;

namespace Service.WalletManager.Domain.Services
{
    public interface IWalletService
    {
        Task RegisterWalletAsync(DepositWalletKey key);
        Task DeleteWalletAsync(DepositWalletKey key);
    }
}
