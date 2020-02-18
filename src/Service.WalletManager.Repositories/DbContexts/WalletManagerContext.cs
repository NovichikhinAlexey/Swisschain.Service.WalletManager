using Microsoft.EntityFrameworkCore;
using Service.WalletManager.Repositories.Entities;

namespace Service.WalletManager.Repositories.DbContexts
{
    public class WalletManagerContext : DbContext
    {
        public WalletManagerContext(DbContextOptions<WalletManagerContext> options) :
            base(options)
        {
        }

        public DbSet<EnrolledBalanceEntity> EnrolledBalances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("wallet_manager");
            modelBuilder.Entity<EnrolledBalanceEntity>()
                .HasKey(c => new { c.BlockchianId, c.BlockchainAssetId, c.WalletAddress });

            base.OnModelCreating(modelBuilder);
        }
    }
}
