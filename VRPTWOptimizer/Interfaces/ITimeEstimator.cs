using CommonGIS;
using System;

namespace VRPTWOptimizer.Interfaces
{
    /// <summary>
    /// Predicts time of loading and unloading cargo at Location
    /// </summary>
    public interface ITimeEstimator
    {
        /// <summary>
        /// This method is used to predict time it will take to serve all the loadings and unloadings within the given location
        /// </summary>
        /// <param name="epUnloadCount">Number of logistic units that are being unloaded</param>
        /// <param name="epLoadOnlyCount">Number of logistic units that are being loaded, but were not unloaded</param>
        /// <param name="epImmediatelyRetrievedCount">Number of logistic units that are being delivered and immediately retrieved after handling in Location</param>
        /// <param name="handledTransportRequestsCount">Number of transport requests loaded and unloaded in the given location</param>
        /// <returns></returns>
        [Obsolete]
        double EstimateLoadUnloadTime(int epUnloadCount, int epLoadOnlyCount, int epImmediatelyRetrievedCount, int handledTransportRequestsCount);

        /// <summary>
        /// This method is used for more accurate load/unload time estimation for a context free (not placed in route) case
        /// </summary>
        /// <param name="epUnloadCount">Number of logistic units that are being unloaded</param>
        /// <param name="epLoadOnlyCount">Number of logistic units that are being loaded, but were not unloaded</param>
        /// <param name="epImmediatelyRetrievedCount">Number of logistic units that are being delivered and immediately retrieved after handling in Location</param>
        /// <param name="handledTransportRequestsCount">Number of transport requests loaded and unloaded in the given location</param>
        /// <param name="relativeSecondsTimeWindowStart">Number of seconds relative to ZeroHour of a given preferred arrival time</param>
        /// <param name="location">Visited location</param>
        /// <returns></returns>
        double EstimateLoadUnloadTime(int epUnloadCount, int epLoadOnlyCount, int epImmediatelyRetrievedCount, int handledTransportRequestsCount,
            double relativeSecondsTimeWindowStart, Location location);
    }
}