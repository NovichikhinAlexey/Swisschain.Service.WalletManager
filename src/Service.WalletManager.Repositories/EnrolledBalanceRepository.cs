using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Service.WalletManager.Domain.Models;
using Service.WalletManager.Domain.Repositories;
using Service.WalletManager.Repositories.DbContexts;
using Service.WalletManager.Repositories.Entities;

namespace Service.WalletManager.Repositories
{
    public class EnrolledBalanceRepository : IEnrolledBalanceRepository
    {
        private readonly DbContextOptionsBuilder<WalletManagerContext> _dbContextOptionsBuilder;

        public EnrolledBalanceRepository(DbContextOptionsBuilder<WalletManagerContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<IEnumerable<EnrolledBalance>> GetAsync(IEnumerable<DepositWalletKey> keys)
        {
            var list = new List<EnrolledBalance>();

            using (var context = new WalletManagerContext(_dbContextOptionsBuilder.Options))
            {
                foreach (var key in keys)
                {
                    var result = await context
                        .EnrolledBalances
                        .FindAsync(key.BlockchainId, key.BlockchainAssetId,
                            key.DepositWalletAddress);

                    list.Add(MapFromEntity(result));
                }
            }

            return list;
        }

        public async Task SetBalanceAsync(DepositWalletKey key, BigInteger balance, long balanceBlock)
        {
            using (var context = new WalletManagerContext(_dbContextOptionsBuilder.Options))
            {
                var existing = await context.EnrolledBalances.FindAsync(key.BlockchainId, key.BlockchainAssetId,
                    key.DepositWalletAddress);

                if (existing != null)
                    return;

                var newEntity = new EnrolledBalanceEntity()
                {
                    BlockchianId = key.BlockchainId,
                    BlockchainAssetId = key.BlockchainAssetId,
                    WalletAddress = key.DepositWalletAddress,
                    Balance = balance.ToString(),
                    BlockNumber = balanceBlock
                };

                context.EnrolledBalances.Add(newEntity);

                await context.SaveChangesAsync();
            }
        }

        public async Task ResetBalanceAsync(DepositWalletKey key, long transactionBlock)
        {
            using (var context = new WalletManagerContext(_dbContextOptionsBuilder.Options))
            {
                var existing = await context.EnrolledBalances.FindAsync(key.BlockchainId, key.BlockchainAssetId,
                    key.DepositWalletAddress);

                if (existing != null)
                {
                    context.EnrolledBalances.Update(existing);
                }
                else
                {
                    var newEntity = new EnrolledBalanceEntity()
                    {
                        BlockchianId = key.BlockchainId,
                        BlockchainAssetId = key.BlockchainAssetId,
                        WalletAddress = key.DepositWalletAddress,
                        Balance = "0",
                        BlockNumber = transactionBlock
                    };

                    context.EnrolledBalances.Add(newEntity);
                }

                await context.SaveChangesAsync();
            }
        }

        public async Task<EnrolledBalance> TryGetAsync(DepositWalletKey key)
        {
            using (var context = new WalletManagerContext(_dbContextOptionsBuilder.Options))
            {
                var result = await context.EnrolledBalances.FindAsync(
                    key.BlockchainId, key.BlockchainAssetId, key.DepositWalletAddress);
                var mapped = MapFromEntity(result);

                return mapped;
            }
        }

        public async Task<IEnumerable<EnrolledBalance>> GetAllAsync(int skip, int count)
        {
            using (var context = new WalletManagerContext(_dbContextOptionsBuilder.Options))
            {
                var result = context
                    .EnrolledBalances
                    .Skip(skip)
                    .Take(count)
                    .Select(MapFromEntity);

                return result;
            }
        }

        private static EnrolledBalance MapFromEntity(EnrolledBalanceEntity entity)
        {
            BigInteger.TryParse(entity.Balance, out var balance);
            return EnrolledBalance.Create(
                new DepositWalletKey(entity.BlockchainAssetId, entity.BlockchianId, entity.WalletAddress),
                balance,
                entity.BlockNumber);
        }
    }
}
