using System;
using System.Collections.Generic;
using System.Linq;

namespace VRPTWOptimizer.Interfaces
{
    /// <summary>
    /// Class containing list of routes for each tractor and straight truck
    /// </summary>
    /// <typeparam name="R"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <typeparam name="Rt"></typeparam>
    public class VRPResult<R, V, Rt>
        where R : TransportRequest
        where V : Vehicle
        where Rt : IRoute
    {
        /// <summary>
        /// Requests that were not assigned to any vehicle
        /// </summary>
        public List<R> LeftRequests { get; }

        /// <summary>
        /// Flat list of routes without tractor assignment
        /// </summary>
        [Obsolete]
        public List<Rt> Routes => TractorRoutes.SelectMany(r => r.Value).ToList();
        /// <summary>
        /// Full schedule with routes for each tractor and straight truck
        /// </summary>
        public Dictionary<V, List<Rt>> TractorRoutes { get; }

        /// <summary>
        /// Creates VRP results from routes dictionary and left requests list
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="leftRequests"></param>
        public VRPResult(Dictionary<V, List<Rt>> routes, List<R> leftRequests)
        {
            LeftRequests = leftRequests;
            TractorRoutes = routes;
        }

        /// <summary>
        /// Converts solution into Tuple for backward compatibility
        /// </summary>
        /// <param name="vrp"></param>
        [Obsolete]
        public static implicit operator Tuple<List<Rt>, List<R>>(VRPResult<R, V, Rt> vrp) => new Tuple<List<Rt>, List<R>>(vrp.Routes, vrp.LeftRequests);
    }
}