using Autofac;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service.WalletManager.Domain.Repositories;
using Service.WalletManager.HostedServices;
using Service.WalletManager.Repositories;
using Service.WalletManager.Repositories.DbContexts;
using Service.WalletManager.Settings;
using Swisschain.Sdk.Server.Common;

namespace Service.WalletManager
{
    public class Startup : SwisschainStartup
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void RegisterEndpoints(IEndpointRouteBuilder endpoints)
        {
            base.RegisterEndpoints(endpoints);

            //endpoints.MapGrpcService<MonitoringService>();
        }

        protected override void ConfigureContainerExt(ContainerBuilder builder)
        {
            var walletManagerConfig = this.Configuration.Get<WalletManagerConfig>();

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

            builder.RegisterDecorator<EnrolledBalanceRepository, IEnrolledBalanceRepository>();
            builder.RegisterDecorator<InMemoryDecoratorEnrolledBalanceRepository, IEnrolledBalanceRepository>();

            builder
                .Register(x => walletManagerConfig)
                .As<WalletManagerConfig>()
                .SingleInstance();
        }

        protected override void ConfigureServicesExt(IServiceCollection services)
        {
            var walletManagerConfig = this.Configuration.Get<WalletManagerConfig>();

            services.AddDbContext<WalletManagerContext>(
                options =>
                {
                    options.UseNpgsql(walletManagerConfig.Db.ConnectionString);
                });

            services.AddMemoryCache();
            services.AddHostedService<BalanceReadingHostedService>();
        }
    }
}
