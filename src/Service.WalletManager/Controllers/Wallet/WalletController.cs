using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Service.WalletManager.Services;

namespace Service.WalletManager.Controllers.Wallet
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletController : ControllerBase
    {
        private readonly IBlockchainApiClientProvider _blockchainApiClientProvider;

        public WalletController(IBlockchainApiClientProvider blockchainApiClientProvider)
        {
            _blockchainApiClientProvider = blockchainApiClientProvider;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterWalletAsync(RegisterWalletRequest request)
        {
            var client = _blockchainApiClientProvider.Get(request.BlockchainId);
            await client.StartBalanceObservationAsync(request.WalletAddress);

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteWalletAsync(RegisterWalletRequest request)
        {
            var client = _blockchainApiClientProvider.Get(request.BlockchainId);
            await client.StopBalanceObservationAsync(request.WalletAddress);

            return Ok();
        }
    }
}