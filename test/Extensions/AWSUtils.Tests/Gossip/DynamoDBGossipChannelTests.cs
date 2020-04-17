using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using Orleans.Clustering.DynamoDB;
using Orleans.Clustering.DynamoDB.MultiClusterNetwork;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.MultiClusterNetwork;
using Orleans.TestingHost.Utils;
using TestExtensions;

namespace AWSUtils.Tests.Gossip
{
    class FakeDynamoDBStorage : IDynamoDBStorage
    {
        public async Task InitializeTable(string tableName, List<KeySchemaElement> keys, List<AttributeDefinition> attributes, List<GlobalSecondaryIndex> secondaryIndexes = null) => throw new NotImplementedException();

        public async Task DeleTableAsync(string tableName) => throw new NotImplementedException();

        public async Task PutEntryAsync(string tableName, Dictionary<string, AttributeValue> fields, string conditionExpression = "",
            Dictionary<string, AttributeValue> conditionValues = null) => throw new NotImplementedException();

        public async Task UpsertEntryAsync(string tableName, Dictionary<string, AttributeValue> keys, Dictionary<string, AttributeValue> fields, string conditionExpression = "",
            Dictionary<string, AttributeValue> conditionValues = null, string extraExpression = "", Dictionary<string, AttributeValue> extraExpressionValues = null) => throw new NotImplementedException();

        public async Task DeleteEntryAsync(string tableName, Dictionary<string, AttributeValue> keys, string conditionExpression = "",
            Dictionary<string, AttributeValue> conditionValues = null) => throw new NotImplementedException();

        public async Task DeleteEntriesAsync(string tableName, IReadOnlyCollection<Dictionary<string, AttributeValue>> toDelete) => throw new NotImplementedException();

        public async Task<TResult> ReadSingleEntryAsync<TResult>(string tableName, Dictionary<string, AttributeValue> keys, Func<Dictionary<string, AttributeValue>, TResult> resolver) where TResult : class => throw new NotImplementedException();

        public async Task<(List<TResult> results, Dictionary<string, AttributeValue> lastEvaluatedKey)> QueryAsync<TResult>(string tableName, Dictionary<string, AttributeValue> keys, string keyConditionExpression, Func<Dictionary<string, AttributeValue>, TResult> resolver,
            string indexName = "", bool scanIndexForward = true, Dictionary<string, AttributeValue> lastEvaluatedKey = null) where TResult : class => throw new NotImplementedException();

        public async Task<List<TResult>> ScanAsync<TResult>(string tableName, Dictionary<string, AttributeValue> attributes, string expression, Func<Dictionary<string, AttributeValue>, TResult> resolver) where TResult : class => throw new NotImplementedException();

        public async Task PutEntriesAsync(string tableName, IReadOnlyCollection<Dictionary<string, AttributeValue>> toCreate) => throw new NotImplementedException();
    }

    class FakeProvider : IStorageProvider
    {
        public IDynamoDBStorage GetConfStorage(ILoggerFactory loggerFactory)
        {

        }

        public IDynamoDBStorage GetGatewayStorage(ILoggerFactory loggerFactory)
        {

        }
    }

    class DynamoDBGossipChannelTests
    {
        public async Task Publish()
        {
            var loggerFactory = TestingUtils.CreateDefaultLoggerFactory($"{this.GetType().Name}.log");
            var channel = new DynamoDBBasedGossipChannel(loggerFactory);

            await channel.Initialize("TestService", new FakeProvider());



        }


    }
}
