using CommonGIS;
using System;
using System.Collections.Generic;
using VRPTWOptimizer.Interfaces;

namespace VRPTWOptimizer.Utils.VrpDefinition
{
    public interface IVRPJSONProvider : IVRPProvider
    {
        string Client { get; }
        string DepotId { get; }
        List<Distance> Distances { get; }
        Dictionary<string, Location> LocationsDictionary { get; }
        DateTime ProblemDate { get; }

        ITimeEstimator ServiceTimeEstimator { get; }
    }
}