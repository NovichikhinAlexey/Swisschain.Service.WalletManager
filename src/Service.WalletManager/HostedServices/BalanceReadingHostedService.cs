using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using Service.WalletManager.Domain.Services;

namespace Service.WalletManager.HostedServices
{
    public class BalanceReadingHostedService : IStartable, IDisposable
    {
        private readonly ILogger<BalanceReadingHostedService> _logger;
        private readonly IBalanceProcessorService _balanceProcessorService;
        private Task _backgroundWorker;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public BalanceReadingHostedService(
            ILogger<BalanceReadingHostedService> logger,
            IBalanceProcessorService balanceProcessorService)
        {
            _logger = logger;
            _balanceProcessorService = balanceProcessorService;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BalanceReadingHostedService running.");

            _backgroundWorker = Task.Run(async () =>
            {
                do
                {
                    await DoWorkAsync();

                    await Task.Delay(30_000);
                } while (!stoppingToken.IsCancellationRequested);
            }, stoppingToken);

            return Task.CompletedTask;
        }

        private async Task DoWorkAsync()
        {
            _logger.LogInformation(
                "BalanceReadingHostedService is working.");

            try
            {
                await _balanceProcessorService.ProcessAsync(100);
            }
            catch (Exception e)
            {
                _logger.LogError(
                    "BalanceReadingHostedService encountered an unexpected error while reading balances.");
            }

            _logger.LogInformation(
                "BalanceReadingHostedService finished reading balances.");
        }

        public async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BalanceReadingHostedService is stopping.");

            try
            {
                await _backgroundWorker;
            }
            catch (Exception e)
            {
            }
        }

        public void Dispose()
        {
            try
            {
                _cancellationTokenSource.Cancel();
            }
            catch (Exception e)
            {
            }
        }

        public void Start()
        {
            StartAsync(_cancellationTokenSource.Token).Wait();
        }
    }
}
