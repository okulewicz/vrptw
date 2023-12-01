using CommonGIS;
using CommonGIS.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using VRPTWOptimizer.Interfaces;
using VRPTWOptimizer.Utils.Model;
using VRPTWOptimizer.Utils.Provider;

namespace VRPTWOptimizer.Utils.Validation
{
    /// <summary>
    /// Class verifying consistency and sensibility of VRP definitions and solutions
    /// </summary>
    public class VRPValidator
    {
        private const int MaxSpeedValue = 200;

        private static void ValidateDistance(Distance distance, ref List<ValidationError> errors)
        {
            StraightLineDistanceProvider straightLineDistanceProvider = new();
            if (distance.Length / distance.Time >= MaxSpeedValue)
            {
                errors.Add(new ValidationError(VRPErrorType.ImprobableValue,
                                               VRPObjectType.Distance,
                                               string.Format($"{distance.FromId}-{distance.ToId}"),
                                               string.Format($"Distance from {distance.FromId} to {distance.ToId} seems to have nonconformat units with time={distance.Time} and distance={distance.Length}"),
                                               distance,
                                               ErrorCode.ImpossibleDriveTime,
                                               MaxSpeedValue
                                               ));
            }
        }

        private static IEnumerable<ValidationError> ValidateRouteLoadUnloadLocationCompliance(IRoute route)
        {
            List<ValidationError> errors = new List<ValidationError>();
            for (int i = 0; i < route.VisitedLocations.Count; i++)
            {
                foreach (var loadedRequest in route.LoadedRequests[i])
                {
                    if (loadedRequest.PickupLocation.Id != route.VisitedLocations[i].Id)
                    {
                        errors.Add(new ValidationError(VRPErrorType.InfeasibleSolution,
                                                       VRPObjectType.Route,
                                                       string.Format($"{route.Vehicle.Id}-{Math.Round(route.ArrivalTimes[i])}"),
                                                       string.Format($"Route of vehicle {route.Vehicle.Id} starting on {Math.Round(route.ArrivalTimes[i])} incorrectly loads request {loadedRequest.Id} at location {route.VisitedLocations[i].Id} instead of {loadedRequest.PickupLocation.Id}"),
                                                       route,
                                                       ErrorCode.ImpossibleOperation,
                                                       loadedRequest.PickupLocation.Id
                                                       ));
                    }
                }
                foreach (var unloadedRequest in route.UnloadedRequests[i])
                {
                    if (unloadedRequest.DeliveryLocation.Id != route.VisitedLocations[i].Id)
                    {
                        errors.Add(new ValidationError(VRPErrorType.InfeasibleSolution,
                                                       VRPObjectType.Route,
                                                       string.Format($"{route.Vehicle.Id}-{Math.Round(route.ArrivalTimes[i])}"),
                                                       string.Format($"Route of vehicle {route.Vehicle.Id} starting on {Math.Round(route.ArrivalTimes[i])} incorrectly unloads request {unloadedRequest.Id} at location {route.VisitedLocations[i].Id} instead of {unloadedRequest.DeliveryLocation.Id}"),
                                                       route,
                                                       ErrorCode.ImpossibleOperation,
                                                       unloadedRequest.DeliveryLocation.Id
                                                       ));
                    }
                }
            }
            return errors;
        }

        private static IEnumerable<ValidationError> ValidateSolutionCompletness(List<IRoute> routes, List<TransportRequest> leftRequests, List<TransportRequest> requests)
        {
            List<ValidationError> errors = new List<ValidationError>();
            var loadedIds = routes.SelectMany(rt => rt.LoadedRequests.SelectMany(lr => lr.Select(r => r.Id))).OrderBy(id => id).ToList();
            var unloadedIds = routes.SelectMany(rt => rt.UnloadedRequests.SelectMany(lr => lr.Select(r => r.Id))).OrderBy(id => id).ToList();
            if (loadedIds.Count != unloadedIds.Count)
            {
                errors.Add(new ValidationError(
                    VRPErrorType.InfeasibleSolution,
                    VRPObjectType.RoutesSet,
                    "routes",
                    $"Different number of requests has been loaded {loadedIds.Count} than unloaded {unloadedIds.Count}",
                    routes,
                    ErrorCode.LeakyRoutes,
                    unloadedIds.Count
                    ));
            }
            foreach (var request in leftRequests)
            {
                if (loadedIds.Contains(request.Id) || unloadedIds.Contains(request.Id))
                {
                    errors.Add(new ValidationError(
                        VRPErrorType.InfeasibleSolution,
                        VRPObjectType.TransportRequest,
                        request.Id.ToString(),
                        $"Request {request.Id} is contained both in left requests and somewhere in the routes",
                        request,
                        ErrorCode.LeakyRoutes,
                        null
                        ));
                }
            }
            foreach (var request in requests)
            {
                if (!loadedIds.Contains(request.Id) && leftRequests.Count(rq => rq.Id == request.Id) != 1)
                {
                    errors.Add(new ValidationError(
                        VRPErrorType.InfeasibleSolution,
                        VRPObjectType.TransportRequest,
                        request.Id.ToString(),
                        $"Request {request.Id} is never loaded or in left requests",
                        request,
                        ErrorCode.LeakyRoutes,
                        request.Id
                        ));
                }
                if (!unloadedIds.Contains(request.Id) && leftRequests.Count(rq => rq.Id == request.Id) != 1)
                {
                    errors.Add(new ValidationError(
                        VRPErrorType.InfeasibleSolution,
                        VRPObjectType.TransportRequest,
                        request.Id.ToString(),
                        $"Request {request.Id} is never unloaded or in left requests",
                        request,
                        ErrorCode.LeakyRoutes,
                        request.Id
                        ));
                }
            }
            return errors;
        }

