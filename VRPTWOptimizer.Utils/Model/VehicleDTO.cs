using CommonGIS;
using CommonGIS.Enums;
using Newtonsoft.Json;
using System;
using VRPTWOptimizer.Enums;

namespace VRPTWOptimizer.Utils.Model

{
    public class VehicleDTO : Vehicle
    {
        [JsonConstructor]
        public VehicleDTO(
            int id,
            double[] capacity,
            int[] specialProperties,
            Aggregation[] capacityAggregationType,
            BaseLocation initialLocation,
            double availabilityStart,
            BaseLocation finalLocation,
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
            double vehicleCostPerRoute) : base(id, capacity, specialProperties, capacityAggregationType, initialLocation, availabilityStart, finalLocation, availabilityEnd, maxRideTime, roadProperties, type, vehicleCostPerDistanceUnit, vehicleCostPerTimeUnit, vehicleCostPerUsage, ownerID, vehicleFlatCostForShortRouteLength, vehicleMaxRouteLengthForFlatCost, vehicleCostPerRoute)
        {
        }

        public void SetType(VehicleType vehicleType)
        {
            Type = vehicleType;
        }
    }
}