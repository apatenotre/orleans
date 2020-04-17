using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Clustering.DynamoDB.MultiClusterNetwork;
using Orleans.MultiCluster;
using Orleans.Runtime;
using Orleans.Runtime.MultiClusterNetwork;

namespace DynamoTest
{
    public class Data : IMultiClusterGossipData
    {
        public MultiClusterConfiguration Configuration { get; set; }
        public IReadOnlyDictionary<SiloAddress, GatewayEntry> Gateways { get; set; }
        public bool IsEmpty { get; }
    }

    class Program
    {

        private const string AccessKeyPropertyName = "AccessKey";
        private const string SecretKeyPropertyName = "SecretKey";
        private const string ServicePropertyName = "Service";
        private const string ReadCapacityUnitsPropertyName = "ReadCapacityUnits";
        private const string WriteCapacityUnitsPropertyName = "WriteCapacityUnits";


        static async Task Main(string[] args)
        {
            var conn = new Dictionary<string, string>()
            {
                [AccessKeyPropertyName] = "AKIAJ5C6UN6EWH3YNVLQ",
                [SecretKeyPropertyName] = "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",
                [ServicePropertyName] = "eu-west-1",
                [ReadCapacityUnitsPropertyName] = "1",
                [WriteCapacityUnitsPropertyName] = "1",
            };
            var str = string.Join(";", conn.Select(c => $"{c.Key}={c.Value}"));

            var channel = new DynamoDBBasedGossipChannel(new LoggerFactory());
            await channel.Initialize("testService", str);

            var data = new Data
            {
                Configuration = new MultiClusterConfiguration(DateTime.UtcNow, new List<string>{"clusterA","clusterB","clusterC"}, "testComment"),
                Gateways = new Dictionary<SiloAddress, GatewayEntry>
                {
                    [SiloAddress.New(new IPEndPoint(IPAddress.Parse("1.1.1.1"), 10), 1)] = new GatewayEntry
                    {
                        ClusterId = "Test1",
                        HeartbeatTimestamp = DateTime.UtcNow,
                        SiloAddress =SiloAddress.New(new IPEndPoint(IPAddress.Parse("1.1.1.1"), 10), 1),
                        Status = GatewayStatus.Active
                    },
                    [SiloAddress.New(new IPEndPoint(IPAddress.Parse("2.2.2.2"), 10), 1)] = new GatewayEntry
                    {
                        ClusterId = "Test2",
                        HeartbeatTimestamp = DateTime.UtcNow,
                        SiloAddress = SiloAddress.New(new IPEndPoint(IPAddress.Parse("2.2.2.2"), 10), 1),
                        Status = GatewayStatus.Active
                    }, [SiloAddress.New(new IPEndPoint(IPAddress.Parse("3.3.3.3"), 10), 1)] = new GatewayEntry
                    {
                        ClusterId = "Test3",
                        HeartbeatTimestamp = DateTime.UtcNow,
                        SiloAddress = SiloAddress.New(new IPEndPoint(IPAddress.Parse("3.3.3.3"), 10), 1),
                        Status = GatewayStatus.Active
                    }
                }
            };
            await channel.Publish(data);

            data = new Data
            {
                Configuration = new MultiClusterConfiguration(DateTime.UtcNow, new List<string>{"clusterA","clusterB","clusterC"}, "testComment"),
                Gateways = new Dictionary<SiloAddress, GatewayEntry>
                {
                    [SiloAddress.New(new IPEndPoint(IPAddress.Parse("4.4.4.4"), 10), 1)] = new GatewayEntry
                    {
                        ClusterId = "Test1",
                        HeartbeatTimestamp = DateTime.UtcNow,
                        SiloAddress =SiloAddress.New(new IPEndPoint(IPAddress.Parse("4.4.4.4"), 10), 1),
                        Status = GatewayStatus.Active
                    },
                    [SiloAddress.New(new IPEndPoint(IPAddress.Parse("2.2.2.2"), 10), 1)] = new GatewayEntry
                    {
                        ClusterId = "Test2",
                        HeartbeatTimestamp = DateTime.UtcNow,
                        SiloAddress = SiloAddress.New(new IPEndPoint(IPAddress.Parse("2.2.2.2"), 10), 1),
                        Status = GatewayStatus.Active
                    }, [SiloAddress.New(new IPEndPoint(IPAddress.Parse("3.3.3.3"), 10), 1)] = new GatewayEntry
                    {
                        ClusterId = "Test3",
                        HeartbeatTimestamp = DateTime.UtcNow,
                        SiloAddress = SiloAddress.New(new IPEndPoint(IPAddress.Parse("3.3.3.3"), 10), 1),
                        Status = GatewayStatus.Active
                    }
                }
            };
            await channel.Synchronize(data);

        }

        public static async Task TestUsageOfDynamo()
        {
            Console.WriteLine("Hello World!");

            var conn = new Dictionary<string, string>()
            {
                [AccessKeyPropertyName] = "AKIAJ5C6UN6EWH3YNVLQ",
                [SecretKeyPropertyName] = "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",
                [ServicePropertyName] = "eu-west-1",
                [ReadCapacityUnitsPropertyName] = "1",
                [WriteCapacityUnitsPropertyName] = "1",
            };
            var str = string.Join(";", conn.Select(c => $"{c.Key}={c.Value}"));

            var m = await GossipTableInstanceManager.GetManager("testServiceId", str, new LoggerFactory());

            var newConf = new MultiClusterConfiguration(DateTime.UtcNow, new List<string> { "clusterA", "clusterB", "clusterC" }, "testComment");

            await m.TryCreateConfigurationEntryAsync(newConf);

            var r = await m.ReadConfigurationEntryAsync();

            var updatedConf = new MultiClusterConfiguration(DateTime.UtcNow, new List<string> { "clusterA", "clusterB" }, "testComment3");

            await m.TryUpdateConfigurationEntryAsync(updatedConf, r);

            var gw = new GatewayEntry
            {
                ClusterId = "Test",
                HeartbeatTimestamp = DateTime.UtcNow,
                SiloAddress = SiloAddress.New(new IPEndPoint(IPAddress.Parse("10.20.30.40"), 10), 20),
                Status = GatewayStatus.Active
            };
            await m.TryCreateGatewayEntryAsync(gw);

            var red = await m.ReadGatewayEntryAsync(gw);

            var gw2 = new GatewayEntry
            {
                ClusterId = "Test2",
                HeartbeatTimestamp = DateTime.UtcNow,
                SiloAddress = SiloAddress.New(new IPEndPoint(IPAddress.Parse("10.20.30.50"), 10), 20),
                Status = GatewayStatus.Inactive
            };
            await m.TryCreateGatewayEntryAsync(gw2);


            var gws = await m.ReadGatewayEntriesAsync();


            // await m.TryDeleteGatewayEntryAsync(red);

            var newgw = new GatewayEntry
            {
                ClusterId = "Test",
                HeartbeatTimestamp = DateTime.UtcNow,
                SiloAddress = SiloAddress.New(new IPEndPoint(IPAddress.Parse("10.20.30.40"), 10), 40),
                Status = GatewayStatus.Inactive
            };
            await m.TryUpdateGatewayEntryAsync(newgw, red);
        }

    }
}