        /// <summary>
        /// Method for running complete validation against a solution and definition
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="leftRequests"></param>
        /// <param name="requests"></param>
        /// <param name="vehicles"></param>
        /// <param name="distances"></param>
        /// <returns></returns>
        public static List<ValidationError> ValidateAll(List<IRoute> routes, List<TransportRequest> leftRequests, List<TransportRequest> requests, List<Vehicle> vehicles, List<Distance> distances)
        {
            DistanceMatrixProvider distanceProvider = new DistanceMatrixProvider(distances);
            var errors = new List<ValidationError>();
            errors.AddRange(ValidateAll(routes, leftRequests, requests, vehicles, distanceProvider));
            if (distances != null)
            {
                errors.AddRange(ValidateDistances(distances));
            }
            return errors;
        }

        /// <summary>
        /// Method for running complete validation against a solution and definition
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="leftRequests"></param>
        /// <param name="requests"></param>
        /// <param name="vehicles"></param>
        /// <param name="distanceProvider"></param>
        /// <returns></returns>
        public static List<ValidationError> ValidateAll(List<IRoute> routes, List<TransportRequest> leftRequests, List<TransportRequest> requests, List<Vehicle> vehicles, IDistanceProvider distanceProvider)
        {
            var errors = new List<ValidationError>();
            errors.AddRange(ValidateProblemData(requests, vehicles, distanceProvider));
            errors.AddRange(ValidateSolutionData(routes, leftRequests, requests));
            return errors;
        }

        /// <summary>
        /// Obsolete method for running complete validation against a solution and definition
        /// </summary>
        [Obsolete]
        public static List<ValidationError> ValidateAll(List<IRoute> routes, List<TransportRequest> leftRequests, List<TransportRequest> requests, List<Vehicle> vehicles)
        {
            return ValidateAll(routes, leftRequests, requests, vehicles, new StraightLineDistanceProvider());
        }

        /// <summary>
        /// Verifying consistency of routes against existence of resources
        /// </summary>
        /// <param name="transportDTO"></param>
        /// <param name="vehicles"></param>
        /// <param name="drivers"></param>
        /// <returns></returns>
        public static IEnumerable<ValidationError> ValidateDeserializedRoutesAgainstVehiclesAndDriversExistence(Transport transportDTO, List<Vehicle> vehicles, List<Driver> drivers)
        {
            List<ValidationError> errors = new();
            if (!vehicles.Any(v => v.Id == transportDTO.TrailerTruckId && (v.Type == CommonGIS.Enums.VehicleType.StraightTruck || v.Type == CommonGIS.Enums.VehicleType.SemiTrailer)))
            {
                errors.Add(new ValidationError(
                    VRPErrorType.InfeasibleSolution,
                    VRPObjectType.TransportDTO,
                    transportDTO.TransportId.ToString(),
                    $"There is no trailer or truck with {transportDTO.TrailerTruckId} in the vehicles collection",
                    transportDTO,
                    ErrorCode.CapacityVehicleMissing,
                    transportDTO.TrailerTruckId
                    ));
            }
            if (transportDTO.TractorId != -1 && transportDTO.TractorId != 0)
            {
                if (!vehicles.Any(v => v.Id == transportDTO.TractorId && v.Type == CommonGIS.Enums.VehicleType.Tractor))
                {
                    errors.Add(new ValidationError(
                        VRPErrorType.InfeasibleSolution,
                        VRPObjectType.TransportDTO,
                        transportDTO.TransportId.ToString(),
                        $"There is no tractor with {transportDTO.TractorId} in the vehicles collection",
                        transportDTO,
                        ErrorCode.TractorMissing,
                        transportDTO.TractorId
                        ));
                }
            }
            return errors;
        }

        /// <summary>
        /// Checks distances for consistency between distance and time
        /// </summary>
        /// <param name="distances"></param>
        /// <returns></returns>
        public static List<ValidationError> ValidateDistances(List<Distance> distances)
        {
            List<ValidationError> errors = new();
            foreach (var distance in distances)
            {
                ValidateDistance(distance, ref errors);
            }
            return errors;
        }

