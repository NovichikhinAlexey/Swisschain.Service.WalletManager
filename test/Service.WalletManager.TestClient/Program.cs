using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.BlockchainSignFacade.Client;
using Lykke.Service.BlockchainSignFacade.Contract.Models;
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
            var signFacadeClient = new BlockchainSignFacadeClient("http://blockchain-sign-facade.services.svc.cluster.local/",
                "350AFDCE-A027-4843-935F-EF5C377CE2EB", new EmptyLog());
            var senderWallet = await signFacadeClient.CreateWalletAsync("Bitcoin");
            var receiverWallet = await signFacadeClient.CreateWalletAsync("Bitcoin");

            var walletKey = new WalletKey()
            {
                BlockchainAssetId = "BTC",
                BlockchainId = "Bitcoin",
                WalletAddress = senderWallet.PublicAddress
            };

            var task = client.Wallets.RegisterWalletAsync(new RegisterWalletRequest()
            {
                WalletKey = walletKey
            }).ResponseAsync.ContinueWith(async
                (prev) =>
            {
                await prev;

                var operationId = Guid.NewGuid().ToString();
                var builtTransaction = await client.Transfers.BuildTransactionAsync(new BuildTransactionRequest()
                {
                    OperationId = operationId,
                    BlockchainAssetId = "BTC",
                    BlockchainId = "Bitcoin",
                    FromAddress = senderWallet.PublicAddress,
                    FromAddressContext = senderWallet.AddressContext,
                    Amount = "1000000",
                    ToAddress = receiverWallet.PublicAddress,
                    IncludeFee = true
                });

                var signedTransaction =
                    await signFacadeClient.SignTransactionAsync("Bitcoin",
                        new SignTransactionRequest()
                        {
                            PublicAddresses = new[] { senderWallet.PublicAddress },
                            TransactionContext = builtTransaction.TransactionContext
                        });

                var sendTransactionResult = await client.Transfers.BroadcastTransactionAsync(new BroadcastTransactionRequest()
                {
                    BlockchainId = "Bitcoin",
                    SignedTransaction = signedTransaction.SignedTransaction,
                    OperationId = operationId
                });

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
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                await Task.Delay(1000);
            }
        }
    }
}
