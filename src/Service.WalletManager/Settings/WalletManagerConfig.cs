namespace Service.WalletManager.Settings
{
    public class WalletManagerConfig
    {
        public DbConfig Db { get; set; }
    }

    public class DbConfig
    {
        public string ConnectionString { get; set; }
    }
}