        /// <summary>
        /// Method aggregating methods verifying VRP problem definition data consistency
        /// </summary>
        /// <param name="requests"></param>
        /// <param name="vehicles"></param>
        /// <param name="distanceProvider"></param>
        /// <returns></returns>
        public static List<ValidationError> ValidateProblemData(List<TransportRequest> requests, List<Vehicle> vehicles, IDistanceProvider distanceProvider)
        {
            List<ValidationError> errors = new();
            foreach (var vehicle in vehicles)
            {
                errors.AddRange(ValidateVehicleProperties(vehicle));
            }
            if (distanceProvider is DistanceMatrixProvider)
            {
                errors.AddRange(ValidateDistanceMatrixConnectivity((distanceProvider as DistanceMatrixProvider).StoredDistances, requests));
                errors.AddRange(ValidateDistances((distanceProvider as DistanceMatrixProvider).StoredDistances));
            }
            foreach (var request in requests)
            {
                errors.AddRange(ValidateRequestProperties(request));
                errors.AddRange(ValidateRequestAgainstVehiclesProperties(request, vehicles, distanceProvider));
            }
            return errors;
        }

        public static IEnumerable<ValidationError> ValidateDistanceMatrixConnectivity(List<Distance> storedDistances, List<TransportRequest> requests)
        {
            List<ValidationError> errors = new();
            foreach (var request in requests)
            {
                string idFrom = request.PickupLocation.Id;
                string idTo = request.DeliveryLocation.Id;
                errors.AddRange(CheckPairOfLocations(storedDistances, request, idFrom, idTo));
                errors.AddRange(CheckPairOfLocations(storedDistances, request, idTo, idFrom));
            }
            return errors;
        }

        private static List<ValidationError> CheckPairOfLocations(List<Distance> storedDistances, TransportRequest request, string idFrom, string idTo)
        {
            List<ValidationError> errors = new();
            if (!storedDistances.Any(d => d.FromId == idFrom && d.ToId == idTo))
            {
                errors.Add(new ValidationError(VRPErrorType.ImpossibleProblem,
                           VRPObjectType.TransportRequest,
                           string.Format($"{request.Id}"),
                           string.Format($"There is no way to serve request {request.Id} as there is no route from {idFrom} to {idTo}"),
                           request,
                           ErrorCode.WrongProperties,
                           $"{idFrom};{ idTo}"
                           ));
            }
            return errors;
        }

        /// <summary>
        /// Method verifying if it is possible to serve every request with existing set of vehicles
        /// </summary>
        /// <param name="request"></param>
        /// <param name="vehicles"></param>
        /// <param name="distanceProvider"></param>
        /// <returns></returns>
        public static IEnumerable<ValidationError> ValidateRequestAgainstVehiclesProperties(TransportRequest request, List<Vehicle> vehicles, IDistanceProvider distanceProvider)
        {
            var errors = new List<ValidationError>();
            if (!vehicles.Any(v => request.MaxVehicleSize.DoesVehicleFitIntoRestrictions(v.RoadProperties) && v.CanHandleRequest(request)))
            {
                errors.Add(new ValidationError(VRPErrorType.ImprobableValue,
                                               VRPObjectType.TransportRequest,
                                               string.Format($"{request.Id}"),
                                               string.Format($"No vehicle can serve request {request.Id} with {string.Join(',', request.Size.Select(s => s.ToString(CultureInfo.InvariantCulture)))} size and vehicle limit of {request.MaxVehicleSize.EpCount}"),
                                               request,
                                               ErrorCode.RequestPropertiesExcludeAllVehicles,
                                               $"{string.Join(',', request.Size.Select(s => s.ToString(CultureInfo.InvariantCulture)))};{request.MaxVehicleSize.EpCount}"
                                               ));
            }
            bool impossiblePreferredPickupTime = true;
            bool impossiblePreferredDeliveryTime = true;
            bool impossiblePickupTime = true;
            bool impossibleDeliveryTime = true;
            bool impossibleReturnTime = true;
            double earliestArrival = double.MaxValue;
            double earliestArrivalAtDelivery = double.MaxValue;
            double earliestReturnTime = double.MaxValue;
            foreach (var vehicle in vehicles)
            {
                earliestArrival = Math.Min(earliestArrival, vehicle.AvailabilityStart + distanceProvider.GetDistance(vehicle.InitialLocation, request.PickupLocation, vehicle.RoadProperties).Time);
                earliestArrivalAtDelivery = Math.Min(earliestArrivalAtDelivery, Math.Max(earliestArrival, request.PickupAvailableTimeWindowStart) + distanceProvider.GetDistance(request.PickupLocation, request.DeliveryLocation, vehicle.RoadProperties).Time);
                earliestReturnTime = Math.Min(earliestReturnTime, Math.Max(earliestArrivalAtDelivery, request.DeliveryAvailableTimeWindowStart) + distanceProvider.GetDistance(request.DeliveryLocation, vehicle.FinalLocation, vehicle.RoadProperties).Time);
                if (earliestArrival <= request.PickupPreferedTimeWindowEnd)
                {
                    impossiblePreferredPickupTime = false;
                }
                if (earliestArrivalAtDelivery <= request.DeliveryPreferedTimeWindowEnd)
                {
                    impossiblePreferredDeliveryTime = false;
                }
                if (earliestArrival <= request.PickupAvailableTimeWindowEnd)
                {
                    impossiblePickupTime = false;
                }
                if (earliestArrivalAtDelivery <= request.DeliveryAvailableTimeWindowEnd)
                {
                    impossibleDeliveryTime = false;
                }
                if (earliestReturnTime <= vehicle.AvailabilityEnd)
                {
                    impossibleReturnTime = false;
                }
                if (!impossiblePreferredPickupTime && !impossiblePreferredDeliveryTime && !impossibleReturnTime && !impossiblePickupTime && !impossibleDeliveryTime)
                {
                    break;
                }
            }
            if (impossiblePreferredPickupTime)
            {
                errors.Add(new ValidationError(VRPErrorType.ImprobableValue,
                               VRPObjectType.TransportRequest,
                               string.Format($"{request.Id}"),
                               string.Format($"No vehicle can get to request {request.Id} pickup location {request.PickupLocation.Id} before preffered {request.PickupPreferedTimeWindowEnd} time"),
                               request,
                               ErrorCode.ImpossiblePreferredTimeWindow,
                               earliestArrival
                               ));
            }
            if (impossiblePreferredDeliveryTime)
            {
                errors.Add(new ValidationError(VRPErrorType.ImprobableValue,
                                           VRPObjectType.TransportRequest,
                                           string.Format($"{request.Id}"),
                                           string.Format($"No vehicle can get to request {request.Id} delivery location {request.DeliveryLocation.Id} before preffered {request.DeliveryPreferedTimeWindowEnd} time"),
                               request,
                               ErrorCode.ImpossiblePreferredTimeWindow,
                               earliestArrivalAtDelivery));
            }
            if (impossiblePickupTime)
            {
                errors.Add(new ValidationError(VRPErrorType.ImpossibleProblem,
                               VRPObjectType.TransportRequest,
                               string.Format($"{request.Id}"),
                               string.Format($"No vehicle can get to request {request.Id} pickup location {request.PickupLocation.Id} before {request.PickupAvailableTimeWindowEnd}"),
                               request,
                               ErrorCode.ImpossibleTimeWindow,
                               earliestArrival));
            }
            if (impossibleDeliveryTime)
            {
                errors.Add(new ValidationError(VRPErrorType.ImpossibleProblem,
                                           VRPObjectType.TransportRequest,
                                           string.Format($"{request.Id}"),
                                           string.Format($"No vehicle can get to request {request.Id} delivery location {request.DeliveryLocation.Id} before {request.DeliveryAvailableTimeWindowEnd}"),
                               request,
                               ErrorCode.ImpossibleTimeWindow,
                               earliestArrivalAtDelivery));
            }
            if (impossibleReturnTime)
            {
                errors.Add(new ValidationError(VRPErrorType.ImpossibleProblem,
                                           VRPObjectType.TransportRequest,
                                           string.Format($"{request.Id}"),
                                           string.Format($"No vehicle can serve request {request.Id} and get back to its final location on time"),
                               request,
                               ErrorCode.ImpossibleTimeWindow,
                               earliestReturnTime));
            }
            return errors;
        }

