namespace Service.WalletManager.Config
{
    public class WalletManagerConfig
    {
        public DbConfig Db { get; set; }

        public string SeqUrl { get; set; }

        public BlockchainSettings BlockchainSettings { get; set; }
    }

    public class DbConfig
    {
        public string ConnectionString { get; set; }
    }

    public class BlockchainSettings
    {
        public string BlockchainSignFacadeUrl { get; set; }

        public Blockchain[] Blockchains { get; set; }
    }

    public class Blockchain
    {
        public string BlockchainId { get; set; }
        public string BlockchainApi { get; set; }
    }
}
