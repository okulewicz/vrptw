using CommonGIS;
using CommonGIS.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace VRPTWOptimizer.DistanceProviders
{
    public abstract class DictionaryDistanceProviderBase : IStoredDistanceProvider
    {
        protected Dictionary<string, Dictionary<string, Dictionary<VehicleRoadRestrictionProperties, Distance>>> distanceMatrix;
        private readonly Dictionary<int, Dictionary<int, Distance>> distanceMatrixList;
        protected ConcurrentDictionary<int, Dictionary<VehicleRoadRestrictionProperties, VehicleRoadRestrictionProperties>> vehicleToProfileMapper;
        private bool optimizedSpeedMode;
        protected bool SelfContain;
        private readonly object vehicleToProfileMapperLock = new object();
        public List<Distance> StoredDistances { get; }

        protected DictionaryDistanceProviderBase(bool selfContain)
        {
            StoredDistances = new List<Distance>();
            SelfContain = selfContain;
            distanceMatrix = new Dictionary<string, Dictionary<string, Dictionary<VehicleRoadRestrictionProperties, Distance>>>();
            distanceMatrixList = new Dictionary<int, Dictionary<int, Distance>>();
            vehicleToProfileMapper = new ConcurrentDictionary<int, Dictionary<VehicleRoadRestrictionProperties, VehicleRoadRestrictionProperties>>();
            optimizedSpeedMode = true;
        }

        protected void InitializeDistanceDictionary(List<Distance> distances)
        {
            StoredDistances.AddRange(distances);

            foreach (var distance in StoredDistances)
            {
                if (!int.TryParse(distance.FromId, out int fromId)) optimizedSpeedMode = false;
                if (!int.TryParse(distance.ToId, out int toId)) optimizedSpeedMode = false;
                if (!distanceMatrixList.ContainsKey(fromId))
                {
                    distanceMatrixList.Add(fromId, new Dictionary<int, Distance>());
                }
                if (!distanceMatrixList[fromId].ContainsKey(toId))
                {
                    distanceMatrixList[fromId].Add(toId, distance);
                }
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
                    if (optimizedSpeedMode && distanceMatrix[distance.FromId][distance.ToId].Keys.Count > 1)
                    {
                        optimizedSpeedMode = false;
                    }
                }
            }
        }

        public Distance GetDistance(
                    Location from,
            Location to,
            VehicleRoadRestrictionProperties vehicleProperties)
        {
            if (optimizedSpeedMode)
            {
                int.TryParse(from.Id, out int fromId);
                int.TryParse(to.Id, out int toId);
                if (from.Id == to.Id)
                {
                    return new TimeLengthDistance(from.Id, to.Id, 0.0, 0.0, vehicleProperties);
                }
                else if (distanceMatrixList.ContainsKey(fromId) && distanceMatrixList[fromId].ContainsKey(toId))
                {
                    return distanceMatrixList[fromId][toId];
                }
                else
                {
                    return NoDistanceFound(from, to, vehicleProperties);
                }
            }
            else
            {
                int threadId = Thread.CurrentThread.ManagedThreadId;
                if (!optimizedSpeedMode && !vehicleToProfileMapper.ContainsKey(threadId))
                {
                    vehicleToProfileMapper.TryAdd(threadId, new Dictionary<VehicleRoadRestrictionProperties, VehicleRoadRestrictionProperties>(VehicleRoadRestrictionsComparer.Instance));
                }
                if (from.Id == to.Id)
                {
                    return new TimeLengthDistance(from.Id, to.Id, 0.0, 0.0, vehicleProperties);
                }
                else if (distanceMatrix.ContainsKey(from.Id) && distanceMatrix[from.Id].ContainsKey(to.Id))
                {
                    if (optimizedSpeedMode && distanceMatrix[from.Id][to.Id].Values.Any())
                    {
                        return distanceMatrix[from.Id][to.Id].Values.First();
                    }
                    if (!vehicleToProfileMapper[threadId].ContainsKey(vehicleProperties))
                    {
                        var profiles = distanceMatrix[from.Id][to.Id].Keys
                            .Where(prf => prf.DoesVehicleFitIntoRestrictions(vehicleProperties));
                        if (profiles.Any())
                        {
                            var profileCapacity =
                                profiles
                                .OrderBy(prf => prf.EpCount)
                                .ThenBy(prf => prf.GrossVehicleWeight)
                                .First();
                            vehicleToProfileMapper[threadId].TryAdd(vehicleProperties, profileCapacity);
                        }
                        else
                        {
                            return NoDistanceFound(from, to, vehicleProperties);
                        }
                    }
                    if (distanceMatrix[from.Id][to.Id].ContainsKey(vehicleToProfileMapper[threadId][vehicleProperties]))
                    {
                        return distanceMatrix[from.Id][to.Id][vehicleToProfileMapper[threadId][vehicleProperties]];
                    }
                    else
                    {
                        var profiles = distanceMatrix[from.Id][to.Id].Keys
                            .Where(prf => prf.DoesVehicleFitIntoRestrictions(vehicleProperties));
                        if (profiles.Any())
                        {
                            var profileCapacity =
                                profiles
                                .OrderBy(prf => prf.EpCount)
                                .ThenBy(prf => prf.GrossVehicleWeight)
                                .First();
                            return distanceMatrix[from.Id][to.Id][profileCapacity];
                        }
                    }
                }
                return NoDistanceFound(from, to, vehicleProperties);
            }
        }

        private Distance NoDistanceFound(Location from, Location to, VehicleRoadRestrictionProperties vehicleProperties)
        {
            /*
            if (distanceMatrix.ContainsKey(from.Id) && distanceMatrix[from.Id].ContainsKey(to.Id))
                return distanceMatrix[from.Id][to.Id].First().Value;
            */
            return SelfContain ?
                new TimeLengthDistance(from.Id, to.Id, double.MaxValue, double.MaxValue, vehicleProperties) : null;
        }
    }
}