        /// <summary>
        /// Checks request for improbable data values
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<ValidationError> ValidateRequestProperties(TransportRequest request)
        {
            List<ValidationError> errors = new List<ValidationError>();
            if (request.Type != Enums.RequestType.GoodsDistribution &&
                request.Type != Enums.RequestType.Backhauling &&
                request.Type != Enums.RequestType.ContainerRetrieval)
            {
                errors.Add(new ValidationError(
                    VRPErrorType.ImprobableValue,
                    VRPObjectType.TransportRequest,
                    request.Id.ToString(),
                    $"Request {request.Id} is of unknown {request.Type} type non-existent in VRPTWOptimizer.Enums.RequestType",
                    request,
                    ErrorCode.WrongType,
                    request.Type
                    ));
            }
            if (request.PackageCount <= 0)
            {
                errors.Add(new ValidationError(
                    VRPErrorType.ImprobableValue,
                    VRPObjectType.TransportRequest,
                    request.Id.ToString(),
                    $"Request {request.Id} has 0 packages to pickup and deliver",
                    request,
                    ErrorCode.ZeroPackages,
                    1
                    ));
            }
            if (request.PickupLocation.Id == request.DeliveryLocation.Id)
            {
                errors.Add(new ValidationError(
                    VRPErrorType.ImprobableValue,
                    VRPObjectType.TransportRequest,
                    request.Id.ToString(),
                    $"Request {request.Id} has identical pickup and delivery location {request.DeliveryLocation.Id}",
                    request,
                    ErrorCode.IdenticalDeliveryAndPickup,
                    null
                    ));
            }
            return errors;
        }

