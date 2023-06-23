using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using VRPTWOptimizer.Interfaces;

namespace VRPTWOptimizer
{
    /// <summary>
    /// Definition of structure describing solution to the Vehicle Routing Problem.
    /// Includes Vehicle assignment to TransportRequest and Vehicle schedule
    /// </summary>
    public class VRPSolution
    {
        /// <summary>
        /// Single time entry describing planned visit at a given Location
        /// </summary>
        public class ScheduleItem
        {
            /// <summary>
            /// Relative time in seconds when Vehicle arrives at Location
            /// </summary>
            public double ArrivalTime { get; set; }
            /// <summary>
            /// Delay in seconds againt the TimeWindowEnd if specified
            /// </summary>
            public double Delay { get; set; }
            /// <summary>
            /// Relative time in seconds when Vehicle leaves the Location
            /// </summary>
            public double DepartureTime { get; set; }
            /// <summary>
            /// Identifiers of TransportRequest objects that are loaded onto Vehicle at Location
            /// </summary>
            public List<int> LoadedRequestsIds { get; set; }
            /// <summary>
            /// Identifier of visited Location
            /// </summary>
            public string LocationId { get; set; }

            /// <summary>
            /// Identifiers of TransportRequest objects that are unloaded from Vehicle at Location
            /// </summary>
            public List<int> UnloadedRequestsIds { get; set; }
        }

        /// <summary>
        /// Entry describing a single loop of the Vehicle/combined Vehicle
        /// </summary>
        public class TransportItem
        {
            /// <summary>
            /// Relative time when the vehicle needs to be at the gate of warehouse to be loaded
            /// </summary>
            public double AvailableForLoadingTime { get; set; }
            /// <summary>
            /// Relative time when the vehicle is free for next assignements
            /// </summary>
            public double AvailableForNextAssignmentTime { get; set; }
            /// <summary>
            ///
            /// Identifier of Driver (if applicable)
            /// </summary>
            public int DriverId { get; set; }
            /// <summary>
            /// Percent of capacity filled when starting the Route
            /// </summary>
            public double FillInRatio { get; set; }
            /// <summary>
            /// Length of a routes in meters
            /// </summary>
            public double Length { get; set; }
            /// <summary>
            /// List of planned visits
            /// </summary>
            public List<ScheduleItem> Schedule { get; set; }
            /// <summary>
            /// Identifier of tractor unit (if applicable)
            /// </summary>
            public int TractorId { get; set; }
            /// <summary>
            /// Identifier of semitrailer or straight truck
            /// </summary>
            public int TrailerTruckId { get; set; }
            /// <summary>
            /// Route identifier
            /// </summary>
            public int TransportId { get; set; }
        }

