using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSServerless2
{
    public class Functions
    {
        // This const is the name of the environment variable that the serverless.template will use to set
        // the name of the DynamoDB table used to store mine data.
        const string TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP = "MineDataTable";

        public const string ID_QUERY_STRING_NAME = "Guid";
        IDynamoDBContext DDBContext { get; set; }

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {
            // Check to see if a table name was passed in through environment variables and if so 
            // add the table mapping.
            var tableName = System.Environment.GetEnvironmentVariable(TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP);
            if (!string.IsNullOrEmpty(tableName))
            {
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(MineData)] = new Amazon.Util.TypeMapping(typeof(MineData), tableName);
            }

            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            this.DDBContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        /// <summary>
        /// Constructor used for testing passing in a preconfigured DynamoDB client.
        /// </summary>
        /// <param name="ddbClient"></param>
        /// <param name="tableName"></param>
        public Functions(IAmazonDynamoDB ddbClient, string tableName)
        {
            if (!string.IsNullOrEmpty(tableName))
            {
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(MineData)] = new Amazon.Util.TypeMapping(typeof(MineData), tableName);
            }

            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            this.DDBContext = new DynamoDBContext(ddbClient, config);
        }

        /// <summary>
        /// A Lambda function that adds a mining record.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> StartMining(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var mineData = new MineData();
            mineData.Guid = Guid.NewGuid().ToString();
            mineData.CreatedTimestamp = DateTime.Now;

            context.Logger.LogLine($"Saving mineData with id {mineData.Guid}");
            await DDBContext.SaveAsync<MineData>(mineData);

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = mineData.Guid.ToString(),
                Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
            };
            return response;
        }

        /// <summary>
        /// A Lambda function that collects mining data from the DynamoDB table.
        /// </summary>
        /// <param name="request"></param>
        public async Task<APIGatewayProxyResponse> CollectMiningResults(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string minedGuid = null;
            if (request.PathParameters != null && request.PathParameters.ContainsKey(ID_QUERY_STRING_NAME))
                minedGuid = request.PathParameters[ID_QUERY_STRING_NAME];
            else if (request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey(ID_QUERY_STRING_NAME))
                minedGuid = request.QueryStringParameters[ID_QUERY_STRING_NAME];

            if (string.IsNullOrEmpty(minedGuid))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = $"Missing required parameter {ID_QUERY_STRING_NAME}"
                };
            }
            MineData mineResult = await DDBContext.LoadAsync<MineData>(minedGuid);
            context.Logger.LogLine($"Found mine result: {mineResult != null}");

            DateTime currentTime = DateTime.Now;
            DateTime timeThirtySecBefore = currentTime.AddSeconds(-30);
            DateTime timeNinetySecBefore = currentTime.AddSeconds(-90);
            if (mineResult != null && mineResult.CreatedTimestamp > timeNinetySecBefore
                && mineResult.CreatedTimestamp < timeThirtySecBefore)
            {
                context.Logger.LogLine($"Deleting mineData with Guid {minedGuid}");
                await this.DDBContext.DeleteAsync<MineData>(minedGuid);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = new Random().Next(1, 100).ToString(),
                };
            }
            else
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = Convert.ToString(0),
                };
            }


        }
    }
}
