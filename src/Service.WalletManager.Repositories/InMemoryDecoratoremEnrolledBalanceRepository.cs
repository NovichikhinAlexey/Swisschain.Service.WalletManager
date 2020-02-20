using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MoreLinq;
using Service.WalletManager.Domain.Models;
using Service.WalletManager.Domain.Repositories;

namespace Service.WalletManager.Repositories
{
    public class InMemoryDecoratorEnrolledBalanceRepository : IEnrolledBalanceRepository, IStartable
    {
        private readonly ILogger<InMemoryDecoratorEnrolledBalanceRepository> _logger;
        private readonly IEnrolledBalanceRepository _enrolledBalanceRepository;
        private readonly IMemoryCache _memCache;

        public InMemoryDecoratorEnrolledBalanceRepository(
            ILogger<InMemoryDecoratorEnrolledBalanceRepository> logger,
            IEnrolledBalanceRepository enrolledBalanceRepository,
            IMemoryCache memCache)
        {
            _logger = logger;
            _enrolledBalanceRepository = enrolledBalanceRepository;
            _memCache = memCache;
        }

        public async Task<IEnumerable<EnrolledBalance>> GetAsync(IEnumerable<DepositWalletKey> keys)
        {
            List<EnrolledBalance> balances = new List<EnrolledBalance>(keys.Count());
            List<DepositWalletKey> askFromDbAgain = new List<DepositWalletKey>(keys.Count());

            foreach (var key in keys)
            {
                if (_memCache.TryGetValue(key, out var balance))
                {
                    balances.Add(balance as EnrolledBalance);
                }
                else
                {
                    askFromDbAgain.Add(key);
                }
            }

            if (askFromDbAgain.Any())
            {
                var fromDb = (await _enrolledBalanceRepository.GetAsync(askFromDbAgain))?.ToList();

                if (fromDb != null && fromDb.Any())
                {
                    fromDb.ForEach((item) =>
                    {
                        using (var entry = _memCache.CreateEntry(item.Key))
                        {
                            entry.Value = item;
                        }
                    });

                    balances.AddRange(fromDb);
                }
            }

            return balances;
        }

        public async Task SetBalanceAsync(DepositWalletKey key, BigInteger balance, long balanceBlock)
        {
            await _enrolledBalanceRepository.SetBalanceAsync(key, balance, balanceBlock);

            if (_memCache.TryGetValue(key, out EnrolledBalance value))
            {
                value.Block = balanceBlock;
                value.Balance = balance;
            }
            else
            {
                using (var entry = _memCache.CreateEntry(key))
                {
                    entry.Value = EnrolledBalance.Create(key, balance, balanceBlock);
                }
            }
        }

        public async Task ResetBalanceAsync(DepositWalletKey key, long transactionBlock)
        {
            await _enrolledBalanceRepository.ResetBalanceAsync(key, transactionBlock);

            if (_memCache.TryGetValue(key, out EnrolledBalance value))
            {
                value.Balance = 0;
                value.Block = transactionBlock;
            }
            else
            {
                using (var entry = _memCache.CreateEntry(key))
                {
                    entry.Value = EnrolledBalance.Create(key, 0, value.Block);
                }
            }
        }

        public async Task DeleteBalanceAsync(DepositWalletKey key)
        {
            _memCache.Remove(key);

            await _enrolledBalanceRepository.DeleteBalanceAsync(key);
        }

        public async Task<EnrolledBalance> TryGetAsync(DepositWalletKey key)
        {
            if (!_memCache.TryGetValue(key, out EnrolledBalance value))
            {
                value = await _enrolledBalanceRepository.TryGetAsync(key);
                using (var entry = _memCache.CreateEntry(key))
                {
                    entry.Value = EnrolledBalance.Create(key, value?.Balance ?? 0, value?.Block ?? 0);
                }
            }

            return value;
        }

        public Task<IEnumerable<EnrolledBalance>> GetAllAsync(int skip, int count)
        {
            return this._enrolledBalanceRepository.GetAllAsync(skip, count);
        }

        public void Start()
        {
            int skip = 0;
            int take = 100;
            int receivedCount = 0;

            try
            {
                do
                {
                    var received = _enrolledBalanceRepository.GetAllAsync(skip, take)
                        .Result?.ToList();

                    if (received == null)
                        break;

                    receivedCount = received.Count();

                    foreach (var item in received)
                    {
                        using (var entry = _memCache.CreateEntry(item.Key))
                        {
                            entry.Value = item;
                            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(48);
                        }
                    }

                    skip += receivedCount;

                } while (receivedCount >= 100);

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error during cache initialization");
            }
        }
    }
}
