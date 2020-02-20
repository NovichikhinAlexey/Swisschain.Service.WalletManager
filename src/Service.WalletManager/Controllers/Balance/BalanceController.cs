using System.Linq;
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
        public async Task<IActionResult> GetBalanceAsync([FromQuery]CheckBalanceRequest request)
        {
            var key = new DepositWalletKey(request.BlockchainAssetId, request.BlockchainId, request.WalletAddress);
            var balance = await _balanceRepository.TryGetAsync(key);

            return Ok(new CheckBalanceResponse()
            {
                BlockchainAssetId = key.BlockchainAssetId,
                BlockchainId = key.BlockchainId,
                WalletAddress = key.WalletAddress, 
                Block = balance?.Block ?? 0,
                Balance = balance?.Balance.ToString() ?? "0"
            });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllBalanceAsync([FromQuery]int skip, [FromQuery]int take)
        {
            var balances = await _balanceRepository.GetAllAsync(skip, take);

            return Ok(new BalanceResponses()
            {
               Balances = balances?.Select(x => new CheckBalanceResponse()
               {
                   BlockchainAssetId = x.Key.BlockchainAssetId,
                   BlockchainId = x.Key.BlockchainId,
                   Block = x.Block,
                   WalletAddress = x.Key.WalletAddress,
                   Balance = x.Balance.ToString()
               }).ToArray()
            });
        }
    }
}