        /// <summary>
        /// Checks if route vehicles are assigned properly
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public static IEnumerable<ValidationError> ValidateRouteAssignmentCompletness(IRoute route)
        {
            List<ValidationError> errors = new List<ValidationError>();
            if (route.Vehicle.Type == CommonGIS.Enums.VehicleType.SemiTrailer && route.VehicleTractor == null)
            {
                errors.Add(new ValidationError(
                    VRPErrorType.InfeasibleSolution,
                    VRPObjectType.Route,
                    string.Format($"{route.Vehicle.Id}-{Math.Round(route.ArrivalTimes[0])}"),
                    string.Format($"Route starting on {Math.Round(route.ArrivalTimes[0])} of vehicle {route.Vehicle.Id} does not have a tractor unit although {route.Vehicle.Id} is a semi-trailer"),
                    route,
                    ErrorCode.TractorMissing,
                    null
                    ));
            }
            if (route.Vehicle.Type == CommonGIS.Enums.VehicleType.Tractor)
            {
                errors.Add(new ValidationError(
                    VRPErrorType.InfeasibleSolution,
                    VRPObjectType.Route,
                    string.Format($"{route.Vehicle.Id}-{Math.Round(route.ArrivalTimes[0])}"),
                    string.Format($"Route starting on {Math.Round(route.ArrivalTimes[0])} has a tractor vehicle {route.Vehicle.Id} assigned as truck or semi-trailer unit"),
                    route,
                    ErrorCode.TractorAssignedAsCapacityVehicle,
                    null
                    ));
            }
            return errors;
        }

        /// <summary>
        /// Checks if vehicle capacity is not exceeded at any point within route
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public static List<ValidationError> ValidateRouteCapacityConstraints(IRoute route)
        {
            List<ValidationError> errors = new List<ValidationError>();
            List<VRPTWOptimizer.TransportRequest> currentStack = new();
            List<VRPTWOptimizer.TransportRequest> handledRequests = new();
            for (int iloc = 0; iloc < route.VisitedLocations.Count; iloc++)
            {
                route.UnloadedRequests[iloc].ForEach(rq => { currentStack.Remove(rq); });
                currentStack.AddRange(route.LoadedRequests[iloc]);
                handledRequests.AddRange(route.LoadedRequests[iloc]);
                for (int i = 0; i < route.Vehicle.Capacity.Length; i++)
                {
                    if (route.Vehicle.CapacityAggregationType[i] == Enums.Aggregation.Sum && currentStack.Sum(rq => rq.Size[i]) > route.Vehicle.Capacity[i])
                    {
                        errors.Add(new ValidationError(
                            VRPErrorType.InfeasibleSolution,
                            VRPObjectType.Route,
                            string.Format($"{route.Vehicle.Id}-{Math.Round(route.ArrivalTimes[0])}"),
                            string.Format($"Route starting on {Math.Round(route.ArrivalTimes[0])} of vehicle {route.Vehicle.Id} validates capacity constraint with {currentStack.Sum(rq => rq.Size[i])} over {route.Vehicle.Capacity[i]} after load in {route.VisitedLocations[iloc].Id}"),
                            route,
                            ErrorCode.ExceededCapacity,
                            route.Vehicle.Capacity[i]
                            ));
                    }
                    if (route.Vehicle.CapacityAggregationType[i] == Enums.Aggregation.Max && currentStack.Any() && currentStack.Max(rq => rq.Size[i]) > route.Vehicle.Capacity[i])
                    {
                        errors.Add(new ValidationError(
                            VRPErrorType.InfeasibleSolution,
                            VRPObjectType.Route,
                            string.Format($"{route.Vehicle.Id}-{Math.Round(route.ArrivalTimes[0])}"),
                            string.Format($"Route starting on {Math.Round(route.ArrivalTimes[0])} of vehicle {route.Vehicle.Id} validates capacity constraint with {currentStack.Max(rq => rq.Size[i])} over {route.Vehicle.Capacity[i]} after load in {route.VisitedLocations[iloc].Id}"),
                            route,
                            ErrorCode.ExceededCapacity,
                            route.Vehicle.Capacity[i]
                            ));
                    }
                }
            }
            foreach (var handledRequest in handledRequests)
            {
                if (!handledRequest.MaxVehicleSize.DoesVehicleFitIntoRestrictions(route.Vehicle.RoadProperties))
                {
                    errors.Add(new ValidationError(
                        VRPErrorType.InfeasibleSolution,
                        VRPObjectType.TransportRequest,
                        $"{handledRequest.Id}",
                        string.Format($"Vehicle {route.Vehicle.Id} should not serve {handledRequest.Id} as it violates max allowed vehicle's size of {handledRequest.MaxVehicleSize.EpCount} by having {route.Vehicle.RoadProperties.EpCount} size"),
                        handledRequest,
                        ErrorCode.TooLargeVehicleSize,
                        route.Vehicle.RoadProperties.EpCount
                        ));
                }
            }
            return errors;
        }

