using System;
using System.Runtime.CompilerServices;
using Amazon.DynamoDBv2.Model;

#if CLUSTERING_DYNAMODB
namespace Orleans.Clustering.DynamoDB
#elif PERSISTENCE_DYNAMODB
namespace Orleans.Persistence.DynamoDB
#elif REMINDERS_DYNAMODB
namespace Orleans.Reminders.DynamoDB
#elif AWSUTILS_TESTS
namespace Orleans.AWSUtils.Tests
#elif TRANSACTIONS_DYNAMODB
namespace Orleans.Transactions.DynamoDB
#else
// No default namespace intentionally to cause compile errors if something is not defined
#endif
{
    public static class DynamoDBStorageUtils
    {
        /// <summary>
        /// Check whether an exception returned from a DynamoDB call might be due to a (temporary) storage contention error.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>true if error is due to contention</returns>
        public static bool IsContentionError(Exception e)
        {
            // For a detailed overview of errors, please refer to
            // https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/Programming.Errors.html

            switch (e)
            {
                case ConditionalCheckFailedException _:
                case Amazon.DynamoDBv2.Model.LimitExceededException _:
                case ProvisionedThroughputExceededException _:
                case ResourceInUseException _:
                case ResourceNotFoundException _:
                    return true;
                default:
                    return false;
            }
        }
    }
}
