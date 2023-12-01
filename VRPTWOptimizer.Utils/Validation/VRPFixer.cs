using CommonGIS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRPTWOptimizer.Interfaces;
using VRPTWOptimizer.Utils.Model;

namespace VRPTWOptimizer.Utils.Validation
{
    public class VRPFixer
    {
        public static bool FixMissingVehicles(VRPDefinitionJSONDTO dto, bool isFixed, List<Vehicle> vehicles, List<ValidationError> errors)
        {
            foreach (var error in errors.Where(e => (e.ErrorCode == ErrorCode.TractorMissing || e.ErrorCode == ErrorCode.CapacityVehicleMissing) && e.ObjectType == VRPObjectType.TransportDTO))
            {
                isFixed = true;
                var transportDTO = (error.ConsideredObject as Transport);
                if (error.ErrorCode == ErrorCode.TractorMissing)
                {
                    if (!dto.Vehicles.Any(v => v.Id == transportDTO.TractorId && v.Type == CommonGIS.Enums.VehicleType.Tractor))
                    {
                        Vehicle templateTractor = vehicles.First(v => v.Type == CommonGIS.Enums.VehicleType.Tractor);
                        VehicleDTO newTractor = new VehicleDTO(transportDTO.TractorId, new double[templateTractor.Capacity.Length],
                            new int[templateTractor.SpecialProperties.Length], new VRPTWOptimizer.Enums.Aggregation[templateTractor.CapacityAggregationType.Length],
                            templateTractor.InitialLocation as CommonGIS.BaseLocation, templateTractor.AvailabilityStart,
                            templateTractor.FinalLocation as CommonGIS.BaseLocation, templateTractor.AvailabilityEnd,
                            templateTractor.MaxRideTime, templateTractor.RoadProperties, CommonGIS.Enums.VehicleType.Tractor,
                            templateTractor.VehicleCostPerDistanceUnit, templateTractor.VehicleCostPerTimeUnit, templateTractor.VehicleCostPerUsage,
                            templateTractor.OwnerID, templateTractor.VehicleFlatCostForShortRouteLength, templateTractor.VehicleMaxRouteLengthForFlatCost,
                            templateTractor.VehicleCostPerRoute);
                        Array.Copy(templateTractor.SpecialProperties, newTractor.SpecialProperties, newTractor.SpecialProperties.Length);
                        Array.Copy(templateTractor.CapacityAggregationType, newTractor.CapacityAggregationType, newTractor.CapacityAggregationType.Length);
                        Array.Copy(templateTractor.Capacity, newTractor.Capacity, newTractor.Capacity.Length);
                        dto.Vehicles.Add(newTractor);
                    }
                }
                else if (error.ErrorCode == ErrorCode.CapacityVehicleMissing)
                {
                    if (!dto.Vehicles.Any(v => v.Id == transportDTO.TrailerTruckId && v.Type != CommonGIS.Enums.VehicleType.Tractor))
                    {
                        bool isTrailer = transportDTO.TractorId != -1 && transportDTO.TractorId != 0;
                        Vehicle templateVehicle = vehicles.First(v => v.Type != CommonGIS.Enums.VehicleType.Tractor);
                        List<VRPTWOptimizer.TransportRequest> currentStack = new();
                        int epCount = VehicleRoadRestrictionProperties.MaxEPCount;
                        double[] maxCapacity = new double[templateVehicle.Capacity.Length];
                        for (int iloc = 0; iloc < transportDTO.Schedule.Count; iloc++)
                        {
                            currentStack.AddRange(transportDTO.Schedule[iloc].LoadedRequestsIds.Select(id => dto.Requests.First(rq => rq.Id == id) as TransportRequest));
                            transportDTO.Schedule[iloc].UnloadedRequestsIds.ForEach(id => { currentStack.RemoveAll(rq => rq.Id == id); });
                            for (int i = 0; i < maxCapacity.Length; i++)
                            {
                                maxCapacity[i] = Math.Max(maxCapacity[i], currentStack.Sum(r => r.Size[i]));
                            }
                            epCount = Math.Min(epCount, currentStack.Min(rq => rq.MaxVehicleSize.EpCount));
                        }
                        VehicleDTO newVehicle = new VehicleDTO(transportDTO.TrailerTruckId, new double[templateVehicle.Capacity.Length],
                            new int[templateVehicle.Capacity.Length], new VRPTWOptimizer.Enums.Aggregation[templateVehicle.CapacityAggregationType.Length],
                            templateVehicle.InitialLocation as CommonGIS.BaseLocation, templateVehicle.AvailabilityStart,
                            templateVehicle.FinalLocation as CommonGIS.BaseLocation, templateVehicle.AvailabilityEnd,
                            templateVehicle.MaxRideTime, new VehicleRoadRestrictionProperties(VehicleRoadRestrictionProperties.MaxGrossVehicleWeight, VehicleRoadRestrictionProperties.MaxHeight,
                            VehicleRoadRestrictionProperties.MaxWidth, epCount, isTrailer ? CommonGIS.Enums.VehicleTypeRouting.TractorWithTrailer : CommonGIS.Enums.VehicleTypeRouting.StraightTruck),
                            isTrailer ? CommonGIS.Enums.VehicleType.SemiTrailer : CommonGIS.Enums.VehicleType.StraightTruck,
                            templateVehicle.VehicleCostPerDistanceUnit, templateVehicle.VehicleCostPerTimeUnit, templateVehicle.VehicleCostPerUsage,
                            templateVehicle.OwnerID, templateVehicle.VehicleFlatCostForShortRouteLength, templateVehicle.VehicleMaxRouteLengthForFlatCost,
                            templateVehicle.VehicleCostPerRoute);
                        Array.Copy(templateVehicle.SpecialProperties, newVehicle.SpecialProperties, newVehicle.SpecialProperties.Length);
                        Array.Copy(templateVehicle.CapacityAggregationType, newVehicle.CapacityAggregationType, newVehicle.CapacityAggregationType.Length);
                        Array.Copy(maxCapacity, newVehicle.Capacity, newVehicle.Capacity.Length);
                        dto.Vehicles.Add(newVehicle);
                    }
                }
            }

            return isFixed;
        }

