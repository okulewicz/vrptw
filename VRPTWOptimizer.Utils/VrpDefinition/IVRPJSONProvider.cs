using CommonGIS;
using System;
using System.Collections.Generic;
using VRPTWOptimizer.Interfaces;

namespace VRPTWOptimizer.Utils.VrpDefinition
{
    public interface IVRPJSONProvider : IVRPProvider
    {
        string Client { get; set; }
        string DepotId { get; set; }
        List<Distance> Distances { get; set; }
        Dictionary<string, Location> LocationsDictionary { get; set; }
        DateTime ProblemDate { get; set; }

        ITimeEstimator ServiceTimeEstimator { get; set; }
    }
}