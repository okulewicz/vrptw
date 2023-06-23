using System;
using System.Collections.Generic;

namespace VRPTWOptimizer.Interfaces
{
    public interface IVRPSolutionWriter
    {
        public void SaveSolution(List<IRoute> routes, List<TransportRequest> unassignedRequests, DateTime billingDate, string homeDepotId, string algorithmName);
    }
}