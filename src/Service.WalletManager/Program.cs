using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Swisschain.Sdk.Server.Common;

namespace Service.WalletManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .SwisschainService<Startup>()
                .ConfigureAppConfiguration(builder =>
                {
                    builder
                        .AddJsonFile("appsettings.json")
                        .AddEnvironmentVariables();
                });
    }
}
