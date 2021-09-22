using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

using Amazon.DynamoDBv2;

using AWSServerless2.Services;

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
        
        MineDataService mineDataService;
        ValidatorService validatorService;
        DBService dbService;

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {
            // Check to see if a table name was passed in through environment variables and if so 
            // add the table mapping.
            mineDataService = new MineDataService();
            validatorService = new ValidatorService();
            dbService = new DBService();
            var tableName = System.Environment.GetEnvironmentVariable(TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP);
            dbService.InstantiateDBContext(tableName, null);
        }

        /// <summary>
        /// Constructor used for testing passing in a preconfigured DynamoDB client.
        /// </summary>
        /// <param name="ddbClient"></param>
        /// <param name="tableName"></param>
        public Functions(IAmazonDynamoDB ddbClient, string tableName)
        {
            mineDataService = new MineDataService();
            validatorService = new ValidatorService();
            dbService = new DBService();
            dbService.InstantiateDBContext(tableName, ddbClient);
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
            return await mineDataService.ProcessStartMining(context, mineData);
        }

        /// <summary>
        /// A Lambda function that collects mining data from the DynamoDB table.
        /// </summary>
        /// <param name="request"></param>
        public async Task<APIGatewayProxyResponse> CollectMiningResults(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string minedGuid = validatorService.ValidateRequestParameter(request, ID_QUERY_STRING_NAME);            
            return await mineDataService.ProcessCollectingMiningResults(context, minedGuid);
        }
    }
}
