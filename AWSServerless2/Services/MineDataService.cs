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
    public class MineDataService
    {
        HTTPService hTTPService;
        public MineDataService()
        {
            hTTPService = new HTTPService();
        }

        public async Task<APIGatewayProxyResponse> ProcessStartMining(ILambdaContext context, MineData mineData)
        {
            context.Logger.LogLine($"Saving mineData with id {mineData.Guid}");
            try
            {
                await DBService.DDB_CONTEXT.SaveAsync<MineData>(mineData);
            }
            catch (Exception e)
            {
                return hTTPService.BuildErrorResponse(e.Message);
            }

            return hTTPService.BuildSuccessResponse(mineData.Guid.ToString());
        }

        public async Task<APIGatewayProxyResponse> ProcessCollectingMiningResults(ILambdaContext context, string minedGuid)
        {
            if (string.IsNullOrEmpty(minedGuid))
            {
                return hTTPService.BuildErrorResponse($"Missing required parameter", (int)HttpStatusCode.BadRequest);
            }
            MineData mineResult = await DBService.DDB_CONTEXT.LoadAsync<MineData>(minedGuid);
            context.Logger.LogLine($"Found mine result: {mineResult != null}");
            var currentTime = DateTime.Now;
            var timeThirtySecBefore = currentTime.AddSeconds(-30);
            var timeNinetySecBefore = currentTime.AddSeconds(-90);
            string responseBody = null;
            if (mineResult != null && mineResult.CreatedTimestamp > timeNinetySecBefore
                && mineResult.CreatedTimestamp < timeThirtySecBefore)
            {
                context.Logger.LogLine($"Deleting mineData with Guid {minedGuid}");
                await DBService.DDB_CONTEXT.DeleteAsync<MineData>(minedGuid);
                responseBody = new Random().Next(1, 100).ToString();
            }
            else
            {
                responseBody = Convert.ToString(0);
            }
            return hTTPService.BuildSuccessResponse(responseBody);
        }

        
    }
}
