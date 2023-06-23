using Newtonsoft.Json;
using VRPTWOptimizer.Interfaces;

namespace VRPTWOptimizer.Utils.Model
{
    public partial class ViaTmsJSONDTOs
    {
        public class TimeEstimatorDTO : ITimeEstimator
        {
            [JsonProperty("StopTime")]
            public double StopTime { get; set; }
            [JsonProperty("TimePerDeliveredPiece")]
            public double TimePerDeliveredPiece { get; set; }
            [JsonProperty("TimePerImmediateReturnPiece")]
            public double TimePerImmediateReturnPiece { get; set; }
            [JsonProperty("TimePerPickedUpPiece")]
            public double TimePerPickedUpPiece { get; set; }

            public double EstimateLoadUnloadTime(int epUnloadCount, int epLoadOnlyCount, int epImmediatelyRetrievedCount, int noRequests)
            {
                if (noRequests == 0)
                {
                    return 0;
                }
                return StopTime +
                    TimePerDeliveredPiece * epUnloadCount +
                    TimePerPickedUpPiece * epLoadOnlyCount +
                    TimePerImmediateReturnPiece * epImmediatelyRetrievedCount;
            }

            public double EstimateLoadUnloadTime(int epUnloadCount, int epLoadOnlyCount, int epImmediatelyRetrievedCount, int handledTransportRequestsCount, double relativeSecondsTimeWindowStart, CommonGIS.Location location)
            {
                return EstimateLoadUnloadTime(epUnloadCount, epLoadOnlyCount, epImmediatelyRetrievedCount, handledTransportRequestsCount);
            }
        }
    }
}