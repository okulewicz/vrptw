using CommonGIS;
using System;
using System.Collections.Generic;
using System.Linq;
using VRPTWOptimizer.Enums;
using VRPTWOptimizer.Interfaces;
using VRPTWOptimizer.Utils.Model;
using VRPTWOptimizer.Utils.TimeEstimators;
using static VRPTWOptimizer.Utils.Model.ViaTmsJSONDTOs;

namespace VRPTWOptimizer.Utils.VrpDefinition
{
    public class VRPDefinitionViaTmsDTOProvider : IVRPJSONProvider
    {
        private const int MaxDriverSecondsOver8Hours = 12600;

        public string Client { get; set; }
        public string DepotId { get; set; }
        public List<CommonGIS.Distance> Distances { get; set; }
        public List<Driver> Drivers { get; set; }
        public CommonGIS.Location HomeDepot { get; set; }
        public Dictionary<string, CommonGIS.Location> LocationsDictionary { get; set; }
        public DateTime ProblemDate { get; set; }
        public List<VRPTWOptimizer.TransportRequest> Requests { get; set; }

        public ITimeEstimator ServiceTimeEstimator { get; set; }
        public List<VRPTWOptimizer.Vehicle> Vehicles { get; set; }

        public DateTime ZeroHour { get; set; }

