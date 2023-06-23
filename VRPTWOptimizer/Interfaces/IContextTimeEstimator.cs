using CommonGIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRPTWOptimizer.Interfaces
{
    /// <summary>
    /// Interface of a more accurate service time predictions taking into account route context of a given service
    /// </summary>
    public interface IContextServiceTimeEstimator
    {
        /// <summary>
        /// This method is used for more accurate load/unload time estimation for a context free (not placed in route) case
        /// </summary>
        /// <param name="epUnloadCount">Number of logistic units that are being unloaded</param>
        /// <param name="epLoadOnlyCount">Number of logistic units that are being loaded, but were not unloaded</param>
        /// <param name="epImmediatelyRetrievedCount">Number of logistic units that are being delivered and immediately retrieved after handling in Location</param>
        /// <param name="handledTransportRequestsCount">Number of transport requests loaded and unloaded in the given location</param>
        /// <param name="epBlockingDelivery">Number of logistics units that were picked up before making all deliveries</param>
        /// <param name="isUnmannedDelivery">Delivery will be made to special container</param>
        /// <param name="secondsDelay">Number of seconds arrival is delayed againts preferred time window start</param>
        /// <param name="vehicleSizeInEp">Vehicle size given in Euro Pallets</param>
        /// <param name="visitOrder">Visit order in route</param>
        /// <param name="visitsCount">Number of visits in route</param>
        /// <param name="relativeSecondsArrivalTime">Number of seconds after ZeroHour of planned arrival time</param>
        /// 
        /// <param name="location">Visited location</param>
        /// <returns></returns>
        double EstimateLoadUnloadTimeInContext(int epUnloadCount, int epLoadOnlyCount, int epImmediatelyRetrievedCount, int handledTransportRequestsCount, int vehicleSizeInEp, int epBlockingDelivery,
            int visitOrder, int visitsCount, double secondsDelay, bool isUnmannedDelivery, double relativeSecondsArrivalTime, Location location);

    }
}
