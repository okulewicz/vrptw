using CommonGIS;
using System;
using System.Collections.Generic;
using VRPTWOptimizer.Interfaces;
using VRPTWOptimizer.Utils.Model;

namespace VRPTWOptimizer.Utils.VrpDefinition
{
    public class VRPDefinitionJSONDTOProvider : IVRPJSONProvider
    {
        public string Client { get; set; }

        public string DepotId { get; set; }
        public List<Distance> Distances { get; set; }
        public List<Driver> Drivers { get; set; }
        public Location HomeDepot { get; set; }
        public List<int> PaczkiIloscMismatch { get; set; }
        public VIATMSSolutionDTO VIATMSSolution { get; set; }
        public Dictionary<string, Location> LocationsDictionary { get; set; }
        public DateTime ProblemDate { get; set; }
        public List<TransportRequest> Requests { get; set; }

        public ITimeEstimator ServiceTimeEstimator { get; set; }
        public List<VRPTWOptimizer.Vehicle> Vehicles { get; set; }

        public DateTime ZeroHour { get; set; }

        public VRPDefinitionJSONDTOProvider(VRPDefinitionJSONDTO dto)
        {
            ZeroHour = dto.ZeroHour;
            LocationsDictionary = new Dictionary<string, Location>();
            foreach (var request in dto.Requests)
            {
                var pickupLocation = new Location(request.PickupLocation.Id, request.PickupLocation.Lng, request.PickupLocation.Lat, request.PickupLocation.Type);
                if (!LocationsDictionary.ContainsKey(pickupLocation.Id))
                {
                    LocationsDictionary.Add(pickupLocation.Id, pickupLocation);
                }
                var deliveryLocation = new Location(request.DeliveryLocation.Id, request.DeliveryLocation.Lng, request.DeliveryLocation.Lat, request.DeliveryLocation.Type);
                if (!LocationsDictionary.ContainsKey(deliveryLocation.Id))
                {
                    LocationsDictionary.Add(deliveryLocation.Id, deliveryLocation);
                }
            }
            foreach (var vehicle in dto.Vehicles)
            {
                var initialLocation = new Location(vehicle.InitialLocation.Id, vehicle.InitialLocation.Lng, vehicle.InitialLocation.Lat, vehicle.InitialLocation.Type);
                if (!LocationsDictionary.ContainsKey(initialLocation.Id))
                {
                    LocationsDictionary.Add(initialLocation.Id, initialLocation);
                }
                var finalLocation = new Location(vehicle.FinalLocation.Id, vehicle.FinalLocation.Lng, vehicle.FinalLocation.Lat, vehicle.FinalLocation.Type);
                if (!LocationsDictionary.ContainsKey(finalLocation.Id))
                {
                    LocationsDictionary.Add(finalLocation.Id, finalLocation);
                }
            }
            Client = dto.Client;
            Distances = new List<Distance>();
            Distances.AddRange(dto.DistanceData.StoredDistances);
            HomeDepot = LocationsDictionary[dto.DepotId];
            DepotId = dto.DepotId;
            ProblemDate = dto.Date;
            Requests = new List<TransportRequest>();
            Requests.AddRange(dto.Requests);
            Vehicles = new List<VRPTWOptimizer.Vehicle>();
            Vehicles.AddRange(dto.Vehicles);
            if (dto.Drivers != null)
            {
                Drivers = new List<Driver>();
                Drivers.AddRange(dto.Drivers);
            }
            ServiceTimeEstimator = dto.ServiceTimeEstimator;

            //this data are optional
            PaczkiIloscMismatch = dto.PaczkiIloscMismatch;
            VIATMSSolution = dto.VIATMSSolution;
        }

        public void LoadData(DateTime billingDate, string homeDepotId)
        {
            throw new NotImplementedException();
        }
    }
}