using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Lykke.Service.BlockchainApi.Client;

namespace Service.WalletManager.Services
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
