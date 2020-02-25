using Service.WalletManager.Client.Common;
using Service.WalletManager.Protos;

namespace Service.WalletManager.Client
{
    public class WalletManagerClient : BaseGrpcClient, IWalletManagerClient
    {
        public WalletManagerClient(string serverGrpcUrl) : base(serverGrpcUrl)
        {
            Monitoring = new Monitoring.MonitoringClient(Channel);
            Wallets= new Wallets.WalletsClient(Channel);
            Balances = new Balances.BalancesClient(Channel);
            Operations = new Operations.OperationsClient(Channel);
            Transfers = new Transfers.TransfersClient(Channel);
        }

        public Monitoring.MonitoringClient Monitoring { get; }

        public Wallets.WalletsClient Wallets { get; }

        public Operations.OperationsClient Operations { get; }

        public Balances.BalancesClient Balances { get; }

        public Transfers.TransfersClient Transfers { get; }
    }
}
