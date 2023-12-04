using CommonGIS;
using CommonGIS.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using VRPTWOptimizer.Enums;

namespace VRPTWOptimizer
{
    /// <summary>
    /// Defines properties of a Vehicle
    /// </summary>
    public abstract class Vehicle : Resource
    {
        /// <summary>
        /// Array of capacity dimensions (e.g. mass, length, volume, euro pallets count)
        /// Freezer availability can also be treated as dimension
        /// </summary>
        public double[] Capacity { get; set; }
        /// <summary>
        /// Array of how the packages sizes are aggregated in various dimensions (e.g. sum for mass, max for length )
        /// </summary>
        public Aggregation[] CapacityAggregationType { get; set; }
        /// <summary>
        /// Location where vehicle needs to finish its operations
        /// </summary>
        public Location FinalLocation { get; set; }
        /// <summary>
        /// Location where vehicle will be available at AvailabilityStart
        /// </summary>
        public Location InitialLocation { get; set; }
        /// <summary>
        /// Max time span when vehicle can be on road
        /// </summary>
        public double MaxRideTime { get; set; }
        /// <summary>
        /// Identifier of the company owning the vehicle
        /// </summary>
        public int OwnerID { get; set; }
        /// <summary>
        /// Size of the vehicle that affects possibility of traversing given route or accessing a certain Location
        /// </summary>
        public VehicleRoadRestrictionProperties RoadProperties { get; set; }
        /// <summary>
        /// Describes properties of vehicles within a given domain (e.g. lift, freezer)
        /// </summary>
        public int[] SpecialProperties { get; set; }
        /// <summary>
        /// Type of vehicle necessary to decide if needs to be combined with a unit with engine (semi-trailer) or not (truck)
        /// </summary>
        public VehicleType Type { get; set; }
        /// <summary>
        /// Cost of using vehicle per distance unit
        /// </summary>
        public double VehicleCostPerDistanceUnit { get; set; }
        /// <summary>
        /// Cost of using vehicle per time unit (while moving from Location to Location)
        /// </summary>
        public double VehicleCostPerTimeUnit { get; set; }

        /// <summary>
        /// Cost of utilizing vehicle fore each of its routes
        /// </summary>
        public double VehicleCostPerRoute { get; set; }
        /// <summary>
        /// Cost of using vehicle at all in a given problem
        /// </summary>
        public double VehicleCostPerUsage { get; set; }
        /// <summary>
        /// Cost of using vehicle if route length is shorter than VehicleMaxRouteLengthForFlatCost (in meters)
        /// </summary>
        public double VehicleFlatCostForShortRouteLength { get; set; }
        /// <summary>
        /// Distance (by default in meters) for which VehicleFlatCostForShortRouteLength flat rate is applied,
        /// otherwise VehicleCostPerDistanceUnit * Distance is applieds
        /// </summary>
        public double VehicleMaxRouteLengthForFlatCost { get; set; }

        /// <summary>
        /// Creates vehicle object for backward compatibility
        /// </summary>
        /// <param name="id"></param>
        /// <param name="capacity"></param>
        /// <param name="specialProperties"></param>
        /// <param name="capacityAggregation"></param>
        /// <param name="initialLocation"></param>
        /// <param name="availabilityStart"></param>
        /// <param name="finalLocation"></param>
        /// <param name="availabilityEnd"></param>
        /// <param name="maxRideTime"></param>
        /// <param name="roadProperties"></param>
        /// <param name="type"></param>
        /// <param name="vehicleCostPerDistanceUnit"></param>
        /// <param name="vehicleCostPerTimeUnit"></param>
        /// <param name="vehicleCostPerUsage"></param>
        /// <param name="ownerID"></param>
        public Vehicle(int id,
                       double[] capacity,
                       int[] specialProperties,
                       Aggregation[] capacityAggregation,
                       Location initialLocation,
                       double availabilityStart,
                       Location finalLocation,
                       double availabilityEnd,
                       double maxRideTime,
                       VehicleRoadRestrictionProperties roadProperties,
                       VehicleType type,
                       double vehicleCostPerDistanceUnit,
                       double vehicleCostPerTimeUnit,
                       double vehicleCostPerUsage,
                       int ownerID) : this(id,
                                           capacity,
                                           specialProperties,
                                           capacityAggregation,
                                           initialLocation,
                                           availabilityStart,
                                           finalLocation,
                                           availabilityEnd,
                                           maxRideTime,
                                           roadProperties,
                                           type,
                                           vehicleCostPerDistanceUnit,
                                           vehicleCostPerTimeUnit,
                                           vehicleCostPerUsage,
                                           ownerID,
                                           vehicleFlatCostForShortRouteLength: 0,
                                           vehicleMaxRouteLengthForFlatCost: 0,
                                           vehicleCostPerRoute: 0)
        {
        }

