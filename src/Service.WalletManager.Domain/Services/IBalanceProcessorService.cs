using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Service.WalletManager.Domain.Services
{
    public interface IBalanceProcessorService
    {
        Task ProcessAsync(int batch);
    }
}
