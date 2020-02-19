using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Service.WalletManager.Domain.Models;
using Service.WalletManager.Domain.Repositories;

namespace Service.WalletManager.Controllers.Balance
{
    [ApiController]
    [Route("api/[controller]")]
    public class BalanceController : ControllerBase
    {
        private readonly IEnrolledBalanceRepository _balanceRepository;

        public BalanceController(IEnrolledBalanceRepository balanceRepository)
        {
            _balanceRepository = balanceRepository;
        }

        [HttpGet]
        public async Task<IActionResult> RegisterWalletAsync([FromQuery]CheckBalanceRequest request)
        {
            var key = new DepositWalletKey(request.BlockchainAssetId, request.BlockchainId, request.WalletAddress);
            var balance = await _balanceRepository.TryGetAsync(key);

            return Ok(new CheckBalanceResponse()
            {
                Block = balance?.Block ?? 0,
                Balance = balance?.Balance.ToString() ?? "0"
            });
        }
    }
}