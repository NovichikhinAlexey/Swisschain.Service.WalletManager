using System;
using System.Collections.Generic;
using System.Text;

namespace Service.WalletManager.Repositories
{
    public class PostgresRepositoryConfiguration
    {
        public static string SchemaName { get; } = "wallet_manager";

        public static string MigrationHistoryTable { get; } = "__EFMigrationsHistory_wallet_manager";
    }
}