        public VRPDefinitionViaTmsDTOProvider(VRPDefinitionViaTmsDTO definition)
        {
            ProblemDate = definition.BillingDates[0];
            DepotId = definition.HomeDepots[0].Id;
            Client = definition.Client;
            if (definition.TimeEstimator != null)
            {
                ServiceTimeEstimator = definition.TimeEstimator;
            }
            else
            {
                ServiceTimeEstimator = new ExpertServiceTimeEstimator();
            }
            Distances = definition.Distances.Select(d =>
            new TimeLengthDistance(d.FromId, d.ToId, d.Length, d.Time,
                new VehicleRoadRestrictionProperties(
                    d.Profile.GrossVehicleWeight,
                    d.Profile.Height,
                    d.Profile.Width,
                    d.Profile.EpCount,
                    CommonGIS.Enums.VehicleTypeRouting.StraightTruck)) as CommonGIS.Distance).ToList();
            ZeroHour = definition.BillingDates[0];
            var homeDepot = new BaseLocation(definition.HomeDepots[0].Id, definition.HomeDepots[0].Lng, definition.HomeDepots[0].Lat);
            HomeDepot = homeDepot;
            Drivers = new List<Driver>();
            if (definition.Drivers != null)
            {
                Drivers.AddRange(
                    definition.Drivers.Select(d => new DriverDTO(
                        id: d.Id,
                        availabilityStart: (d.AvailabilityStart - ZeroHour).TotalSeconds,
                        availabilityEnd: (d.AvailabilityEnd - ZeroHour).TotalSeconds,
                        compatibileVehiclesIds: definition.Vehicles
                            .Where(v => v.OwnerType == VehicleOwnership.Internal)
                            .Select(v => v.Id)
                            .ToArray(),
                        new int[] { VehicleOwnership.Internal }
                        ))    
                    );
                /*
                var maxId = Drivers.Max(d => d.Id);
                foreach (var vehicle in definition.Vehicles.Where(v => v.OwnerType == VehicleOwnership.External))
                {
                    maxId++;
                    // create artificial DriverDTO for each vehicle
                    Drivers.Add(new DriverDTO(id: maxId,
                                              availabilityStart: (vehicle.AvailabilityStart - ZeroHour).TotalSeconds,
                                              availabilityEnd: (vehicle.AvailabilityEnd - ZeroHour).TotalSeconds,
                                              compatibileVehiclesIds: new int[] { vehicle.Id }));
                }
                */
            }

            double maxInternalVehicleAvaialbility = double.MinValue;
            if (Drivers.Any())
            {
                maxInternalVehicleAvaialbility = Drivers.Max(dr => dr.AvailabilityEnd + MaxDriverSecondsOver8Hours);
            }

            Vehicles = new List<VRPTWOptimizer.Vehicle>();
            Vehicles.AddRange(definition.Vehicles.Select(v => new VehicleDTO(
                id: v.Id,
                capacity: new double[] { v.EpCapacity, v.WeightCapacity },
                specialProperties: new int[0],
                capacityAggregationType: new Enums.Aggregation[] { Enums.Aggregation.Sum, Enums.Aggregation.Sum },
                initialLocation: homeDepot,
                availabilityStart: (v.AvailabilityStart - ZeroHour).TotalSeconds,
                finalLocation: homeDepot,
                availabilityEnd: (v.OwnerType == (int)VehicleOwnership.Internal) ? Math.Max((v.AvailabilityEnd - ZeroHour).TotalSeconds, maxInternalVehicleAvaialbility) : (v.AvailabilityEnd - ZeroHour).TotalSeconds,
                maxRideTime: double.MaxValue,
                roadProperties: new VehicleRoadRestrictionProperties(
                    grossVehicleWeight: v.GrossVehicleWeight,
                    height: 0,
                    width: 0,
                    //HACK: use old field value for older JSON formats
                    epCount: v.DrivewayEpSize ?? v.EpCapacity,
                    vehicleType: CommonGIS.Enums.VehicleTypeRouting.StraightTruck),
                type: CommonGIS.Enums.VehicleType.StraightTruck,
                vehicleCostPerDistanceUnit: v.CostPerDistance / 1000.0,
                vehicleCostPerTimeUnit: v.CostPerTime / 3600.0,
                vehicleCostPerUsage: v.ExistenceCost,
                ownerID: v.OwnerType,
                vehicleFlatCostForShortRouteLength: v.CostPerUsage,
                vehicleMaxRouteLengthForFlatCost: v.CostPerUsage > 0 ? 100000.0 : 0.0,
                vehicleCostPerRoute: 0.0
                )));

            LocationsDictionary = new Dictionary<string, CommonGIS.Location>();
            LocationsDictionary.Add(HomeDepot.Id, HomeDepot);
            Requests = new List<VRPTWOptimizer.TransportRequest>();
            foreach (var request in definition.TransportRequests)
            {
                var location = new BaseLocation(request.EndLocation.Id, request.EndLocation.Lng, request.EndLocation.Lat);
                if (!LocationsDictionary.ContainsKey(location.Id))
                {
                    LocationsDictionary.Add(location.Id, location);
                }
                var unwantedLocation = new string[]
                {
                    "Barbora sp z o.o. Gdańsk Logistics Centre",
                };
                //HACK removing Barbora and others
                if (!unwantedLocation.Contains(location.Id))
                {
                    double processedPickupAvailableTimeWindowEnd = request.PickupTimeWindowEnd != null ? (request.PickupTimeWindowEnd.Value - ZeroHour).TotalSeconds : double.MaxValue;
                    double processedDeliveryAvailableTimeWindowStart = request.DeliveryTimeWindowStart != null ? (request.DeliveryTimeWindowStart.Value - ZeroHour).TotalSeconds : double.MinValue;
                    double processedDeliveryAvailableTimeWindowEnd = request.DeliveryTimeWindowEnd != null ? Math.Max((request.DeliveryTimeWindowEnd.Value - ZeroHour).TotalSeconds, (request.TimeWindowEnd - ZeroHour).TotalSeconds) : Math.Max(72000, (request.TimeWindowEnd - ZeroHour).TotalSeconds);
                    VRPTWOptimizer.TransportRequest transportRequest = new RequestDTO(
                        id: request.Ids[0],
                        size: new double[] { request.Ep, request.Mass },
                        necessaryVehicleSpecialProperties: new int[0],
                        packageCount: (int)Math.Ceiling(request.Ep),
                        packageCountForImediateRetrieval: (int)request.EpMeat,
                        pickupLocation: homeDepot,
                        pickupAvailableTimeWindowStart: double.MinValue,
                        pickupPreferedTimeWindowStart: double.MinValue,
                        pickupPreferedTimeWindowEnd: double.MaxValue,
                        pickupAvailableTimeWindowEnd: processedPickupAvailableTimeWindowEnd,
                        deliveryLocation: location,
                        deliveryAvailableTimeWindowStart: processedDeliveryAvailableTimeWindowStart,
                        deliveryPreferedTimeWindowStart: (request.TimeWindowStart - ZeroHour).TotalSeconds,
                        deliveryPreferedTimeWindowEnd: (request.TimeWindowEnd - ZeroHour).TotalSeconds,
                        deliveryAvailableTimeWindowEnd: processedDeliveryAvailableTimeWindowEnd,
                        type: RequestType.GoodsDistribution,
                        cargoTypes: new int[] { 1 },
                        maxVehicleSize: new VehicleRoadRestrictionProperties(
                            grossVehicleWeight: VehicleRoadRestrictionProperties.MaxGrossVehicleWeight,
                            height: VehicleRoadRestrictionProperties.MaxHeight,
                            width: VehicleRoadRestrictionProperties.MaxWidth,
                            epCount: request.DrivewaySizeInEp,
                            vehicleType: CommonGIS.Enums.VehicleTypeRouting.TractorWithTrailer
                            ),
                        restrictedGoodsTypes: new int[0],
                        mutuallyExclusiveRequestsIdTimeBufferDict: new Dictionary<int, double>(),
                        revenueValue: 0.0,
                        name: string.Join(',', request.Ids.Select(id => id.ToString()))
                        );
                    Requests.Add(transportRequest);
                }
            }
        }

        public void LoadData(DateTime billingDate, string homeDepotId)
        {
            throw new NotImplementedException();
        }
    }
}