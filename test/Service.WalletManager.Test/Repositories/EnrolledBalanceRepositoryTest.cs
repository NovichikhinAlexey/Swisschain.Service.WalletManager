using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Service.WalletManager.Domain.Models;
using Service.WalletManager.Domain.Repositories;
using Service.WalletManager.Repositories;
using Service.WalletManager.Repositories.DbContexts;

namespace Service.WalletManager.Test.Repositories
{
    public class EnrolledBalanceRepositoryTest
    {
        private readonly string _connectionString;

        public EnrolledBalanceRepositoryTest()
        {
            this._connectionString = Environment.GetEnvironmentVariable("POSTGRE_SQL_CONNECTION_STRING");
        }

        [Fact(Skip = "skip for now")]
        public async Task Test1()
        {
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();
            builder
                .Register(x =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder<WalletManagerContext>();
                    optionsBuilder.UseNpgsql(_connectionString);

                    return optionsBuilder;
                })
                .As<DbContextOptionsBuilder<WalletManagerContext>>()
                .SingleInstance();

            builder.RegisterType<EnrolledBalanceRepository>()
                .As<IEnrolledBalanceRepository>()
                .SingleInstance();

            builder.RegisterDecorator<EnrolledBalanceRepository, IEnrolledBalanceRepository>();
            builder.RegisterDecorator<InMemoryDecoratorEnrolledBalanceRepository, IEnrolledBalanceRepository>();
            services.AddMemoryCache();

            builder.Populate(services);
            var container = builder.Build();

            var cache = container.Resolve<IMemoryCache>();
            var balanceRepository = container.Resolve<IEnrolledBalanceRepository>();

            var key = new DepositWalletKey(
                "BTC",
                "Bitcoin",
                "2NEMnuF2wg2rn225sRnwEtMjsen6wnrqM9a");

            //var mig = await context.Database.EnsureCreatedAsync();
            //await context.Database.MigrateAsync();
            await balanceRepository.SetBalanceAsync(
                    key,
                    BigInteger.One,
                    1);

            var received = await balanceRepository.TryGetAsync(key);

            Assert.Equal(1, received.Block);
            Assert.Equal(BigInteger.One, received.Balance);

            var receivedByArray = await balanceRepository.GetAsync(new DepositWalletKey[] { key });

            Assert.NotNull(receivedByArray);
            Assert.True(receivedByArray.Count() == 1);

            received = receivedByArray.First();

            Assert.Equal(1, received.Block);
            Assert.Equal(BigInteger.One, received.Balance);

            await balanceRepository.ResetBalanceAsync(key, 2);

            received = await balanceRepository.TryGetAsync(key);

            Assert.Equal(2, received.Block);
            Assert.Equal(BigInteger.Zero, received.Balance);
        }
    }
}
