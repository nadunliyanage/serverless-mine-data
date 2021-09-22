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

namespace AWSServerless2.Services
{
    public class ValidatorService
    {
        public string ValidateRequestParameter(APIGatewayProxyRequest request, string parameterName)
        {
            string minedGuid = null;
            if (request.PathParameters != null && request.PathParameters.ContainsKey(parameterName))
                minedGuid = request.PathParameters[parameterName];
            else if (request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey(parameterName))
                minedGuid = request.QueryStringParameters[parameterName];
            return minedGuid;
        }
    }
}