        public static bool FixRequestLimits(VRPDefinitionJSONDTO dto, bool isFixed, List<ValidationError> errors)
        {
            foreach (var error in errors.Where(e => e.ErrorCode == ErrorCode.TooLargeVehicleSize && e.ObjectType == VRPObjectType.TransportRequest &&
                e.Description.Contains("it violates max allowed vehicle's size")))
            {
                isFixed = true;
                var originalMaxVehicleSize = (error.ConsideredObject as TransportRequest).MaxVehicleSize;
                var fixedMaxVehicleSize = new VehicleRoadRestrictionProperties(
                    originalMaxVehicleSize.GrossVehicleWeight,
                    originalMaxVehicleSize.Height,
                    originalMaxVehicleSize.Width,
                    (int)error.ExpectedValue,
                    originalMaxVehicleSize.VehicleType
                    );
                var originalRequest = dto.Requests.First(r => r.Id.ToString() == error.ObjectId);
                dto.Requests.Remove(originalRequest);
                dto.Requests.Add(
                new RequestDTO(originalRequest.Id, originalRequest.Size, originalRequest.NecessaryVehicleSpecialProperties,
                    originalRequest.PackageCount, originalRequest.PackageCountForImediateRetrieval,
                    originalRequest.PickupLocation as BaseLocation, originalRequest.PickupAvailableTimeWindowStart, originalRequest.PickupPreferedTimeWindowStart,
                    originalRequest.PickupPreferedTimeWindowEnd, originalRequest.PickupAvailableTimeWindowEnd,
                    originalRequest.DeliveryLocation as BaseLocation, originalRequest.DeliveryAvailableTimeWindowStart, originalRequest.DeliveryPreferedTimeWindowStart,
                    originalRequest.DeliveryPreferedTimeWindowEnd, originalRequest.DeliveryAvailableTimeWindowEnd, originalRequest.Type,
                    originalRequest.CargoTypes, fixedMaxVehicleSize, originalRequest.RestrictedCargoTypes, originalRequest.MutuallyExclusiveRequestsIdTimeBufferDict,
                    originalRequest.RevenueValue, originalRequest.Name));
            }
            return isFixed;
        }

