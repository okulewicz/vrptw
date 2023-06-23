using CommonGIS;
using System;
using System.Collections.Generic;
using VRPTWOptimizer.Interfaces;
using VRPTWOptimizer.Utils.Model;

namespace VRPTWOptimizer.Utils.VrpDefinition
{
    public class VRPDefinitionJSONDTOProvider : IVRPJSONProvider
    {
        public string Client { get; private set; }

        public string DepotId { get; private set; }
        public List<Distance> Distances { get; private set; }
        public List<Driver> Drivers { get; private set; }
        public Location HomeDepot { get; private set; }
        public List<int> PaczkiIloscMismatch { get; set; }
        public VIATMSSolutionDTO VIATMSSolution { get; set; }
        public Dictionary<string, Location> LocationsDictionary { get; }
        public DateTime ProblemDate { get; private set; }
        public List<TransportRequest> Requests { get; private set; }

        public ITimeEstimator ServiceTimeEstimator { get; private set; }
        public List<VRPTWOptimizer.Vehicle> Vehicles { get; private set; }

        public DateTime ZeroHour { get; private set; }

        public VRPDefinitionJSONDTOProvider(VRPDefinitionJSONDTO dto)
        {
            ZeroHour = dto.ZeroHour;
            LocationsDictionary = new Dictionary<string, Location>();
            foreach (var request in dto.Requests)
            {
                var pickupLocation = new BaseLocation(request.PickupLocation.Id, request.PickupLocation.Lng, request.PickupLocation.Lat);
                if (!LocationsDictionary.ContainsKey(pickupLocation.Id))
                {
                    LocationsDictionary.Add(pickupLocation.Id, pickupLocation);
                }
                var deliveryLocation = new BaseLocation(request.DeliveryLocation.Id, request.DeliveryLocation.Lng, request.DeliveryLocation.Lat);
                if (!LocationsDictionary.ContainsKey(deliveryLocation.Id))
                {
                    LocationsDictionary.Add(deliveryLocation.Id, deliveryLocation);
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