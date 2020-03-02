using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service.WalletManager.Repositories.DbContexts;
using Swisschain.Sdk.Server.Common;
using Swisschain.Sdk.Server.Loggin;

namespace Service.WalletManager
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using (var loggerFactory = LogConfigurator.Configure("Service.WalletManager",
                ApplicationEnvironment.Config["SeqUrl"]))
            {
                var logger = loggerFactory.CreateLogger<Program>();

                try
                {
                    logger.LogInformation("Application is being started");

                    var host = CreateHostBuilder(loggerFactory).Build();

                    host.Run();

                    logger.LogInformation("Application has been stopped");
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, "Application has been terminated unexpectedly");
                }
            }
        }

        private static IHostBuilder CreateHostBuilder(ILoggerFactory loggerFactory) =>
            new HostBuilder()
                .SwisschainService<Startup>(options =>
                {
                    options.UseLoggerFactory(loggerFactory);

                    var remoteSettingsUrl = ApplicationEnvironment.Config["RemoteSettingsUrl"];

                    if (remoteSettingsUrl != default)
                    {
                        options.WithWebJsonConfigurationSource(webJsonOptions =>
                        {
                            webJsonOptions.Url = remoteSettingsUrl;
                            webJsonOptions.IsOptional = ApplicationEnvironment.IsDevelopment;
                            webJsonOptions.Version = ApplicationInformation.AppVersion;
                        });
                    }
                });
    }
}