            public static bool FixRequestSize(bool isFixed, List<ValidationError> errors)
        {
            foreach (var error in errors.Where(e => e.ErrorCode == ErrorCode.ExceededCapacity && e.ObjectType == VRPObjectType.Route))
            {
                isFixed = true;
                Dictionary<TransportRequest, double[]> capacityToBeSubtracted = new();
                var route = (error.ConsideredObject as IRoute);
                List<VRPTWOptimizer.TransportRequest> currentStack = new();
                for (int iloc = 0; iloc < route.VisitedLocations.Count; iloc++)
                {
                    route.UnloadedRequests[iloc].ForEach(rq => { currentStack.Remove(rq); });
                    currentStack.AddRange(route.LoadedRequests[iloc]);
                    foreach (var loadedRequest in route.LoadedRequests[iloc])
                    {
                        capacityToBeSubtracted.Add(loadedRequest, new double[route.Vehicle.Capacity.Length]);
                    }
                    for (int i = 0; i < route.Vehicle.Capacity.Length; i++)
                    {
                        double capacitySumDim = currentStack.Sum(rq => rq.Size[i]);
                        int currentStackSize = currentStack.Count;
                        if (capacitySumDim > route.Vehicle.Capacity[i])
                        {
                            double overhead = capacitySumDim - route.Vehicle.Capacity[i];
                            foreach (var stackRequest in currentStack)
                            {
                                capacityToBeSubtracted[stackRequest][i] = Math.Max(capacityToBeSubtracted[stackRequest][i], overhead * stackRequest.Size[i] / capacitySumDim);
                            }
                        }
                    }
                }
                foreach (var requestToBeSubtracted in capacityToBeSubtracted)
                {
                    for (int i = 0; i < requestToBeSubtracted.Value.Length; i++)
                    {
                        if (requestToBeSubtracted.Value[i] > 0)
                        {
                            requestToBeSubtracted.Key.Size[i] -= requestToBeSubtracted.Value[i];
                            requestToBeSubtracted.Key.Size[i] = Math.Floor(requestToBeSubtracted.Key.Size[i] * 4.0) / 4.0;
                        }
                    }
                }
            }

            return isFixed;
        }

        public static bool FixRequestsProperties(VRPDefinitionJSONDTO dto, bool isFixed, List<ValidationError> errors)
        {
            foreach (var error in errors.Where(e => e.ErrorCode == ErrorCode.IdenticalDeliveryAndPickup && e.ObjectType == VRPObjectType.TransportRequest))
            {
                isFixed = true;
                dto.Requests.RemoveAll(r => r.Id.ToString() == error.ObjectId);
            }
            foreach (var error in errors.Where(e => e.ErrorCode == ErrorCode.ZeroPackages && e.ObjectType == VRPObjectType.TransportRequest))
            {
                TransportRequest zeroPackageRequest = (error.ConsideredObject as TransportRequest);
                dto.Requests.RemoveAll(r => r.Id.ToString() == error.ObjectId);
                dto.Requests.Add(
                new RequestDTO(zeroPackageRequest.Id, zeroPackageRequest.Size, zeroPackageRequest.NecessaryVehicleSpecialProperties,
                    Math.Max((int)Math.Ceiling(zeroPackageRequest.Size.Min()), 1), zeroPackageRequest.PackageCountForImediateRetrieval,
                    zeroPackageRequest.PickupLocation as BaseLocation, zeroPackageRequest.PickupAvailableTimeWindowStart, zeroPackageRequest.PickupPreferedTimeWindowStart,
                    zeroPackageRequest.PickupPreferedTimeWindowEnd, zeroPackageRequest.PickupAvailableTimeWindowEnd,
                    zeroPackageRequest.DeliveryLocation as BaseLocation, zeroPackageRequest.DeliveryAvailableTimeWindowStart, zeroPackageRequest.DeliveryPreferedTimeWindowStart,
                    zeroPackageRequest.DeliveryPreferedTimeWindowEnd, zeroPackageRequest.DeliveryAvailableTimeWindowEnd, zeroPackageRequest.Type,
                    zeroPackageRequest.CargoTypes, zeroPackageRequest.MaxVehicleSize, zeroPackageRequest.RestrictedCargoTypes, zeroPackageRequest.MutuallyExclusiveRequestsIdTimeBufferDict,
                    zeroPackageRequest.RevenueValue, zeroPackageRequest.Name)
                );
            }
            return isFixed;
        }

        public static bool FixVehicleSize(VRPDefinitionJSONDTO dto, bool isFixed, List<Vehicle> vehicles, List<ValidationError> errors)
        {
            foreach (var error in errors.Where(e => e.ErrorCode == ErrorCode.TooLargeVehicleSize && e.ObjectType == VRPObjectType.Vehicle))
            {
                Vehicle vehicleToFix = error.ConsideredObject as Vehicle;
                VehicleDTO newVehicle = new VehicleDTO(
                    vehicleToFix.Id, vehicleToFix.Capacity, vehicleToFix.SpecialProperties,
                    vehicleToFix.CapacityAggregationType, new BaseLocation(vehicleToFix.InitialLocation), vehicleToFix.AvailabilityStart,
                    new BaseLocation(vehicleToFix.FinalLocation), vehicleToFix.AvailabilityEnd, vehicleToFix.MaxRideTime,
                    VehicleRoadRestrictionProperties.BoundProperties(vehicleToFix.RoadProperties), vehicleToFix.Type, vehicleToFix.VehicleCostPerDistanceUnit, vehicleToFix.VehicleCostPerTimeUnit,
                    vehicleToFix.VehicleCostPerUsage, vehicleToFix.OwnerID, vehicleToFix.VehicleFlatCostForShortRouteLength, vehicleToFix.VehicleMaxRouteLengthForFlatCost, vehicleToFix.VehicleCostPerRoute);
                isFixed = true;
                vehicles.RemoveAll(v => v.Id == newVehicle.Id);
                vehicles.Add(newVehicle);
            }
            return isFixed;
        }

