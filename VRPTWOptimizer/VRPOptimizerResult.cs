using System.Collections.Generic;
using VRPTWOptimizer.Interfaces;

namespace VRPTWOptimizer
{
    /// <summary>
    /// Represents the output of Vehicle Routing Problem optimization algorithm
    /// </summary>
    public class VRPOptimizerResult
    {
        /// <summary>
        /// List of TransportRequest that the algorithm was unable to fit into any of routes
        /// </summary>
        public List<TransportRequest> LeftRequests { get; set; }

        /// <summary>
        /// Visits schedule for the Vehicle objects
        /// </summary>
        public List<IRoute> Routes { get; set; }
    }
}