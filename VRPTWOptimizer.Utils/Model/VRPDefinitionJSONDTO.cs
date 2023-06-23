using CommonGIS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VRPTWOptimizer.Utils.Model

{
    public class DistanceData
    {
        public List<TimeLengthDistance> StoredDistances { get; set; }
    }

    public class Schedule
    {
        public double ArrivalTime { get; set; }
        public double Delay { get; set; }
        public double DepartureTime { get; set; }
        public List<int> LoadedRequestsIds { get; set; }
        public string LocationId { get; set; }
        public List<int> UnloadedRequestsIds { get; set; }
    }

    public class ServiceTimeEstimator
    {
        public double StopTime { get; set; }
        public double TimePerDeliveredPiece { get; set; }
        public double TimePerImmediateReturnPiece { get; set; }
        public double TimePerPickedUpPiece { get; set; }
    }

    public class Solution
    {
        public string Algorithm { get; set; }
        public double ComputationTime { get; set; }
        public DateTime? ComputationTimestamp { get; set; }
        public string ComputerId { get; set; }
        public int DelaysCount { get; set; }
        public List<int> LeftRequestsIds { get; set; }
        public double MaxDelay { get; set; }
        public double TotalDelay { get; set; }
        public double TotalLength { get; set; }
        public List<Transport> Transports { get; set; }
    }

    public class Transport
    {
        public double AvailableForLoadingTime { get; set; }
        public double AvailableForNextAssignmentTime { get; set; }
        public double FillInRatio { get; set; }
        public double Length { get; set; }
        public List<Schedule> Schedule { get; set; }
        public int TractorId { get; set; }
        public int TrailerTruckId { get; set; }
        public int TransportId { get; set; }
        public int DriverId { get; set; }
    }

    public class VRPDefinitionJSONDTO
    {
        public string Client { get; set; }
        public VRPCostFunction CostFunctionFactors { get; set; }
        public string DataFormatVersion { get; set; }
        public DateTime Date { get; set; }
        public List<int> PaczkiIloscMismatch { get; set; }
        public VIATMSSolutionDTO VIATMSSolution { get; set; }
        public string DepotId { get; set; }
        public DistanceData DistanceData { get; set; }
        [JsonIgnore]
        public List<Distance> Distances => DistanceData.StoredDistances.Select(d => d as Distance).ToList();

        public List<RequestDTO> Requests { get; set; }
        public ViaTmsJSONDTOs.TimeEstimatorDTO ServiceTimeEstimator { get; set; }
        public List<Solution> Solutions { get; set; }
        public List<VehicleDTO> Vehicles { get; set; }
        public List<DriverDTO> Drivers { get; set; }
        public DateTime ZeroHour { get; set; }
    }
}