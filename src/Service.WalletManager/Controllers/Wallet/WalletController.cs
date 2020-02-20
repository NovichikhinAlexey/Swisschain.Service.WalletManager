using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Service.WalletManager.Domain.Models;
using Service.WalletManager.Domain.Services;
using Service.WalletManager.Services;

namespace Service.WalletManager.Controllers.Wallet
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterWalletAsync(RegisterWalletRequest request)
        {
            var key = new DepositWalletKey(
                request.BlockchainId, 
                request.BlockchainAssetId, 
                request.WalletAddress);
            await _walletService.RegisterWalletAsync(key);

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteWalletAsync(RegisterWalletRequest request)
        {
            var key = new DepositWalletKey(
                request.BlockchainId,
                request.BlockchainAssetId,
                request.WalletAddress);
            await _walletService.DeleteWalletAsync(key);

            return Ok();
        }
    }
}
