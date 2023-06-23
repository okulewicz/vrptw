using CommonGIS;
using CommonGIS.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VRPTWOptimizer;
using VRPTWOptimizer.Interfaces;

namespace VRPTWOptimizer.Utils.Model
{
    public class RouteDto : IRoute
    {
        public List<double> ArrivalTimes { get; }

        public List<double> DepartureTimes { get; }

        public List<Distance> Distances { get; }

        public double Length => Distances.Sum(d => d.Length);

        public List<List<TransportRequest>> LoadedRequests { get; }

        public double MaxDelay
        {
            get
            {
                double delay = 0.0;
                for (int i = 0; i < ArrivalTimes.Count; i++)
                {
                    delay = Math.Max(ArrivalTimes[i] - TimeWindowEnd[i], delay);
                }
                return delay;
            }
        }

        public List<double> TimeWindowEnd { get; }

        public List<double> TimeWindowStart { get; }

        public double TotalDelay
        {
            get
            {
                double delay = 0.0;
                for (int i = 0; i < ArrivalTimes.Count; i++)
                {
                    delay += Math.Max(ArrivalTimes[i] - TimeWindowEnd[i], 0);
                }
                return delay;
            }
        }

        public double TravelTime => Distances.Sum(d => d.Time);

        public List<List<TransportRequest>> UnloadedRequests { get; }

        public Vehicle Vehicle { get; }

        public Driver VehicleDriver { get; }

        public Vehicle VehicleTractor { get; }

        public List<Location> VisitedLocations { get; }

        public RouteDto(List<double> arrivalTimes, List<double> departureTimes, List<Distance> distances, List<List<TransportRequest>> loadedRequests, List<List<TransportRequest>> unloadedRequests, List<double> timeWindowStart, List<double> timeWindowEnd, Vehicle vehicle, Driver vehicleDriver, Vehicle vehicleTractor, List<Location> visitedLocations)
        {
            ArrivalTimes = arrivalTimes;
            DepartureTimes = departureTimes;
            Distances = distances;
            LoadedRequests = loadedRequests;
            UnloadedRequests = unloadedRequests;
            TimeWindowStart = timeWindowStart;
            TimeWindowEnd = timeWindowEnd;
            Vehicle = vehicle;
            VehicleDriver = vehicleDriver;
            VehicleTractor = vehicleTractor;
            VisitedLocations = visitedLocations;
        }

        public RouteDto(Transport definitionDto, List<TransportRequest> requests, List<Vehicle> vehicles, List<Driver> drivers, IDistanceProvider distanceProvider)
        {
            Vehicle = vehicles.FirstOrDefault(v => v.Id == definitionDto.TrailerTruckId);
            if (definitionDto.TractorId != -1 && definitionDto.TractorId != 0)
            {
                VehicleTractor = vehicles.FirstOrDefault(v => v.Id == definitionDto.TractorId);
            }
            if (definitionDto.DriverId != -1 && definitionDto.DriverId != 0)
            {
                VehicleDriver = drivers.FirstOrDefault(d => d.Id == definitionDto.DriverId);
            }
            ArrivalTimes = definitionDto.Schedule.Select(s => s.ArrivalTime).ToList();
            DepartureTimes = definitionDto.Schedule.Select(s => s.DepartureTime).ToList();
            Dictionary<string, Location> locationsDict = FillInLocationsDict(requests);
            VisitedLocations = new();
            Distances = new();
            for (int i = 0; i < definitionDto.Schedule.Count; i++)
            {
                VisitedLocations.Add(locationsDict[definitionDto.Schedule[i].LocationId]);
                if (i > 0)
                {
                    Distance dist = distanceProvider.GetDistance(
                                                locationsDict[definitionDto.Schedule[i - 1].LocationId],
                                                locationsDict[definitionDto.Schedule[i].LocationId],
                                                Vehicle.RoadProperties);
                    Distances.Add(dist);
                }
            }
            LoadedRequests = definitionDto.Schedule.Select(s => s.LoadedRequestsIds.Select(id => requests.First(rq => rq.Id == id)).ToList()).ToList();
            UnloadedRequests = definitionDto.Schedule.Select(s => s.UnloadedRequestsIds.Select(id => requests.First(rq => rq.Id == id)).ToList()).ToList();
            TimeWindowStart = new();
            TimeWindowEnd = new();
            for (int i = 0; i < VisitedLocations.Count; i++)
            {
                TimeWindowStart.Add(LoadedRequests[i]
                    .Select(rq => rq.PickupPreferedTimeWindowStart)
                    .Concat(UnloadedRequests[i]
                    .Select(rq => rq.DeliveryPreferedTimeWindowStart))
                    .Concat(new List<double>() { Vehicle.AvailabilityStart })
                    .Concat(new List<double>() { VehicleDriver != null ? VehicleDriver.AvailabilityEnd : double.MinValue })
                    .Max());
                TimeWindowEnd.Add(LoadedRequests[i]
                    .Select(rq => rq.PickupPreferedTimeWindowEnd)
                    .Concat(UnloadedRequests[i]
                    .Select(rq => rq.DeliveryPreferedTimeWindowEnd))
                    .Concat(new List<double>() { Vehicle.AvailabilityEnd })
                    .Concat(new List<double>() { VehicleDriver != null ? VehicleDriver.AvailabilityEnd : double.MaxValue })
                    .Min());
            }
        }

        private static void AddLocationToDictIfNewLocation(Dictionary<string, Location> locationsDict, Location location)
        {
            if (!locationsDict.ContainsKey(location.Id))
            {
                locationsDict.Add(location.Id, location);
            }
        }

        public static Dictionary<string, Location> FillInLocationsDict(List<TransportRequest> requests)
        {
            var locationsDict = new Dictionary<string, Location>();
            foreach (var request in requests)
            {
                AddLocationToDictIfNewLocation(locationsDict, request.PickupLocation);
                AddLocationToDictIfNewLocation(locationsDict, request.DeliveryLocation);
            }

            return locationsDict;
        }
    }
}