        /// <summary>
        /// Creates vehicle object
        /// </summary>
        /// <param name="id"></param>
        /// <param name="capacity"></param>
        /// <param name="specialProperties"></param>
        /// <param name="capacityAggregation"></param>
        /// <param name="initialLocation"></param>
        /// <param name="availabilityStart"></param>
        /// <param name="finalLocation"></param>
        /// <param name="availabilityEnd"></param>
        /// <param name="maxRideTime"></param>
        /// <param name="roadProperties"></param>
        /// <param name="type"></param>
        /// <param name="vehicleCostPerDistanceUnit"></param>
        /// <param name="vehicleCostPerTimeUnit"></param>
        /// <param name="vehicleCostPerUsage"></param>
        /// <param name="ownerID"></param>
        /// <param name="vehicleFlatCostForShortRouteLength"></param>
        /// <param name="vehicleMaxRouteLengthForFlatCost"></param>
        /// <param name="vehicleCostPerRoute"></param>
        public Vehicle(int id,
               double[] capacity,
               int[] specialProperties,
               Aggregation[] capacityAggregation,
               Location initialLocation,
               double availabilityStart,
               Location finalLocation,
               double availabilityEnd,
               double maxRideTime,
               VehicleRoadRestrictionProperties roadProperties,
               VehicleType type,
               double vehicleCostPerDistanceUnit,
               double vehicleCostPerTimeUnit,
               double vehicleCostPerUsage,
               int ownerID,
               double vehicleFlatCostForShortRouteLength,
               double vehicleMaxRouteLengthForFlatCost,
               double vehicleCostPerRoute) : base(id, availabilityStart, availabilityEnd)
        {
            Capacity = new double[capacity.Length];
            Array.Copy(capacity, Capacity, capacity.Length);
            CapacityAggregationType = new Aggregation[capacityAggregation.Length];
            Array.Copy(capacityAggregation, CapacityAggregationType, capacityAggregation.Length);
            SpecialProperties = new int[specialProperties.Length];
            Array.Copy(specialProperties, SpecialProperties, specialProperties.Length);
            InitialLocation = initialLocation;
            FinalLocation = finalLocation;
            MaxRideTime = maxRideTime;
            Type = type;
            RoadProperties = roadProperties;
            VehicleCostPerDistanceUnit = vehicleCostPerDistanceUnit;
            VehicleCostPerTimeUnit = vehicleCostPerTimeUnit;
            VehicleCostPerUsage = vehicleCostPerUsage;
            OwnerID = ownerID;
            VehicleFlatCostForShortRouteLength = vehicleFlatCostForShortRouteLength;
            VehicleMaxRouteLengthForFlatCost = vehicleMaxRouteLengthForFlatCost;
            VehicleCostPerRoute = vehicleCostPerRoute;
        }

        private static bool CanAccomodateCargoTypesTogether(IEnumerable<TransportRequest> requests)
        {
            foreach (var request in requests.Where(rq => rq.RestrictedCargoTypes.Any()))
            {
                if (requests.Any(
                    req => request.CargoTypes.Any(
                        ct => req.RestrictedCargoTypes.Contains(ct))))
                {
                    return false;
                }
            }
            return true;
        }

