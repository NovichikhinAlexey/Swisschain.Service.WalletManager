using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Service.WalletManager.Controllers.Balance;
using Service.WalletManager.Domain.Models;
using Service.WalletManager.Domain.Repositories;

namespace Service.WalletManager.Controllers.Operation
{
    [ApiController]
    [Route("api/[controller]")]
    public class OperationController : ControllerBase
    {
        private readonly IOperationRepository _operationRepository;

        public OperationController(IOperationRepository operationRepository)
        {
            _operationRepository = operationRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetOperationsAsync([FromQuery]GetOperationsRequest request)
        {
            var key = new DepositWalletKey(request.BlockchainAssetId, request.BlockchainId, request.WalletAddress);
            var operations = await _operationRepository.GetAsync(key, request.Skip, request.Take);

            return Ok(new GetOperationsResponse()
            {
                Operations = operations?.Select(x => new OperationResponse()
                {
                    BlockchainAssetId = x.Key.BlockchainAssetId,
                    Block = x.Block,
                    BlockchainId = x.Key.BlockchainId,
                    BalanceChange = x.BalanceChange.ToString(),
                    OperationId = x.OperationId,
                    WalletAddress = x.Key.WalletAddress,
                }).ToArray()
            });
        }
    }
}