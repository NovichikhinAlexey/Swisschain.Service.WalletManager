using System;
using Autofac.Features.Indexed;
using Lykke.Service.BlockchainApi.Client;
using Service.WalletManager.Domain.Services;

namespace Service.WalletManager.DomainServices
{
    public class BlockchainApiClientProvider : IBlockchainApiClientProvider
    {
        private readonly IIndex<string, IBlockchainApiClient> _clients;

        public BlockchainApiClientProvider(IIndex<string, IBlockchainApiClient> clients)
        {
            _clients = clients;
        }

        public IBlockchainApiClient Get(string blockchainType)
        {
            if (!_clients.TryGetValue(blockchainType, out var client))
            {
                throw new InvalidOperationException($"Blockchain API client [{blockchainType}] is not found");
            }

            return client;
        }
    }
}