        public static bool FixWrongVehicleType(VRPDefinitionJSONDTO dto, bool isFixed, List<ValidationError> errors)
        {
            foreach (var error in errors.Where(e => e.ErrorCode == ErrorCode.TractorAssignedAsCapacityVehicle && e.ObjectType == VRPObjectType.Route))
            {
                isFixed = true;
                var route = (error.ConsideredObject as IRoute);
                var routeVehicle = dto.Vehicles.First(v => v.Id == route.Vehicle.Id);
                routeVehicle.SetType(CommonGIS.Enums.VehicleType.StraightTruck);
                List<VRPTWOptimizer.TransportRequest> currentStack = new();
                double[] maxCapacity = new double[route.Vehicle.Capacity.Length];
                for (int iloc = 0; iloc < route.VisitedLocations.Count; iloc++)
                {
                    route.UnloadedRequests[iloc].ForEach(rq => { currentStack.Remove(rq); });
                    currentStack.AddRange(route.LoadedRequests[iloc]);
                    for (int i = 0; i < maxCapacity.Length; i++)
                    {
                        maxCapacity[i] = Math.Max(maxCapacity[i], currentStack.Sum(r => r.Size[i]));
                        routeVehicle.Capacity[i] = maxCapacity[i];
                    }
                }
            }
            return isFixed;
        }

        public static bool RemoveTransfersAndLoops(VRPDefinitionJSONDTO dto, bool isFixed)
        {
            if (dto.Requests.RemoveAll(r => r.DeliveryLocation.Id == r.PickupLocation.Id) > 0)
            {
                //removing loops
                isFixed = true;
            }
            /*
            if (dto.Requests.RemoveAll(r => r.DeliveryLocation.Id != dto.DepotId && r.PickupLocation.Id != dto.DepotId) > 0)
            {
                //removing transfers
                isFixed = true;
            }
            */
            return isFixed;
        }

        public static bool TrimLeftInHistoric(VRPDefinitionJSONDTO dto, bool isFixed)
        {
            if (dto.VIATMSSolution != null && dto.VIATMSSolution.LeftRequestIds != null)
            {
                foreach (var id in dto.VIATMSSolution.LeftRequestIds)
                {
                    dto.Requests.RemoveAll(rq => rq.Id.ToString() == id);
                    isFixed = true;
                }
                dto.VIATMSSolution.LeftRequestIds.Clear();
            }
            return isFixed;
        }

        public static bool FillInDrivers(VRPDefinitionJSONDTO dto, bool isFixed)
        {
            if (dto.Drivers == null || dto.Drivers.Count == 0)
            {
                //HACK for drivers management without drivers
                var driverAvai = (dto.ZeroHour.Date.AddHours(20) - dto.ZeroHour).TotalSeconds;
                var relevantVehicles = dto.Vehicles.Where(vh => vh.Type != CommonGIS.Enums.VehicleType.SemiTrailer).ToList();
                int countVehicles = relevantVehicles.Count;
                for (int i = 0; i < countVehicles; i++)
                {
                    dto.Drivers.Add(new DriverDTO(
                        i + 1,
                        driverAvai,
                        driverAvai + VRPCostFunction.SingleDriverWorkTime,
                        new int[] { relevantVehicles[i].Id },
                        new int[] { relevantVehicles[i].OwnerID }
                        ));
                    dto.Drivers.Add(new DriverDTO(
                        i + 1 + countVehicles,
                        driverAvai + 12 * 3600,
                        driverAvai + 12 * 3600 + VRPCostFunction.SingleDriverWorkTime,
                        new int[] { relevantVehicles[i].Id },
                        new int[] { relevantVehicles[i].OwnerID }
                        ));
                    dto.Drivers.Add(new DriverDTO(
                        i + 1,
                        driverAvai + 24 * 3600,
                        driverAvai + 24 * 3600 + VRPCostFunction.SingleDriverWorkTime,
                        new int[] { relevantVehicles[i].Id },
                        new int[] { relevantVehicles[i].OwnerID }
                        ));
                }
                isFixed = true;
            }

            return isFixed;
        }

    }
}