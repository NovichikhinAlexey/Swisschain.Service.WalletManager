using System.Collections.Generic;
using System.Globalization;
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
                            key.WalletAddress.ToLower(CultureInfo.InvariantCulture));

                    if (result == null)
                        continue;

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
                    key.WalletAddress.ToLower(CultureInfo.InvariantCulture));

                if (existing != null)
                {
                    existing.Balance = balance.ToString();
                    existing.BlockNumber = balanceBlock;

                    context.Update(existing);
                }
                else
                {
                    var newEntity = new EnrolledBalanceEntity()
                    {
                        BlockchianId = key.BlockchainId,
                        BlockchainAssetId = key.BlockchainAssetId,
                        WalletAddress = key.WalletAddress.ToLower(CultureInfo.InvariantCulture),
                        Balance = balance.ToString(),
                        BlockNumber = balanceBlock,
                        OriginalWalletAddress = key.WalletAddress
                    };

                    context.EnrolledBalances.Add(newEntity);
                }

                await context.SaveChangesAsync();
            }
        }

        public async Task ResetBalanceAsync(DepositWalletKey key, long transactionBlock)
        {
            using (var context = new WalletManagerContext(_dbContextOptionsBuilder.Options))
            {
                var existing = await context.EnrolledBalances.FindAsync(key.BlockchainId, key.BlockchainAssetId,
                    key.WalletAddress.ToLower(CultureInfo.InvariantCulture));

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
                        WalletAddress = key.WalletAddress.ToLower(CultureInfo.InvariantCulture),
                        Balance = "0",
                        BlockNumber = transactionBlock,
                        OriginalWalletAddress = key.WalletAddress
                    };

                    context.EnrolledBalances.Add(newEntity);
                }

                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteBalanceAsync(DepositWalletKey key)
        {
            using (var context = new WalletManagerContext(_dbContextOptionsBuilder.Options))
            {
                var result = await context.EnrolledBalances.FindAsync(
                    key.BlockchainId, key.BlockchainAssetId, key.WalletAddress.ToLower(CultureInfo.InvariantCulture));

                if (result != null)
                    context.EnrolledBalances.Remove(result);

                await context.SaveChangesAsync();
            }
        }

        public async Task<EnrolledBalance> TryGetAsync(DepositWalletKey key)
        {
            using (var context = new WalletManagerContext(_dbContextOptionsBuilder.Options))
            {
                var result = await context.EnrolledBalances.FindAsync(
                    key.BlockchainId, key.BlockchainAssetId, key.WalletAddress.ToLower(CultureInfo.InvariantCulture));
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
                    .Take(count);

                await result.LoadAsync();

                return result.Select(MapFromEntity).ToList();
            }
        }

        public async Task<IEnumerable<EnrolledBalance>> GetAllForBlockchainAsync(string blockchainId, int skip, int count)
        {
            using (var context = new WalletManagerContext(_dbContextOptionsBuilder.Options))
            {
                var result = context
                    .EnrolledBalances
                    .Where(x => x.BlockchianId == blockchainId)
                    .Skip(skip)
                    .Take(count);

                await result.LoadAsync();

                return result.Select(MapFromEntity).ToList();
            }
        }

        private static EnrolledBalance MapFromEntity(EnrolledBalanceEntity entity)
        {
            if (entity == null)
                return null;

            BigInteger.TryParse(entity.Balance, out var balance);
            return EnrolledBalance.Create(
                new DepositWalletKey(entity.BlockchainAssetId, entity.BlockchianId, entity.OriginalWalletAddress),
                balance,
                entity.BlockNumber);
        }
    }
}
