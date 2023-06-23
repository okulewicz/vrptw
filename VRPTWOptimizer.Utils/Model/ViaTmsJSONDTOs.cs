using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace VRPTWOptimizer.Utils.Model
{
    public partial class ViaTmsJSONDTOs
    {
        public class Distance
        {
            [JsonProperty("from_id")]
            public string FromId { get; set; }

            [JsonProperty("length")]
            public double Length { get; set; }

            [JsonProperty("profile")]
            public Profile Profile { get; set; }

            [JsonProperty("time")]
            public double Time { get; set; }

            [JsonProperty("to_id")]
            public string ToId { get; set; }
        }

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class HomeDepot
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("lat")]
            public double Lat { get; set; }

            [JsonProperty("lng")]
            public double Lng { get; set; }
        }

        public class Location
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("lat")]
            public double Lat { get; set; }

            [JsonProperty("lng")]
            public double Lng { get; set; }
        }

        public class Profile
        {
            [JsonProperty("ep_count")]
            public int EpCount { get; set; }

            [JsonProperty("gross_vehicle_weight")]
            public int GrossVehicleWeight { get; set; }

            [JsonProperty("height")]
            public int Height { get; set; }

            [JsonProperty("vehicle_type")]
            public int VehicleType { get; set; }

            [JsonProperty("width")]
            public int Width { get; set; }
        }

        public class Route
        {
            [JsonProperty("day_start_date")]
            public DateTime DayStartDate { get; set; }

            [JsonProperty("transports")]
            public List<Transport> Transports { get; set; }
        }

        public class Transport
        {
            [JsonProperty("vehicle_id")]
            public int VehicleId { get; set; }

            [JsonProperty("visited_locations")]
            public List<VisitedLocation> VisitedLocations { get; set; }
        }

        public class TransportRequest
        {
            [JsonProperty("driveway_size_in_ep")]
            public int DrivewaySizeInEp { get; set; }
            [JsonProperty("end_location")]
            public Location EndLocation { get; set; }
            [JsonProperty("ep")]
            public double Ep { get; set; }
            [JsonProperty("ep_meat")]
            public double EpMeat { get; set; }
            [JsonProperty("id")]
            public int Id { get; set; }
            [JsonProperty("id_home_depot")]
            public string IdHomeDepot { get; set; }
            [JsonProperty("ids")]
            public int[] Ids { get; set; }
            [JsonProperty("mass")]
            public double Mass { get; set; }
            [JsonProperty("time_window_end")]
            public DateTime TimeWindowEnd { get; set; }
            [JsonProperty("time_window_start")]
            public DateTime TimeWindowStart { get; set; }

            [JsonProperty("delivery_time_window_start")]
            public DateTime? DeliveryTimeWindowStart { get; set; }
            [JsonProperty("pickup_time_window_end")]
            public DateTime? PickupTimeWindowEnd { get; set; }
        }

        public class Driver
        {
            [JsonProperty("availability_end")]
            public DateTime AvailabilityEnd { get; set; }
            [JsonProperty("availability_start")]
            public DateTime AvailabilityStart { get; set; }
            [JsonProperty("compatibile_vehicles_ids")]
            public int[] CompatibileVehiclesIds { get; set; }
            [JsonProperty("id")]
            public int Id { get; set; }
        }

        public class UnassignedRequest
        {
            [JsonProperty("arrival_time")]
            public DateTime ArrivalTime { get; set; }
            [JsonProperty("day_start_date")]
            public DateTime DayStartDate { get; set; }

            [JsonProperty("home_depot_id")]
            public string HomeDepotId { get; set; }

            [JsonProperty("lat")]
            public double Lat { get; set; }
            [JsonProperty("lng")]
            public double Lng { get; set; }
            [JsonProperty("poi_id")]
            public string PoiId { get; set; }
        }

        public class Vehicle
        {
            [JsonProperty("availability_end")]
            public DateTime AvailabilityEnd { get; set; }
            [JsonProperty("availability_start")]
            public DateTime AvailabilityStart { get; set; }
            [JsonProperty("cost_per_distance")]
            public double CostPerDistance { get; set; }
            [JsonProperty("cost_per_time")]
            public double CostPerTime { get; set; }
            [JsonProperty("cost_per_usage")]
            public double CostPerUsage { get; set; }
            [JsonProperty("ep_capacity")]
            public int EpCapacity { get; set; }
            [JsonProperty("gross_vehicle_weight")]
            public int GrossVehicleWeight { get; set; }
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("id_home_depot")]
            public string IdHomeDepot { get; set; }

            [JsonProperty("owner_type")]
            public int OwnerType { get; internal set; }
            [JsonProperty("weight_capacity")]
            public int WeightCapacity { get; set; }
        }

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class VisitedLocation
        {
            [JsonProperty("arrival_time")]
            public DateTime ArrivalTime { get; set; }
            [JsonProperty("lat")]
            public double Lat { get; set; }
            [JsonProperty("lng")]
            public double Lng { get; set; }
            [JsonProperty("poi_id")]
            public string PoiId { get; set; }
        }

        public class VRPDefinitionViaTmsDTO
        {
            [JsonProperty("billing_dates")]
            public List<DateTime> BillingDates { get; set; }
            public string Client => "Stokrotka API JSON";
            public string DepotId => HomeDepots[0].Id;
            [JsonProperty("distances")]
            public List<Distance> Distances { get; set; }
            [JsonProperty("home_depots")]
            public List<HomeDepot> HomeDepots { get; set; }
            [JsonProperty("parameters")]
            public TimeEstimatorDTO TimeEstimator { get; set; }
            [JsonProperty("transport_requests")]
            public List<TransportRequest> TransportRequests { get; set; }
            [JsonProperty("vehicles")]
            public List<Vehicle> Vehicles { get; set; }
            [JsonProperty("drivers")]
            public List<Vehicle> Drivers { get; set; }
        }

        public class VRPSolutionDTO
        {
            [JsonProperty("algorithm_name")]
            public string AlgorithmName { get; set; }

            [JsonProperty("billing_date")]
            public DateTime BillingDate { get; set; }
            [JsonProperty("computations_end")]
            public DateTime ComputationsEnd { get; set; }
            [JsonProperty("computations_start")]
            public DateTime ComputationsStart { get; set; }
            [JsonProperty("home_depot_id")]
            public string HomeDepotId { get; set; }

            [JsonProperty("routes")]
            public List<Route> Routes { get; set; }
            [JsonProperty("unassigned_requests")]
            public List<UnassignedRequest> UnassignedRequests { get; set; }
        }
    }
}