        private bool CanFitCapacity(IEnumerable<TransportRequest> requestGroup)
        {
            return Vehicle.CanFitCapacity(this.Capacity, this.CapacityAggregationType, requestGroup.Select(rq => rq.Size));
        }

        public double ComputeDistanceCost(double length)
        {
            if (length < this.VehicleMaxRouteLengthForFlatCost)
            {
                return this.VehicleFlatCostForShortRouteLength;
            }
            else
            {
                return this.VehicleCostPerDistanceUnit * length;
            }
        }

        public double ComputeTimeCost(double travelTime)
        {
            return this.VehicleCostPerTimeUnit * travelTime;
        }

        /// <summary>
        /// Checks if the given list of cargo sizes would fit into specified capacity according to proper size aggregation
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="aggregationType"></param>
        /// <param name="sizes"></param>
        /// <returns></returns>
        public static bool CanFitCapacity(double[] capacity, Aggregation[] aggregationType, IEnumerable<double[]> sizes)
        {
            for (int i = 0; i < capacity.Length; i++)
            {
                if (aggregationType[i] == Aggregation.Sum)
                {
                    if (capacity[i] < sizes.Sum(r => r[i]))
                    {
                        return false;
                    }
                }
                else if (aggregationType[i] == Aggregation.Max)
                {
                    if (capacity[i] < sizes.Max(r => r[i]))
                    {
                        return false;
                    }
                }
                else
                {
                    throw new ArgumentException($"Vehicle does not support {aggregationType[i]} aggregation of sizes");
                }
            }
            return true;
        }

        /// <summary>
        /// Verifies if a pool of TransportRequest objects can fit together within the Vehicle
        /// </summary>
        /// <param name="requests"></param>
        /// <returns></returns>
        public bool CanFitRequests(IEnumerable<TransportRequest> requests)
        {
            if (requests.Any(req => !this.CanHandleRequest(req)))
            {
                return false;
            }
            if (!CanAccomodateCargoTypesTogether(requests))
            {
                return false;
            }
            if (!CanFitCapacity(requests))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Verifies if a pool of TransportRequest objects can fit together within the Vehicle
        /// </summary>
        /// <param name="requests"></param>
        /// <returns></returns>
        public bool CanFitRequestsSomewhereInVehicle(IEnumerable<TransportRequest> requests)
        {
            if (requests.Any(req => !this.CanHandleRequest(req)))
            {
                return false;
            }
            foreach (var requestGroup in requests.GroupBy(rq => rq.PickupLocation.Id))
            {
                if (!CanAccomodateCargoTypesTogether(requestGroup))
                {
                    return false;
                }
                if (!CanFitCapacity(requestGroup))
                {
                    return false;
                }
            }
            foreach (var requestGroup in requests.GroupBy(rq => rq.DeliveryLocation.Id))
            {
                if (!CanAccomodateCargoTypesTogether(requestGroup))
                {
                    return false;
                }
                if (!CanFitCapacity(requestGroup))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Verifies if properties of TransportRequest conform with abilities of Vehicle
        /// and restrictions of TransportRequest allow the Vehicle to approach initial
        /// and final destinations
        /// </summary>
        /// <param name="candidateRequest"></param>
        /// <returns></returns>
        public bool CanHandleRequest(TransportRequest candidateRequest)
        {
            if (this.Capacity.Length != candidateRequest.Size.Length)
            {
                throw new ArgumentException("Capacity properties of the vehicle do not match request size");
            }
            if (!candidateRequest.MaxVehicleSize.DoesVehicleFitIntoRestrictions(this.RoadProperties))
            {
                return false;
            }
            for (int i = 0; i < this.Capacity.Length; i++)
            {
                if (this.Capacity[i] < candidateRequest.Size[i])
                {
                    return false;
                }
            }
            if (candidateRequest.NecessaryVehicleSpecialProperties != null)
            {
                for (int i = 0; i < candidateRequest.NecessaryVehicleSpecialProperties.Length; i++)
                {
                    if (!this.SpecialProperties.Contains(candidateRequest.NecessaryVehicleSpecialProperties[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}