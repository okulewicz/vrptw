using CommonGIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using VRPTWOptimizer.Enums;
using VRPTWOptimizer.Interfaces;
using VRPTWOptimizer.Utils.Provider;

namespace VRPTWOptimizer.Logging
{
    public class ValidationInfo
    {
        private const string FormatTimeSpan = "d\\.hh\\:mm";
        private const string FormatTimeSpanWithoutDay = "hh\\:mm";
        public int ErrorCode;
        public SeverityLevel Severity; 
        public string Message;

        static public ValidationInfo CreateNonFittingRequestInfo(TransportRequest transportRequest) {
            return new ValidationInfo()
            {
                ErrorCode = ((int)ValidationCode.RequestTooLarge),
                Message = $"Request {transportRequest.Id} with size {string.Join("; ", transportRequest.Size.Select(s => s.ToString()))} and vehicle size limit {transportRequest.MaxVehicleSize.EpCount} cannot be handled by any vehicle",
                Severity = SeverityLevel.Error
            };
        }

        static public ValidationInfo CreateStrangeVehicleDefinition(Vehicle vehicle)
        {
            return new ValidationInfo()
            {
                ErrorCode = ((int)ValidationCode.StrangeCapacity),
                Message = $"Vehicle {vehicle} has large ep size {vehicle.RoadProperties.EpCount}",
                Severity = SeverityLevel.Error
            };
        }

        static public ValidationInfo CreateStrangeLocationRequest(string locationId, Distance distance)
        {
            return new ValidationInfo()
            {
                ErrorCode = ((int)ValidationCode.FarAwayLocation),
                Message = $"Location {locationId} seems to be located far from depot ({Math.Round(distance.Length / 1000)}km)",
                Severity = SeverityLevel.Warning
            };
        }

        static public ValidationInfo CreateLowVehicleAvailability(Vehicle vehicle, double maxDriverAvailabilityEnd, double minDrvAvalStart, double estimatedLoadTimeAtDepot)
        {
            return new ValidationInfo()
            {
                ErrorCode = ((int)ValidationCode.LowVehicleAvailability),
                Message = $"Vehicle {vehicle.Id} seems to have too small availabilty from {FormatedTime(vehicle.AvailabilityStart)} to {FormatedTime(vehicle.AvailabilityEnd)}" +
                $" while loading can start as early as {FormatedTime(minDrvAvalStart - estimatedLoadTimeAtDepot)} as drivers are available from {FormatedTime(minDrvAvalStart)}" +
                $" to {FormatedTime(maxDriverAvailabilityEnd)}",
                Severity = SeverityLevel.Warning
            };
        }

        private static string FormatedTime(double timeInSeconds)
        {
            if (timeInSeconds >= 24 * 3600)
            {
                return TimeSpan.FromSeconds(timeInSeconds).ToString(FormatTimeSpan);
            }
            else
            {
                return TimeSpan.FromSeconds(timeInSeconds).ToString(FormatTimeSpanWithoutDay);
            }
        }

        public static List<ValidationInfo> Validate(List<TransportRequest> requests, List<Vehicle> vehicles, List<Driver> drivers, Location homeDepot, double maxDriverDelay, ITimeEstimator serviceTimeEstimator, out List<TransportRequest> markedRequests, out List<Vehicle> markedVehicles)
        {
            StraightLineDistanceProvider straightLineDistanceProvider = new StraightLineDistanceProvider();
            List<ValidationInfo> validationInfos = new List<ValidationInfo>();
            markedRequests = requests.Where(rq => !vehicles.Any(v => v.CanHandleRequest(rq))).ToList();
            foreach (var request in markedRequests)
            {
                validationInfos.Add(ValidationInfo.CreateNonFittingRequestInfo(request));
            }

            List<Distance> distances = new List<Distance>();
            foreach (var re in requests)
            {
                if (re.PickupLocation.Id != homeDepot.Id)
                {
                    Distance distance = straightLineDistanceProvider.GetDistance(re.PickupLocation, homeDepot, re.MaxVehicleSize);
                    distances.Add(distance);
                }
                else if (re.DeliveryLocation.Id != homeDepot.Id)
                {
                    Distance distance = straightLineDistanceProvider.GetDistance(re.DeliveryLocation, homeDepot, re.MaxVehicleSize);
                    distances.Add(distance);
                }
            }
            var meanDistance = distances.Average(d => d.Length);
            var stdVarDistance = distances.Average(d => Math.Abs(d.Length - meanDistance));
            foreach (var largeDistance in distances.Where(d => d.Length > meanDistance + 2.5 * stdVarDistance))
            {
                validationInfos.Add(CreateStrangeLocationRequest(largeDistance.FromId, largeDistance));
            }

            markedVehicles = vehicles.Where(v => v.RoadProperties.EpCount > VehicleRoadRestrictionProperties.MaxEPCount).ToList();
            foreach (var vehicle in markedVehicles)
            {
                validationInfos.Add(ValidationInfo.CreateStrangeVehicleDefinition(vehicle));
            }

            if (drivers != null && drivers.Any())
            {
                double maxDrvAvalEnd = drivers.Max(d => d.AvailabilityEnd + maxDriverDelay);
                double minDrvAvalStart = drivers.Min(d => d.AvailabilityStart);
                var lowAvailabilityVehicles = vehicles.Where(v =>
                {
                    var estimatedLoadTimeAtDepotBuffer = serviceTimeEstimator.EstimateLoadUnloadTime(0, v.RoadProperties.EpCount, 0, 1, 0, v.InitialLocation);
                    return v.OwnerID == VehicleOwnership.Internal && (v.AvailabilityEnd < maxDrvAvalEnd || v.AvailabilityStart > minDrvAvalStart - estimatedLoadTimeAtDepotBuffer);
                });
                foreach (var lowAvaVeh in lowAvailabilityVehicles)
                {
                    var estimatedLoadTimeAtDepotBuffer = serviceTimeEstimator.EstimateLoadUnloadTime(0, lowAvaVeh.RoadProperties.EpCount, 0, 1, 0, lowAvaVeh.InitialLocation);
                    validationInfos.Add(ValidationInfo.CreateLowVehicleAvailability(lowAvaVeh, maxDrvAvalEnd, minDrvAvalStart, estimatedLoadTimeAtDepotBuffer));
                }
                //markedVehicles.AddRange(lowAvailabilityVehicles);
            }

            return validationInfos;
        }
    }
}
