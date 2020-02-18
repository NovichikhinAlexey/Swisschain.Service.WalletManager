using Service.WalletManager.Protos;

namespace Service.WalletManager.Client
{
    public interface IWalletManagerClient
    {
        Monitoring.MonitoringClient Monitoring { get; }
    }
}