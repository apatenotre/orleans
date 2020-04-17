using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Messaging;

namespace Orleans.AzureUtils
{
    internal class AzureGatewayListProvider : IGatewayListProvider
    {
        private OrleansSiloInstanceManager siloInstanceManager;
        private readonly string clusterId;
        private readonly AzureStorageGatewayOptions options;
        private readonly ILoggerFactory loggerFactory;
        private readonly TimeSpan maxStaleness;

        public AzureGatewayListProvider(ILoggerFactory loggerFactory, IOptions<AzureStorageGatewayOptions> options, IOptions<ClusterOptions> clusterOptions, IOptions<GatewayOptions> gatewayOptions)
        {
            this.loggerFactory = loggerFactory;
            this.clusterId = clusterOptions.Value.ClusterId;
            this.maxStaleness = gatewayOptions.Value.GatewayListRefreshPeriod;
            this.options = options.Value;
        }

        public async Task InitializeGatewayListProvider()
        {
            siloInstanceManager = await OrleansSiloInstanceManager.GetManager(
                this.clusterId, this.options.ConnectionString, this.options.TableName, this.loggerFactory);
        }
        // no caching
        public Task<IList<Uri>> GetGateways()
        {
            // FindAllGatewayProxyEndpoints already returns a deep copied List<Uri>.
            return siloInstanceManager.FindAllGatewayProxyEndpoints();
        }

        public TimeSpan MaxStaleness
        {
            get { return this.maxStaleness; }
        }

        public bool IsUpdatable
        {
            get { return true; }
        }
    }
}
