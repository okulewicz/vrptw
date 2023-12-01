using CommonGIS.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using VRPTWOptimizer.Interfaces;

namespace VRPTWOptimizer.Logging
{
    /// <summary>
    /// Serializes solution to JSON
    /// </summary>
    public class JSONDefinitionWriter : IVRPSolutionWriter

    {
        private readonly string _clientName;
        private readonly DateTime _computationsEnd;
        private readonly DateTime _computationsStart;
        private readonly IDistanceProvider _distanceData;
        private readonly string _filename;
        private readonly IVRPOptimizer _optimizer;
        private readonly List<TransportRequest> _requests;
        private readonly ITimeEstimator _timeEstimator;
        private readonly List<Vehicle> _vehicles;
        private readonly DateTime _zeroHour;

        [Obsolete("Serialization of result should be done with serializing VRPDefinition with VRPSolutions collection")]
        public JSONDefinitionWriter(List<TransportRequest> requests,
                                    List<Vehicle> vehicles,
                                    IVRPOptimizer optimizer,
                                    DateTime zeroHour,
                                    DateTime computationsEnd,
                                    DateTime computationsStart,
                                    IDistanceProvider distanceData,
                                    ITimeEstimator timeEstimator,
                                    string filename,
                                    string clientName)
        {
            _requests = requests;
            _vehicles = vehicles;
            _optimizer = optimizer;
            _zeroHour = zeroHour;
            _computationsEnd = computationsEnd;
            _computationsStart = computationsStart;
            _distanceData = distanceData;
            _timeEstimator = timeEstimator;
            _filename = filename;
            _clientName = clientName;
        }

        /// <summary>
        /// Saves solution to the format implemented by this Writer class (JSON in this case)
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="unassignedRequests"></param>
        /// <param name="billingDate"></param>
        /// <param name="homeDepotId"></param>
        /// <param name="algorithmName"></param>
        public void SaveSolution(List<IRoute> routes, List<TransportRequest> unassignedRequests, DateTime billingDate, string homeDepotId, string algorithmName)
        {
            var vrpDefinition = new VRPDefinition();
            vrpDefinition.Requests = _requests;
            vrpDefinition.ServiceTimeEstimator = _timeEstimator;
            vrpDefinition.Vehicles = _vehicles;
            var vrpSolution = new VRPSolution();
            vrpDefinition.Solutions = new List<VRPSolution>();
            vrpDefinition.Date = billingDate.Date;
            vrpDefinition.DepotId = homeDepotId;
            vrpDefinition.Client = _clientName;
            vrpDefinition.ZeroHour = _zeroHour;
            vrpDefinition.Solutions.Add(vrpSolution);
            vrpDefinition.DistanceData = _distanceData;

            vrpSolution.LeftRequestsIds = unassignedRequests.Select(req => req.Id).ToList();
            vrpSolution.ComputationTimestamp = _computationsEnd;
            vrpSolution.ComputationTime = (_computationsEnd - _computationsStart).TotalSeconds;
            vrpSolution.ComputerId = Environment.MachineName;
            vrpSolution.Version = Assembly.GetAssembly(_optimizer.GetType()).GetName().Version;
            var orderedTrailerAssignment = routes
                .OrderBy(rt => rt.ArrivalTimes[0]);
            vrpSolution.DelaysCount = routes.Count(rt => rt.TotalDelay > 0);
            vrpSolution.MaxDelay = routes.Max(rt => rt.MaxDelay);
            vrpSolution.TotalDelay = routes.Sum(rt => rt.TotalDelay);
            vrpSolution.TotalLength = routes.Sum(rt => rt.Length);
            vrpSolution.Transports = new List<VRPSolution.TransportItem>();
            vrpSolution.Algorithm = _optimizer.GetType().FullName;
            foreach (var assignment in orderedTrailerAssignment)
            {
                double fillInRatio = assignment.Vehicle.Capacity
                        .Select((capacity, index) => index)
                        .Max(index =>
                            {
                                if (assignment.Vehicle.CapacityAggregationType[index] == Enums.Aggregation.Sum)
                                {
                                    return assignment.LoadedRequests[0].Sum(rq => rq.Size[index]) / assignment.Vehicle.Capacity[index];
                                }
                                else
                                {
                                    if (assignment.LoadedRequests[0].Count > 0)
                                    {
                                        return assignment.LoadedRequests[0].Max(rq => rq.Size[index]) / assignment.Vehicle.Capacity[index];
                                    }
                                    return 0;
                                }
                            })
                        ;
                var transport = new VRPSolution.TransportItem();
                transport.TransportId = assignment.Id;
                transport.TractorId = -1;
                transport.TrailerTruckId = assignment.Vehicle.Id;
                transport.Length = assignment.Length;
                transport.Schedule = new List<VRPSolution.ScheduleItem>();
                transport.AvailableForLoadingTime = assignment.ArrivalTimes[0];
                transport.AvailableForNextAssignmentTime = assignment.DepartureTimes[assignment.DepartureTimes.Count - 1];
                transport.FillInRatio = Math.Round(fillInRatio, 2);
                for (int i = 0; i < assignment.VisitedLocations.Count; i++)
                {
                    var scheduleItem = new VRPSolution.ScheduleItem();
                    scheduleItem.LocationId = assignment.VisitedLocations[i].Id;
                    scheduleItem.ArrivalTime = assignment.ArrivalTimes[i];
                    scheduleItem.DepartureTime = assignment.DepartureTimes[i];
                    scheduleItem.Delay = Math.Max(assignment.ArrivalTimes[i] - assignment.TimeWindowEnd[i], 0);
                    scheduleItem.LoadedRequestsIds = assignment.LoadedRequests[i].Select(rq => rq.Id).ToList();
                    scheduleItem.UnloadedRequestsIds = assignment.UnloadedRequests[i].Select(rq => rq.Id).ToList();
                    transport.Schedule.Add(scheduleItem);
                }
                vrpSolution.Transports.Add(transport);
            }

            File.WriteAllText(_filename, vrpDefinition.ToPrettyJSONString());
        }
    }
}