        /// <summary>
        /// Name of the algorithm that generated this solution
        /// </summary>
        public string Algorithm { get; set; }
        /// <summary>
        /// Computations time it took to generate the solution (problem data is assumed to be loaded to memory)
        /// </summary>
        public double ComputationTime { get; set; }
        /// <summary>
        /// Timestamp when the computations were being performed
        /// </summary>
        public DateTime? ComputationTimestamp { get; set; }
        /// <summary>
        /// NETBIOS computer name that run the computations
        /// </summary>
        public string ComputerId { get; set; }
        /// <summary>
        /// Number of transport requests that were late (even 1 second against the prefered time)
        /// </summary>
        public int DelaysCount { get; set; }
        /// <summary>
        /// Ids of the TransportRequest objects that were not assigned to any Vehicle
        /// </summary>
        public List<int> LeftRequestsIds { get; set; }
        /// <summary>
        /// Max delay in serving an assigned TransportRequest (against the prefered time)
        /// </summary>
        public double MaxDelay { get; set; }
        /// <summary>
        /// Sum of all delay in serving TransportRequest objects (against the prefered time)
        /// </summary>
        public double TotalDelay { get; set; }
        /// <summary>
        /// Length of all routes in meters
        /// </summary>
        public double TotalLength { get; set; }
        /// <summary>
        /// List of all assigned transports
        /// </summary>
        public List<TransportItem> Transports { get; set; }
        /// <summary>
        /// Algorithm library version
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Creates VRPSolution in a standard format on the basis of results and computation properties
        /// </summary>
        /// <param name="_optimizer"></param>
        /// <param name="computationsStart"></param>
        /// <param name="computationsEnd"></param>
        /// <param name="leftRequests"></param>
        /// <param name="routes"></param>
        /// <returns></returns>
        public static VRPSolution GenerateVRPSolution(
            IVRPOptimizer _optimizer,
            DateTime computationsStart,
            DateTime computationsEnd,
            List<TransportRequest> leftRequests,
            List<IRoute> routes)
        {
            List<VRPSolution.TransportItem> transportItems = new();
            var orderedTrailerAssignment = routes
                .OrderBy(rt => rt.ArrivalTimes[0]);
            int transportId = 1;
            foreach (var assignment in orderedTrailerAssignment)
            {
                List<VRPSolution.ScheduleItem> scheduleItems = new();
                for (int i = 0; i < assignment.VisitedLocations.Count; i++)
                {
                    VRPSolution.ScheduleItem scheduleItem = new()
                    {
                        LocationId = assignment.VisitedLocations[i].Id,
                        ArrivalTime = assignment.ArrivalTimes[i],
                        DepartureTime = assignment.DepartureTimes[i],
                        Delay = Math.Max(assignment.ArrivalTimes[i] - assignment.TimeWindowEnd[i], 0),
                        LoadedRequestsIds = assignment.LoadedRequests[i].Select(rq => rq.Id).ToList(),
                        UnloadedRequestsIds = assignment.UnloadedRequests[i].Select(rq => rq.Id).ToList()
                    };
                    scheduleItems.Add(scheduleItem);
                }

                {
                    double totalDelay = 0;
                    double maxDelay = 0;
                    for (int i = 0; i < assignment.VisitedLocations.Count; ++i)
                    {
                        List<double> timeWindowEnds = new List<double>();
                        timeWindowEnds.Add(double.MaxValue);
                        timeWindowEnds.Add(assignment.Vehicle.AvailabilityEnd);
                        if (assignment.VehicleDriver != null)
                        {
                            timeWindowEnds.Add(assignment.VehicleDriver.AvailabilityEnd);
                        }
                        foreach (var unloadedRequest in assignment.UnloadedRequests[i])
                        {
                            timeWindowEnds.Add(unloadedRequest.DeliveryPreferedTimeWindowEnd);
                        }
                        foreach (var unloadedRequest in assignment.LoadedRequests[i])
                        {
                            timeWindowEnds.Add(unloadedRequest.PickupPreferedTimeWindowEnd);
                        }
                        double delay = timeWindowEnds.Max(tw => Math.Max(assignment.ArrivalTimes[i] - tw, 0));
                        maxDelay = Math.Max(delay, maxDelay);
                        totalDelay += delay;
                    }
                }

                double fillInRatio = assignment.Vehicle.Capacity
                    .Select((capacity, index) => index)
                    .Max(index => assignment.LoadedRequests[0].Sum(rq => rq.Size[index]) / assignment.Vehicle.Capacity[index])
                    ;
                VRPSolution.TransportItem transport = new()
                {
                    TransportId = transportId,
                    TractorId = assignment.VehicleTractor == null ? -1 : assignment.VehicleTractor.Id,
                    TrailerTruckId = assignment.Vehicle.Id,
                    DriverId = assignment.VehicleDriver == null ? -1 : assignment.VehicleDriver.Id,
                    Length = assignment.Length,
                    Schedule = scheduleItems,
                    AvailableForLoadingTime = assignment.ArrivalTimes[0],
                    AvailableForNextAssignmentTime = assignment.DepartureTimes[^1],
                    FillInRatio = Math.Round(fillInRatio, 2)
                };
                transportItems.Add(transport);
                transportId++;
            }

            VRPSolution vrpSolution = new()
            {
                LeftRequestsIds = leftRequests.Select(req => req.Id).ToList(),
                ComputationTimestamp = computationsEnd,
                ComputationTime = (computationsEnd - computationsStart).TotalSeconds,
                ComputerId = Environment.MachineName,
                Version = Assembly.GetAssembly(_optimizer.GetType()).GetName().Version,
                DelaysCount = routes.Count(ta => ta.TotalDelay > 0),
                MaxDelay = routes.Any() ? routes.Max(ta => ta.MaxDelay) : 0,
                TotalDelay = routes.Sum(ta => ta.TotalDelay),
                TotalLength = routes.Sum(ta => ta.Length),
                Transports = transportItems,
                Algorithm = _optimizer.GetType().FullName
            };
            return vrpSolution;
        }
    }
}