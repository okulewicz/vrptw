using CommonGIS;
using VRPTWOptimizer.Interfaces;

namespace VRPTWOptimizer.Utils.TimeEstimators
{
    public class ExpertServiceTimeEstimator : ITimeEstimator
    {
        public double StopTime => 1500;

        public double TimePerDeliveredPiece => 60;

        public double TimePerImmediateReturnPiece => 300;

        public double TimePerPickedUpPiece => 90;

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

        public double EstimateLoadUnloadTime(int epUnloadCount, int epLoadOnlyCount, int epImmediatelyRetrievedCount, int handledTransportRequestsCount, double relativeSecondsTimeWindowStart, Location location)
        {
            return EstimateLoadUnloadTime(epUnloadCount, epLoadOnlyCount, epImmediatelyRetrievedCount, handledTransportRequestsCount);
        }
    }
}