        /// <summary>
        /// Checks if routes served by the same vehicle do not overlap
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="arrivalsCanOverlapDepartures">In case of tractor vehicle it may be allowed to switch trailers before/after unloading/loading finishes/starts</param>
        /// <returns></returns>
        public static List<ValidationError> ValidateRoutesTimeOverlap(IEnumerable<IRoute> routes, bool arrivalsCanOverlapDepartures)
        {
            List<ValidationError> errors = new List<ValidationError>();
            foreach (var route1 in routes)
            {
                foreach (var route2 in routes)
                {
                    if (route1 != route2)
                    {
                        if (!arrivalsCanOverlapDepartures)
                        {
                            if (route1.DepartureTimes[^1] > route2.ArrivalTimes[0] && route1.ArrivalTimes[0] < route2.DepartureTimes[^1])
                            {
                                int driverId = route1.VehicleDriver != null ? route1.VehicleDriver.Id : -1;
                                int tractorId = route1.VehicleTractor != null ? route1.VehicleTractor.Id : -1;
                                errors.Add(new ValidationError(
                                    VRPErrorType.InfeasibleSolution,
                                    VRPObjectType.Route,
                                    string.Format($"{route1.Vehicle.Id}-{Math.Round(route1.ArrivalTimes[0])}"),
                                    string.Format($"[Acceptable in historic] Routes of vehicle {route1.Vehicle.Id} drove by {driverId} and pulled by {tractorId} spanning from {Math.Round(route1.ArrivalTimes[0])} to {Math.Round(route1.DepartureTimes[^1])} and {Math.Round(route2.ArrivalTimes[0])} to {Math.Round(route2.DepartureTimes[^1])} overlap"),
                                    route1,
                                    ErrorCode.OverlappingRoutes,
                                    Math.Round(route2.ArrivalTimes[0])
                                    ));
                            }
                        }
                        else if (arrivalsCanOverlapDepartures)
                        {
                            if (route1.ArrivalTimes[^1] > route2.DepartureTimes[0] && route1.DepartureTimes[0] < route2.ArrivalTimes[^1])
                            {
                                int driverId = route1.VehicleDriver != null ? route1.VehicleDriver.Id : -1;
                                int tractorId = route1.VehicleTractor != null ? route1.VehicleTractor.Id : -1;
                                errors.Add(new ValidationError(
                                    VRPErrorType.InfeasibleSolution,
                                    VRPObjectType.Route,
                                    string.Format($"{route1.Vehicle.Id}-{route1.ArrivalTimes[0]}"),
                                    string.Format($"[Acceptable in historic] Routes of vehicle {route1.Vehicle.Id} drove by {driverId} and pulled by {tractorId} spanning from {Math.Round(route1.DepartureTimes[0])} to {Math.Round(route1.ArrivalTimes[^1])} and vehicle {route2.Vehicle.Id} from {Math.Round(route2.DepartureTimes[0])} to {Math.Round(route2.ArrivalTimes[^1])} overlap"),
                                    route1,
                                    ErrorCode.OverlappingRoutes,
                                    route2.DepartureTimes[0]
                                    ));
                            }
                        }
                    }
                }
            }
            return errors;
        }

        /// <summary>
        /// Checks the solution for feasibility and consistency
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="leftRequests"></param>
        /// <param name="requests"></param>
        /// 
        /// <returns></returns>
        public static List<ValidationError> ValidateSolutionData(List<IRoute> routes, List<TransportRequest> leftRequests, List<TransportRequest> requests)
        {
            List<ValidationError> errors = new List<ValidationError>();
            errors.AddRange(ValidateSolutionCompletness(routes, leftRequests, requests));
            foreach (var route in routes)
            {
                errors.AddRange(ValidateRouteDelayCalculation(route));
                errors.AddRange(ValidateRouteAssignmentCompletness(route));
                errors.AddRange(ValidateRouteCapacityConstraints(route));
                errors.AddRange(ValidateRouteLoadUnloadLocationCompliance(route));
            }
            foreach (var vehicleRoutes in routes.GroupBy(rt => rt.Vehicle.Id))
            {
                errors.AddRange(ValidateRoutesTimeOverlap(vehicleRoutes, false));
            }
            foreach (var tractorRoutes in routes.Where(rt => rt.VehicleTractor != null).GroupBy(rt => rt.VehicleTractor.Id))
            {
                errors.AddRange(ValidateRoutesTimeOverlap(tractorRoutes, true));
            }
            foreach (var driverRoutes in routes.Where(rt => rt.VehicleDriver != null).GroupBy(rt => rt.VehicleDriver.Id))
            {
                errors.AddRange(ValidateRoutesTimeOverlap(driverRoutes, true));
            }
            return errors;
        }

