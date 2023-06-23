using CommonGIS;
using Newtonsoft.Json;
using System.Collections.Generic;
using VRPTWOptimizer.Enums;

namespace VRPTWOptimizer.Utils.Model

{
    public class RequestDTO : TransportRequest
    {
        [JsonConstructor]
        public RequestDTO(
            int id,
            double[] size,
            int[] necessaryVehicleSpecialProperties,
            int packageCount,
            int packageCountForImediateRetrieval,
            BaseLocation pickupLocation,
            double pickupAvailableTimeWindowStart,
            double pickupPreferedTimeWindowStart,
            double pickupPreferedTimeWindowEnd,
            double pickupAvailableTimeWindowEnd,
            BaseLocation deliveryLocation,
            double deliveryAvailableTimeWindowStart,
            double deliveryPreferedTimeWindowStart,
            double deliveryPreferedTimeWindowEnd,
            double deliveryAvailableTimeWindowEnd,
            RequestType type,
            int[] cargoTypes,
            VehicleRoadRestrictionProperties maxVehicleSize,
            int[] restrictedGoodsTypes,
            Dictionary<int, double> mutuallyExclusiveRequestsIdTimeBufferDict,
            double revenueValue,
            string name) : base(
                id,
                size,
                necessaryVehicleSpecialProperties,
                packageCount,
                packageCountForImediateRetrieval,
                pickupLocation,
                pickupAvailableTimeWindowStart,
                pickupPreferedTimeWindowStart,
                pickupPreferedTimeWindowEnd,
                pickupAvailableTimeWindowEnd,
                deliveryLocation,
                deliveryAvailableTimeWindowStart,
                deliveryPreferedTimeWindowStart,
                deliveryPreferedTimeWindowEnd,
                deliveryAvailableTimeWindowEnd,
                type,
                cargoTypes,
                maxVehicleSize,
                restrictedGoodsTypes ?? new int[0],
                mutuallyExclusiveRequestsIdTimeBufferDict,
                revenueValue,
                name)
        {
        }
    }
}