using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using Swisschain.Sdk.Server.Common;
using Service.WalletManager.Protos;

namespace Service.WalletManager.Services
{
    public class MonitoringService : Monitoring.MonitoringBase
    {
        public override Task<IsAliveResponse> IsAlive(IsAliveRequest request, ServerCallContext context)
        {
            var name = Assembly.GetEntryAssembly()?.GetName();
            var result = new IsAliveResponse()
            {
                Name = ApplicationInformation.AppName,
                Version = ApplicationInformation.AppVersion,
                StartedAt = ApplicationInformation.StartedAt.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return Task.FromResult(result);
        }
    }
}