        private static IEnumerable<ValidationError> ValidateRouteDelayCalculation(IRoute route)
        {
            List<ValidationError> errors = new List<ValidationError>();
            double totalDelay = 0;
            double maxDelay = 0;
            for (int i = 0; i < route.VisitedLocations.Count; ++i)
            {
                List<double> timeWindowEnds = new List<double>();
                timeWindowEnds.Add(route.Vehicle.AvailabilityEnd);
                if (route.VehicleDriver != null)
                {
                    timeWindowEnds.Add(route.VehicleDriver.AvailabilityEnd);
                }
                foreach (var unloadedRequest in route.UnloadedRequests[i])
                {
                    timeWindowEnds.Add(unloadedRequest.DeliveryPreferedTimeWindowEnd);
                }
                foreach (var loadedRequest in route.LoadedRequests[i])
                {
                    timeWindowEnds.Add(loadedRequest.PickupPreferedTimeWindowEnd);
                }
                double delay = timeWindowEnds.Max(tw => Math.Max(route.ArrivalTimes[i] - tw, 0));
                maxDelay = Math.Max(delay, maxDelay);
                totalDelay += delay;
                if (Math.Abs(timeWindowEnds.Min() - route.TimeWindowEnd[i]) > 1e-8)
                {
                    errors.Add(
                        new ValidationError(VRPErrorType.ImprobableValue,
                        VRPObjectType.Route,
                                    string.Format($"{route.Vehicle.Id}-{Math.Round(route.ArrivalTimes[0])}"),
                    $"Route of vehicle {route.Vehicle.Id} spanning from {Math.Round(route.ArrivalTimes[0])} has improper time window at point {i}: should be {timeWindowEnds.Min()} but is {route.TimeWindowEnd[i]}",
                    route,
                    ErrorCode.ImpossibleDriveTime,
                    timeWindowEnds.Min()
                    ));
                }
            }
            if (Math.Abs(maxDelay - route.MaxDelay) > 1e-8)
            {
                errors.Add(
                    new ValidationError(VRPErrorType.ImprobableValue,
                    VRPObjectType.Route,
                                string.Format($"{route.Vehicle.Id}-{Math.Round(route.ArrivalTimes[0])}"),
                $"Route of vehicle {route.Vehicle.Id} spanning from {Math.Round(route.ArrivalTimes[0])} has improper max delay: should be {maxDelay} but is {route.MaxDelay}",
                route,
                ErrorCode.ImpossibleDriveTime,
                maxDelay
                ));
            }
            if (Math.Abs(totalDelay - route.TotalDelay) > 1e-8)
            {
                errors.Add(
                    new ValidationError(VRPErrorType.ImprobableValue,
                    VRPObjectType.Route,
                                string.Format($"{route.Vehicle.Id}-{Math.Round(route.ArrivalTimes[0])}"),
                $"Route of vehicle {route.Vehicle.Id} spanning from {Math.Round(route.ArrivalTimes[0])} has improper total delay: should be {totalDelay} but is {route.TotalDelay}",
                route,
                ErrorCode.ImpossibleDriveTime,
                totalDelay
                ));
            }
            return errors;
        }

        public static IEnumerable<ValidationError> ValidateVehicleProperties(Vehicle vehicle)
        {
            List<ValidationError> errors = new List<ValidationError>();
            if (vehicle.Type != CommonGIS.Enums.VehicleType.SemiTrailer &&
                vehicle.Type != CommonGIS.Enums.VehicleType.StraightTruck &&
                vehicle.Type != CommonGIS.Enums.VehicleType.Tractor)
            {
                errors.Add(new ValidationError(
                    VRPErrorType.ImprobableValue,
                    VRPObjectType.Vehicle,
                    vehicle.Id.ToString(),
                    $"Vehicle {vehicle.Id} is of unknown {vehicle.Type} type non-existent in CommonGIS.Enums.VehicleType",
                    vehicle,
                    ErrorCode.WrongType,
                    vehicle.Type
                    ));
            }
            if (vehicle.RoadProperties.EpCount > VehicleRoadRestrictionProperties.MaxEPCount)
            {
                errors.Add(new ValidationError(
                    VRPErrorType.ImprobableValue,
                    VRPObjectType.Vehicle,
                    vehicle.Id.ToString(),
                    $"Vehicle {vehicle.Id} has larger ep count ({vehicle.RoadProperties.EpCount}) then expected maximum of {VehicleRoadRestrictionProperties.MaxEPCount}",
                    vehicle,
                    ErrorCode.TooLargeVehicleSize,
                    VehicleRoadRestrictionProperties.MaxEPCount
                    ));
            }
            if (vehicle.RoadProperties.GrossVehicleWeight > VehicleRoadRestrictionProperties.MaxGrossVehicleWeight)
            {
                errors.Add(new ValidationError(
                    VRPErrorType.ImprobableValue,
                    VRPObjectType.Vehicle,
                    vehicle.Id.ToString(),
                    $"Vehicle {vehicle.Id} has larger gross vehicle weight ({vehicle.RoadProperties.GrossVehicleWeight}) then expected maximum of {VehicleRoadRestrictionProperties.MaxGrossVehicleWeight}",
                    vehicle,
                    ErrorCode.TooLargeVehicleSize,
                    VehicleRoadRestrictionProperties.MaxGrossVehicleWeight
                    ));
            }
            if (vehicle.RoadProperties.Height > VehicleRoadRestrictionProperties.MaxHeight)
            {
                errors.Add(new ValidationError(
                    VRPErrorType.ImprobableValue,
                    VRPObjectType.Vehicle,
                    vehicle.Id.ToString(),
                    $"Vehicle {vehicle.Id} has larger height ({vehicle.RoadProperties.Height}) then expected maximum of {VehicleRoadRestrictionProperties.MaxHeight}",
                    vehicle,
                    ErrorCode.TooLargeVehicleSize,
                    VehicleRoadRestrictionProperties.MaxHeight
                    ));
            }
            if (vehicle.RoadProperties.Width > VehicleRoadRestrictionProperties.MaxWidth)
            {
                errors.Add(new ValidationError(
                    VRPErrorType.ImprobableValue,
                    VRPObjectType.Vehicle,
                    vehicle.Id.ToString(),
                    $"Vehicle {vehicle.Id} has larger width ({vehicle.RoadProperties.Width}) then expected maximum of {VehicleRoadRestrictionProperties.MaxWidth}",
                    vehicle,
                    ErrorCode.TooLargeVehicleSize,
                    VehicleRoadRestrictionProperties.MaxWidth
                    ));
            }
            if (vehicle.Capacity.All(cap => cap <= 0) && vehicle.Type != CommonGIS.Enums.VehicleType.Tractor)
            {
                errors.Add(new ValidationError(
                    VRPErrorType.ImprobableValue,
                    VRPObjectType.Vehicle,
                    vehicle.Id.ToString(),
                    $"Vehicle {vehicle.Id} has zero capacity although it is {vehicle.Type}",
                    vehicle,
                    ErrorCode.WrongProperties,
                    1
                    ));
            }
            if (vehicle.Capacity.Any(cap => cap > 0) && vehicle.Type == CommonGIS.Enums.VehicleType.Tractor)
            {
                errors.Add(new ValidationError(
                    VRPErrorType.ImprobableValue,
                    VRPObjectType.Vehicle,
                    vehicle.Id.ToString(),
                    $"Vehicle {vehicle.Id} has non zero capacity although it is {vehicle.Type}",
                    vehicle,
                    ErrorCode.WrongProperties,
                    0
                    ));
            }
            return errors;
        }

