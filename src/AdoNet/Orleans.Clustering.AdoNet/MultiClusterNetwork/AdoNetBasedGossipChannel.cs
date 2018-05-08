using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Clustering.AdoNet.Storage;
using Orleans.Configuration;
using Orleans.MultiCluster;
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
        private string _serviceId;


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
            _serviceId = serviceId;
            _logger.Info("Initializing Gossip Channel for ServiceId={0} using connection: {1}",
                serviceId, ConfigUtilities.RedactConnectionStringInfo(connectionString));

            _orleansQueries = await RelationalOrleansQueries.CreateInstance(_clusteringTableOptions.Invariant, _clusteringTableOptions.ConnectionString, _grainReferenceConverter);
        }

        public async Task Publish(IMultiClusterGossipData data)
        {
            _logger.Debug("-Publish data:{0}", data);
            // this is (almost) always called with just one item in data to be written back
            // so we are o.k. with doing individual tasks for each storage read and write

            if (data.Configuration != null)
            {
                var currentConfig = await _orleansQueries.GossipConfigurationReadAsync(_serviceId);
                await DiffAndWriteBackConfigAsync(data.Configuration, currentConfig);
            }
            foreach (var gateway in data.Gateways.Values)
            {
                var gatewayInfoInStorage = await _orleansQueries.ReadGossipGatewayEntryAsync(gateway);
                await DiffAndWriteBackGatewayInfoAsync(gateway, gatewayInfoInStorage);
            }
        }

        private async Task<MultiClusterConfiguration> DiffAndWriteBackConfigAsync(MultiClusterConfiguration dataConfiguration, GossipConfiguration currentConfig)
        {
            // interpret empty admin timestamp by taking the azure table timestamp instead
            // this allows an admin to inject a configuration by editing table directly
            if (currentConfig != null && currentConfig.GossipTimestamp == default(DateTime))
                currentConfig.GossipTimestamp = currentConfig.TimeStamp;

            if (dataConfiguration != null &&
                (currentConfig == null || currentConfig.GossipTimestamp < dataConfiguration.AdminTimestamp))
            {
                // push the more recent configuration to storage
                if (currentConfig == null)
                    await _orleansQueries.TryCreateGossipConfigurationEntryAsync(_serviceId, dataConfiguration);
                else
                    await _orleansQueries.TryUpdateGossipConfigurationEntryAsync(dataConfiguration, currentConfig, currentConfig.Version + 1);
            }
            else if (currentConfig != null && (dataConfiguration == null || dataConfiguration.AdminTimestamp < currentConfig.GossipTimestamp))
            {
                // pull the more recent configuration from storage
                return currentConfig.ToConfiguration();
            }

            return null;
        }

        // compare gatewayInfo with gatewayInfoInStorage, and
        // - write gatewayInfo to storage if it is newer (or do nothing on version conflict)
        // - remove expired gateway info from storage
        // - return gatewayInfoInStorage if it is newer
        internal async Task<GatewayEntry> DiffAndWriteBackGatewayInfoAsync(GatewayEntry gatewayInfo, GossipGateway currentGatewayConfig)
        {
            if (gatewayInfo != null && !gatewayInfo.Expired && (currentGatewayConfig == null || currentGatewayConfig.GossipTimestamp < gatewayInfo.HeartbeatTimestamp))
            {
                // push  the more recent gateway info to storage
                if (currentGatewayConfig == null)
                {
                    await _orleansQueries.TryCreateGossipGatewayEntryAsync(_serviceId, gatewayInfo);
                }
                else
                {
                    await _orleansQueries.TryUpdateGossipGatewayEntryAsync(gatewayInfo, currentGatewayConfig, currentGatewayConfig.Version + 1);
                }
            }
            else if (currentGatewayConfig != null && (gatewayInfo == null || gatewayInfo.HeartbeatTimestamp < currentGatewayConfig.GossipTimestamp))
            {
                var fromstorage = currentGatewayConfig.ToGatewayEntry();
                if (fromstorage.Expired)
                {
                    // remove gateway info from storage
                    await _orleansQueries.TryDeleteGossipGatewayEntryAsync(currentGatewayConfig, currentGatewayConfig.Version);
                }
                else
                {
                    // pull the more recent info from storage
                    return fromstorage;
                }
            }
            return null;
        }

        public Task<IMultiClusterGossipData> Synchronize(IMultiClusterGossipData pushed)
        {
            _logger.Debug("-Synchronize pushed:{0}", pushed);

            throw new NotImplementedException();
        }
    }
}
