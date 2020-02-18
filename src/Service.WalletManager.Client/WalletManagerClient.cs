using Service.WalletManager.Client.Common;
using Service.WalletManager.Protos;

namespace Service.WalletManager.Client
{
    public class WalletManagerClient : BaseGrpcClient, IWalletManagerClient
    {
        public WalletManagerClient(string serverGrpcUrl) : base(serverGrpcUrl)
        {
            Monitoring = new Monitoring.MonitoringClient(Channel);
        }

        public Monitoring.MonitoringClient Monitoring { get; }
    }
}