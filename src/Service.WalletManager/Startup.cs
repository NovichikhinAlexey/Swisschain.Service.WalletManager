using System.Collections.Generic;
using System.Collections.ObjectModel;
using Autofac;
using Lykke.Service.BlockchainApi.Client;
using Lykke.Service.BlockchainApi.Client.Models;
using Lykke.Service.BlockchainApi.Contract.Assets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Service.WalletManager.Config;
using Service.WalletManager.Domain.Repositories;
using Service.WalletManager.Domain.Services;
using Service.WalletManager.DomainServices;
using Service.WalletManager.HostedServices;
using Service.WalletManager.Repositories;
using Service.WalletManager.Repositories.DbContexts;
using Service.WalletManager.Services;
using Swisschain.Sdk.Server.Common;

namespace Service.WalletManager
{
    public class Startup : SwisschainStartup<WalletManagerConfig>
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void RegisterEndpoints(IEndpointRouteBuilder endpoints)
        {
            base.RegisterEndpoints(endpoints);

            endpoints.MapGrpcService<MonitoringService>();

            endpoints.MapGrpcService<WalletGrpcService>();

            endpoints.MapGrpcService<BalanceGrpcService>();

            endpoints.MapGrpcService<OperationGrpcService>();

            endpoints.MapGrpcService<TransferGrpcService>();
        }

        protected override void ConfigureContainerExt(ContainerBuilder builder)
        {
            var walletManagerConfig = this.Config;

            #region Repositories

            builder
                .Register(x =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder<WalletManagerContext>();
                    optionsBuilder.UseNpgsql(walletManagerConfig.Db.ConnectionString);

                    return optionsBuilder;
                })
                .As<DbContextOptionsBuilder<WalletManagerContext>>()
                .SingleInstance();

            builder.RegisterType<EnrolledBalanceRepository>()
                .As<IEnrolledBalanceRepository>()
                .SingleInstance();

            builder.RegisterType<InMemoryDecoratorEnrolledBalanceRepository>()
                .As<IStartable>()
                .SingleInstance()
                .AutoActivate();

            builder.RegisterType<OperationRepository>()
                .As<IOperationRepository>()
                .SingleInstance();

            builder.RegisterDecorator<EnrolledBalanceRepository, IEnrolledBalanceRepository>();
            builder.RegisterDecorator<InMemoryDecoratorEnrolledBalanceRepository, IEnrolledBalanceRepository>();

            #endregion

            #region Services

            builder.RegisterType<AssetService>()
                .SingleInstance();

            builder.RegisterType<TransferService>()
                .As<ITransferService>()
                .SingleInstance();

            builder.RegisterType<WalletService>()
                .As<IWalletService>()
                .SingleInstance();

            builder.RegisterType<BlockchainApiClientProvider>()
                .As<IBlockchainApiClientProvider>()
                .SingleInstance();

            #endregion

            builder
                .Register(x => walletManagerConfig)
                .As<WalletManagerConfig>()
                .SingleInstance();

            foreach (var blockchain in walletManagerConfig.BlockchainSettings.Blockchains)
            {
                builder.RegisterType<BlockchainApiClient>()
                    .Named<IBlockchainApiClient>(blockchain.BlockchainId)
                    .WithParameter(TypedParameter.From(blockchain.BlockchainApi))
                    .SingleInstance()
                    .AutoActivate();

                builder.Register(factory =>
                    {
                        var logger = factory.Resolve<ILogger<BalanceReadingHostedService>>();
                        var logger2 = factory.Resolve<ILogger<BalanceProcessorService>>();
                        var blockchainApiProvider = factory.Resolve<IBlockchainApiClientProvider>();
                        var balanceRepository = factory.Resolve<IEnrolledBalanceRepository>();
                        var operationRepository = factory.Resolve<IOperationRepository>();
                        var client = blockchainApiProvider.Get(blockchain.BlockchainId);
                        var blockchainAssetsDict = client
                            .GetAllAssetsAsync(100)
                            .GetAwaiter()
                            .GetResult();
                        var balanceProcessorService = new BalanceProcessorService(
                            blockchain.BlockchainId,
                            logger2,
                            client,
                            balanceRepository,
                            operationRepository,
                            blockchainAssetsDict);

                        return new BalanceReadingHostedService(logger, balanceProcessorService);
                    })
                    .SingleInstance()
                    .As<IStartable>()
                    .AutoActivate();
            }
        }

        protected override void ConfigureServicesExt(IServiceCollection services)
        {
            var walletManagerConfig = this.Config;

            services.AddDbContext<WalletManagerContext>(
                options =>
                {
                    options.UseNpgsql(walletManagerConfig.Db.ConnectionString);
                });

            services.AddMemoryCache();
            //Should be here
            var options = services.BuildServiceProvider().GetRequiredService<DbContextOptions<WalletManagerContext>>();

            using (var context = new WalletManagerContext(options))
            {
                context.Database.Migrate();
            }
        }
    }
}
