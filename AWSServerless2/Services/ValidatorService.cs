using Amazon.Lambda.APIGatewayEvents;

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
