using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

using Newtonsoft.Json;

namespace AWSServerless2.Services
{
    public class DBService
    {
        public static IDynamoDBContext DDB_CONTEXT;
        public void InstantiateDBContext(string tableName, IAmazonDynamoDB ddbClient)
        {
            if (!string.IsNullOrEmpty(tableName))
            {
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(MineData)] = new Amazon.Util.TypeMapping(typeof(MineData), tableName);
            }
            var client = ddbClient != null ? ddbClient : new AmazonDynamoDBClient();
            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            DDB_CONTEXT = new DynamoDBContext(client, config);
        }
    }
}
