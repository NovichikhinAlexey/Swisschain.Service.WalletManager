using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Service.WalletManager.Repositories.DbContexts;

namespace Service.WalletManager.Repositories.DesignTime
{
    public class WalletManagerContextFactory : IDesignTimeDbContextFactory<WalletManagerContext>
    {
        public WalletManagerContext CreateDbContext(string[] args)
        {
            var connString = Environment.GetEnvironmentVariable("POSTGRE_SQL_CONNECTION_STRING");

            var optionsBuilder = new DbContextOptionsBuilder<WalletManagerContext>();
            optionsBuilder.UseNpgsql(connString);

            return new WalletManagerContext(optionsBuilder.Options);
        }
    }
}
