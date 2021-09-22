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
    public class HTTPService
    {
        public APIGatewayProxyResponse BuildSuccessResponse(string message)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = message,
                Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
            };
        }

        public APIGatewayProxyResponse BuildErrorResponse(string errorMessage, int statusCode = 0)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = statusCode > 0 ? statusCode : (int)HttpStatusCode.InternalServerError,
                Body = errorMessage,
                Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
            };
        }
    }
}
