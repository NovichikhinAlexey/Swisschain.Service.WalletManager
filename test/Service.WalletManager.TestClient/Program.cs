using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Service.WalletManager.Client;
using Service.WalletManager.Protos;

namespace Service.WalletManager.TestClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Press enter to start");
            Console.ReadLine();
            var client = new WalletManagerClient("http://localhost:5001");
            var walletKey = new WalletKey()
            {
                BlockchainAssetId = "BTC",
                BlockchainId = "Bitcoin",
                WalletAddress = "2NEZP81rD5VhqexqoWk1Hubh3QcHiNJCzCR"
            };

            var task = client.Wallets.RegisterWalletAsync(new RegisterWalletRequest()
            {
                WalletKey = walletKey
            }).ResponseAsync.ContinueWith(async
                (prev) =>
            {
                await prev;

                var operations = await client.Operations.GetOperationsAsync(new GetOperationRequest()
                {
                    WalletKey = walletKey, 
                    Skip = 0,
                    Take = 100
                });

                var balance = await client.Balances.GetBalanceAsync(new GetBalanceRequest()
                {
                    WalletKey = walletKey,
                });
            });

            while (true)
            {
                try
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    var result = await client.Monitoring.IsAliveAsync(new IsAliveRequest());
                    sw.Stop();
                    Console.WriteLine($"{result.Name}  {sw.ElapsedMilliseconds} ms");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                await Task.Delay(1000);
            }
        }
    }
}