        public static IEnumerable<ValidationError> ValidateRoutesDelaysAndDistance(List<Model.Transport> transports, List<TransportRequest> transportRequests, List<Vehicle> vehicles, List<Driver> drivers, IDistanceProvider distanceProvider)
        {
            List<ValidationError> errors = new List<ValidationError>();
            drivers = drivers ?? new List<Driver>();
            foreach (var transport in transports)
            {
                var vehicle = vehicles.FirstOrDefault(v => v.Id == transport.TrailerTruckId);
                var driver = drivers.FirstOrDefault(d => d.Id == transport.DriverId);
                var locationsDict = RouteDto.FillInLocationsDict(transportRequests);
                var distances = new List<CommonGIS.Distance>();
                for (int i = 0; i < transport.Schedule.Count; i++)
                {
                    if (i > 0)
                    {
                        CommonGIS.Distance dist = distanceProvider.GetDistance(
                                                    locationsDict[transport.Schedule[i - 1].LocationId],
                                                    locationsDict[transport.Schedule[i].LocationId],
                                                    vehicle.RoadProperties);
                        distances.Add(dist);
                        double travelTime = Math.Round(transport.Schedule[i].ArrivalTime - transport.Schedule[i - 1].DepartureTime);
                        if (travelTime < 0.8 * dist.Time)
                        {
                            errors.Add(
                                new ValidationError(VRPErrorType.ImprobableValue,
                                VRPObjectType.TransportDTO,
                                transport.TransportId.ToString(),
                                $"[Acceptable in historic] transport {transport.TransportId} travels from {transport.Schedule[i - 1].LocationId} to {transport.Schedule[i].LocationId} in {travelTime} instead of at least {dist.Time}",
                                transport,
                                ErrorCode.ImpossibleDriveTime,
                                dist.Time
                                ));
                        }
                        var delay = Math.Max(0, transport.Schedule[i].ArrivalTime - vehicle.AvailabilityEnd);
                        if (driver != null)
                        {
                            delay = Math.Max(0, transport.Schedule[i].ArrivalTime - driver.AvailabilityEnd);
                        }
                        foreach (var uId in transport.Schedule[i].UnloadedRequestsIds)
                        {
                            var request = transportRequests.First(r => r.Id == uId);
                            var twend = request.DeliveryPreferedTimeWindowEnd;
                            delay = Math.Max(delay, transport.Schedule[i].ArrivalTime - twend);
                        }
                        foreach (var lId in transport.Schedule[i].LoadedRequestsIds)
                        {
                            var request = transportRequests.First(r => r.Id == lId);
                            var twend = request.PickupPreferedTimeWindowEnd;
                            delay = Math.Max(delay, transport.Schedule[i].ArrivalTime - twend);
                        }
                        if (Math.Abs(transport.Schedule[i].Delay - delay) > 1e-8)
                        {
                            errors.Add(
                                new ValidationError(VRPErrorType.ImprobableValue,
                                VRPObjectType.TransportDTO,
                                transport.TransportId.ToString(),
                                $"Transport {transport.TransportId} is delayed {delay} in {transport.Schedule[i].LocationId} ({i} point in shedule) while delay reported in solution was {transport.Schedule[i].Delay}",
                                transport,
                                ErrorCode.ImpossibleDriveTime,
                                delay
                                ));

                        }
                    }
                }

                double distanceSum = distances.Sum(d => d.Length);
                if (Math.Abs(transport.Length - distanceSum) > 1e-8)
                {
                    errors.Add(
                        new ValidationError(VRPErrorType.ImprobableValue,
                        VRPObjectType.TransportDTO,
                        transport.TransportId.ToString(),
                        $"Transport {transport.TransportId} should travel distance of {distanceSum} while reported distance was {transport.Length}",
                        transport,
                        ErrorCode.ImpossibleDriveTime,
                        distanceSum
                        ));
                }
            }
            return errors;

        }
    }
}