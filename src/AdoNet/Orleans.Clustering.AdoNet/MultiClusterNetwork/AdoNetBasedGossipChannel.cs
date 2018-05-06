using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Clustering.AdoNet.Storage;
using Orleans.Configuration;
using Orleans.Runtime.Configuration;

namespace Orleans.Runtime.MultiClusterNetwork
{
    internal class AdoNetBasedGossipChannel : IGossipChannel
    {
        private ILogger _logger;
        public string Name { get; }
        private static int _sequenceNumber;
        private RelationalOrleansQueries _orleansQueries;
        private readonly IGrainReferenceConverter _grainReferenceConverter;
        private readonly AdoNetClusteringSiloOptions _clusteringTableOptions;


        public AdoNetBasedGossipChannel(
            ILoggerFactory loggerFactory,
            IOptions<AdoNetClusteringSiloOptions> clusteringOptions,
            RelationalOrleansQueries orleansQueries,
            IGrainReferenceConverter grainReferenceConverter)
        {
            Name = "AdoNetBasedGossipChannel-" + ++_sequenceNumber;
            
            _orleansQueries = orleansQueries;
            _grainReferenceConverter = grainReferenceConverter;
            _logger = loggerFactory.CreateLogger<AdoNetBasedGossipChannel>();
            _clusteringTableOptions = clusteringOptions.Value;
        }

        public async Task Initialize(string serviceId, string connectionString)
        {
            _logger.Info("Initializing Gossip Channel for ServiceId={0} using connection: {1}",
                serviceId, ConfigUtilities.RedactConnectionStringInfo(connectionString));

            _orleansQueries = await RelationalOrleansQueries.CreateInstance(_clusteringTableOptions.Invariant, _clusteringTableOptions.ConnectionString, _grainReferenceConverter);
        }

        public Task Publish(IMultiClusterGossipData data)
        {
            _logger.Debug("-Publish data:{0}", data);
            // this is (almost) always called with just one item in data to be written back
            // so we are o.k. with doing individual tasks for each storage read and write

            if (data.Configuration != null)
            {

            }
            foreach (var gateway in data.Gateways.Values)
            {

            }

            return Task.CompletedTask;
        }

        public Task<IMultiClusterGossipData> Synchronize(IMultiClusterGossipData pushed)
        {
            _logger.Debug("-Synchronize pushed:{0}", pushed);

            throw new NotImplementedException();
        }
    }
}
