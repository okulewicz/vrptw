using CommonGIS;
using System.Collections.Generic;
using VRPTWOptimizer.DistanceProviders;

namespace VRPTWOptimizer.Utils.Provider
{
    public class DistanceMatrixProvider : DictionaryDistanceProviderBase
    {
        public DistanceMatrixProvider(List<Distance> distancesList, bool selfContain = true) : base(selfContain)
        {
            InitializeDistanceDictionary(distancesList);
        }

        public void AddDistances(List<Distance> distances)
        {
            InitializeDistanceDictionary(distances);
        }
    }
}