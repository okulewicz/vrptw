using CommonGIS;
using CommonGIS.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace VRPTWOptimizer.DistanceProviders
{
    public abstract class DictionaryDistanceProviderBase : IDistanceProvider
    {
        protected Dictionary<string, Dictionary<string, Dictionary<VehicleRoadRestrictionProperties, Distance>>> distanceMatrix;
        protected Dictionary<VehicleRoadRestrictionProperties, VehicleRoadRestrictionProperties> vehicleToProfileMapper;
        protected bool SelfContain;
        private readonly object vehicleToProfileMapperLock = new object();
        public List<Distance> StoredDistances { get; }

        protected DictionaryDistanceProviderBase(bool selfContain)
        {
            StoredDistances = new List<Distance>();
            SelfContain = selfContain;
            distanceMatrix = new Dictionary<string, Dictionary<string, Dictionary<VehicleRoadRestrictionProperties, Distance>>>();
            vehicleToProfileMapper = new Dictionary<VehicleRoadRestrictionProperties, VehicleRoadRestrictionProperties>(VehicleRoadRestrictionsComparer.Instance);
        }

        protected void InitializeDistanceDictionary(List<Distance> distances)
        {
            StoredDistances.AddRange(distances);

            foreach (var distance in StoredDistances)
            {
                if (!distanceMatrix.ContainsKey(distance.FromId))
                {
                    distanceMatrix.Add(distance.FromId, new Dictionary<string, Dictionary<VehicleRoadRestrictionProperties, Distance>>());
                }
                if (!distanceMatrix[distance.FromId].ContainsKey(distance.ToId))
                {
                    distanceMatrix[distance.FromId].Add(distance.ToId, new Dictionary<VehicleRoadRestrictionProperties, Distance>(VehicleRoadRestrictionsComparer.Instance));
                }
                if (!distanceMatrix[distance.FromId][distance.ToId].ContainsKey(distance.Profile))
                {
                    distanceMatrix[distance.FromId][distance.ToId].Add(distance.Profile, distance);
                }
            }
        }

        public Distance GetDistance(
                    Location from,
            Location to,
            VehicleRoadRestrictionProperties vehicleProperties)
        {
            if (from.Id == to.Id)
            {
                return new TimeLengthDistance(from.Id, to.Id, 0.0, 0.0, vehicleProperties);
            }
            else if (distanceMatrix.ContainsKey(from.Id) && distanceMatrix[from.Id].ContainsKey(to.Id))
            {
                if (!vehicleToProfileMapper.ContainsKey(vehicleProperties))
                {
                    var profiles = distanceMatrix[from.Id][to.Id].Keys
                        .Where(prf => prf.DoesVehicleFitIntoRestrictions(vehicleProperties));
                    if (profiles.Any())
                    {
                        var profileCapacity =
                            profiles
                            .OrderBy(prf => prf.EpCount)
                            .OrderBy(prf => prf.GrossVehicleWeight)
                            .First();
                        lock (vehicleToProfileMapperLock)
                        {
                            vehicleToProfileMapper.TryAdd(vehicleProperties, profileCapacity);
                        }
                    }
                    else
                    {
                        return NoDistanceFound(from, to, vehicleProperties);
                    }
                }
                if (distanceMatrix[from.Id][to.Id].ContainsKey(vehicleToProfileMapper[vehicleProperties]))
                {
                    return distanceMatrix[from.Id][to.Id][vehicleToProfileMapper[vehicleProperties]];
                }
            }
            return NoDistanceFound(from, to, vehicleProperties);
        }

        private Distance NoDistanceFound(Location from, Location to, VehicleRoadRestrictionProperties vehicleProperties)
        {
            return SelfContain ?
                new TimeLengthDistance(from.Id, to.Id, double.MaxValue, double.MaxValue, vehicleProperties) : null;
        }
    }
}