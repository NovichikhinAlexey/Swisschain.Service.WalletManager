using Service.WalletManager.Protos;

namespace Service.WalletManager.Client
{
    public interface IWalletManagerClient
    {
        Monitoring.MonitoringClient Monitoring { get; }

        Wallets.WalletsClient Wallets { get; }

        Operations.OperationsClient Operations { get; }

        Balances.BalancesClient Balances { get; }
    }
}
