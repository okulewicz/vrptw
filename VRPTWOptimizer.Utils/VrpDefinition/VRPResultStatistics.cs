using CommonGIS;
using CommonGIS.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRPTWOptimizer.Interfaces;
using VRPTWOptimizer.Utils.Model;
using static VRPTWOptimizer.Utils.Model.ViaTmsJSONDTOs;

namespace VRPTWOptimizer.Utils.VrpDefinition
{
    /// <summary>
    /// Statistcs of VRPResult object
    /// </summary>
    public class VRPResultStatistics
    {
        public double Cost { get; }
        public double FunctionCost { get; }
        public double FillRatio { get; }
        public int LeftEP { get; }
        public object LeftRequests { get; }
        public double Length { get; }
        public string MaxDelayStr { get; }
        public string MaxServiceDelayStr { get; }
        public TimeSpan MaxDelayTime { get; }
        public TimeSpan MaxServiceDelayTime { get; }
        public TimeSpan MaxEarlyArrival { get; }
        public string MaxEarlyStr { get; }
        public TimeSpan MaxLateStart { get; }
        public string MaxLateStartStr { get; }
        public TimeSpan MaxSpreadTime { get; }
        public string MaxSpreadTimeStr { get; }
        public int Routes { get; }
        public TimeSpan TotalDelay { get; }
        public string TotalDelayStr { get; }
        public TimeSpan TotalEarlyArrival { get; }
        public string TotalEarlyStr { get; }
        public TimeSpan TotalTravelTime { get; }
        public string TravelTimeStr { get; }
        public int UniqueTractorCount { get; }
        public int UniqueTrailersCount { get; }
        public int UniqueTrucksCount { get; }
        public int Visits { get; }
        public int CountFillInHalf { get; set; }
        public double LengthFillInHalf { get; set; }
        public double EPSizeSumAtInitialLoad { get; set; }
        public int PackgeCountAtInitialLoad { get; set; }
        public object UniqueDriversCount { get; private set; }
        public int NecessaryDriversCount { get; private set; }

        /// <summary>
        /// Creates object computing statistics over provided VRPResult object
        /// </summary>
        /// <param name="newResult"></param>
        public VRPResultStatistics(VRPOptimizerResult newResult, VRPCostFunction vrpCostFunction)
        {
            var newRoutes = newResult.Routes;
            LeftRequests = newResult.LeftRequests.Count;
            TotalTravelTime = TimeSpan.FromSeconds(newRoutes.Sum(rt => rt.Distances.Sum(dt => dt.Time)));
            MaxDelayTime = TimeSpan.FromSeconds(newRoutes.Any() ? newRoutes.Max(rt => rt.MaxDelay) : 0);
            MaxServiceDelayTime = TimeSpan.FromSeconds(newRoutes.Any() ? newRoutes.Max(rt => VRPCostFunction.ComputeMaxServiceDelay(rt)) : 0);
            MaxEarlyArrival = TimeSpan.FromSeconds(newRoutes.Any() ? newRoutes.Max(rt => VRPCostFunction.ComputeMaxEarlyArrival(rt)) : 0);
            MaxLateStart = TimeSpan.FromSeconds(VRPCostFunction.ComputeMaxLateVehicleStart(newRoutes));
            MaxSpreadTime = TimeSpan.FromSeconds(VRPCostFunction.ComputeMaxVehicleSpread(newRoutes));
            TotalDelay = TimeSpan.FromSeconds(newRoutes.Sum(rt => rt.TotalDelay));
            TotalEarlyArrival = TimeSpan.FromSeconds(newRoutes.Sum(rt => VRPCostFunction.ComputeTotalEarlyArrival(rt)));
            Routes = newRoutes.Count();
            Length = Math.Round(newRoutes.Sum(rt => rt.Length) / 1000);
            Cost = 0.0;
            Cost += newRoutes.Sum(rt => (rt.Length < rt.Vehicle.VehicleMaxRouteLengthForFlatCost ? rt.Vehicle.VehicleFlatCostForShortRouteLength : rt.Vehicle.VehicleCostPerDistanceUnit * rt.Length));
            Cost += newRoutes.Sum(rt => (rt.TravelTime * rt.Vehicle.VehicleCostPerTimeUnit));
            Cost += newRoutes.Sum(rt => (rt.Vehicle.VehicleCostPerRoute));
            Cost += newRoutes.Where(rt => rt.VehicleTractor != null).Sum(rt => (rt.VehicleTractor.VehicleCostPerRoute));
            Cost += newRoutes.Where(rt => rt.VehicleTractor != null).Sum(rt => (rt.Length < rt.VehicleTractor.VehicleMaxRouteLengthForFlatCost ? rt.VehicleTractor.VehicleFlatCostForShortRouteLength : rt.VehicleTractor.VehicleCostPerDistanceUnit * rt.Length));
            Cost += newRoutes.Where(rt => rt.VehicleTractor != null).Sum(rt => (rt.TravelTime * rt.VehicleTractor.VehicleCostPerTimeUnit));
            Cost += newRoutes.Select(rt => rt.Vehicle).Distinct().Sum(v => v.VehicleCostPerUsage);
            Cost += newRoutes.Where(rt => rt.VehicleTractor != null).Select(rt => rt.VehicleTractor).Distinct().Sum(v => v.VehicleCostPerUsage);
            Cost = Math.Round(Cost);
            FunctionCost = Math.Round(vrpCostFunction.Value(newResult.Routes, newResult.LeftRequests), 2);
            TravelTimeStr = $"{TotalTravelTime.Days * 24 + TotalTravelTime.Hours}:{TotalTravelTime.Minutes:00}";
            var fillInRatios = newRoutes.Select(rt => VRPCostFunction.ComputeFillInFactor(rt));
            FillRatio = Math.Round(fillInRatios.Any() ? fillInRatios.Average() : 0, 2);
            CountFillInHalf = fillInRatios.Count(ratio => ratio < 0.5);
            LengthFillInHalf = Math.Round(newRoutes.Where(rt => VRPCostFunction.ComputeFillInFactor(rt) < 0.5).Sum(rt => rt.Length / 1000));
            PackgeCountAtInitialLoad = newRoutes.Any() ? (int)newRoutes.Average(rt => rt.LoadedRequests[0].Sum(rq => rq.PackageCount)) : 0;
            EPSizeSumAtInitialLoad = newRoutes.Any() ? Math.Round(newRoutes.Average(rt => rt.LoadedRequests[0].Sum(rq => rq.Size[0])),2) : 0.0;
            MaxDelayStr = $"{MaxDelayTime.Days * 24 + MaxDelayTime.Hours}:{MaxDelayTime.Minutes:00}";
            MaxServiceDelayStr = $"{MaxServiceDelayTime.Days * 24 + MaxServiceDelayTime.Hours}:{MaxServiceDelayTime.Minutes:00}";
            TotalDelayStr = $"{TotalDelay.Days * 24 + TotalDelay.Hours}:{TotalDelay.Minutes:00}";
            MaxEarlyStr = $"{MaxEarlyArrival.Days * 24 + MaxEarlyArrival.Hours}:{MaxEarlyArrival.Minutes:00}";
            MaxLateStartStr = $"{MaxLateStart.Days * 24 + MaxLateStart.Hours}:{MaxLateStart.Minutes:00}";
            MaxSpreadTimeStr = $"{MaxSpreadTime.Days * 24 + MaxSpreadTime.Hours}:{MaxSpreadTime.Minutes:00}";
            TotalEarlyStr = $"{TotalEarlyArrival.Days * 24 + TotalEarlyArrival.Hours}:{TotalEarlyArrival.Minutes:00}";
            Visits = newRoutes.Sum(rt => rt.VisitedLocations.Count - 2);
            LeftEP = newResult.LeftRequests.Sum(rq => rq.PackageCount);
            UniqueTrailersCount = newResult.Routes.Where(rt => rt.Vehicle.Type == CommonGIS.Enums.VehicleType.SemiTrailer).Select(rt => rt.Vehicle.Id).Distinct().Count();
            UniqueTrucksCount = newResult.Routes.Where(rt => rt.Vehicle.Type == CommonGIS.Enums.VehicleType.StraightTruck).Select(rt => rt.Vehicle.Id).Distinct().Count();
            UniqueTractorCount = newResult.Routes.Where(rt => rt.VehicleTractor != null).Select(rt => rt.VehicleTractor.Id).Distinct().Count();
            UniqueDriversCount = newResult.Routes.Where(rt => rt.VehicleDriver != null).Select(rt => rt.VehicleDriver.Id).Distinct().Count();
            NecessaryDriversCount = VRPCostFunction.ComputeDriversCount(newResult.Routes);
        }

        /// <summary>
        /// Creates object computing statistics over provided VRPResult object
        /// </summary>
        /// <param name="newResult"></param>
        public VRPResultStatistics(VRPDefinitionJSONDTO dtoWithSolution, IDistanceProvider distanceProvider)
        {
            if (dtoWithSolution.Solutions?.Count > 0)
            {
                List<TransportRequest> transportRequests = dtoWithSolution.Requests.Select(r => r as TransportRequest).ToList();
                List<Vehicle> vehicles = dtoWithSolution.Vehicles.Select(v => v as Vehicle).ToList();
                List<Driver> drivers = (dtoWithSolution.Drivers ?? new List<DriverDTO>()).Select(d => d as Driver).ToList();
                List<IRoute> routes = dtoWithSolution.Solutions[0].Transports.Select(t => new RouteDto(t, transportRequests, vehicles, drivers, distanceProvider) as IRoute).ToList();
                List<TransportRequest> leftRequests = dtoWithSolution.Solutions[0].LeftRequestsIds.Select(id => transportRequests.FirstOrDefault(tr => tr.Id == id)).ToList();
                if (dtoWithSolution.CostFunctionFactors != null)
                {
                    if (dtoWithSolution.CostFunctionFactors.CarrierMinDistanceThreshold == null)
                    {
                        dtoWithSolution.CostFunctionFactors.CarrierMinDistanceThreshold = new Dictionary<int, double>();
                    }
                    if (dtoWithSolution.CostFunctionFactors.CarrierShareRatio == null)
                    {
                        dtoWithSolution.CostFunctionFactors.CarrierShareRatio = new Dictionary<int, double>();
                    }
                    FunctionCost = Math.Round(dtoWithSolution.CostFunctionFactors.Value(routes, leftRequests),2);
                }
                var newRoutes = dtoWithSolution.Solutions[0].Transports.OrderBy(t => t.Schedule[0].ArrivalTime);
                LeftRequests = dtoWithSolution.Solutions[0].LeftRequestsIds.Count;
                var requestsDict = new Dictionary<int, RequestDTO>();
                var vehiclesDict = new Dictionary<int, VehicleDTO>();
                var driversDict = new Dictionary<int, DriverDTO>();
                var locationsDict = new Dictionary<string, CommonGIS.Location>();
                foreach (var request in dtoWithSolution.Requests)
                {
                    requestsDict.Add(request.Id, request);
                    if (!locationsDict.ContainsKey(request.PickupLocation.Id))
                    {
                        locationsDict.Add(request.PickupLocation.Id, request.PickupLocation);
                    }
                    if (!locationsDict.ContainsKey(request.DeliveryLocation.Id))
                    {
                        locationsDict.Add(request.DeliveryLocation.Id, request.DeliveryLocation);
                    }
                }
                foreach (var vehicle in dtoWithSolution.Vehicles)
                {
                    if (!vehiclesDict.ContainsKey(vehicle.Id))
                    {
                        vehiclesDict.Add(vehicle.Id, vehicle);
                    }
                }
                if (dtoWithSolution.Drivers != null)
                {
                    foreach (var driver in dtoWithSolution.Drivers)
                    {
                        if (!driversDict.ContainsKey(driver.Id))
                        {
                            driversDict.Add(driver.Id, driver);
                        }
                    }
                }
                TotalTravelTime = new TimeSpan(0);
                Length = 0.0;
                MaxDelayTime = new TimeSpan(0);
                MaxServiceDelayTime = new TimeSpan(0);
                MaxEarlyArrival = new TimeSpan(0);
                TotalDelay = new TimeSpan(0);
                TotalEarlyArrival = new TimeSpan(0);

                Cost = 0.0;
                FillRatio = 0.0;
                CountFillInHalf = 0;
                LengthFillInHalf = 0.0;

                List<double> fillRatios = new List<double>();
                List<double> epSums = new List<double>();
                List<int> packageCounts = new List<int>();
                foreach (var route in newRoutes)
                {
                    Vehicle vehicle = vehiclesDict[route.TrailerTruckId];
                    Driver driver = null;
                    if (route.DriverId > 0)
                    {
                        driver = driversDict[route.DriverId];
                    }
                    double routeLength = 0.0;
                    double routeTime = 0.0;
                    double fillRatio = 0.0;
                    int initPackageCount = route.Schedule[0].LoadedRequestsIds.Sum(id => requestsDict[id].PackageCount);
                    packageCounts.Add(initPackageCount);
                    double initEPSum = route.Schedule[0].LoadedRequestsIds.Sum(id => requestsDict[id].Size[0]);
                    epSums.Add(initEPSum);
                    for (int c = 0; c < vehicle.Capacity.Length; c++)
                    {
                        if (vehicle.CapacityAggregationType[c] == Enums.Aggregation.Sum)
                        {
                            fillRatio = Math.Max(fillRatio, route.Schedule[0].LoadedRequestsIds.Sum(id => requestsDict[id].Size[c]) / vehicle.Capacity[c]);
                        }
                        else if (vehicle.CapacityAggregationType[c] == Enums.Aggregation.Max)
                        {
                            fillRatio = Math.Max(fillRatio, route.Schedule[0].LoadedRequestsIds.Max(id => requestsDict[id].Size[c]) / vehicle.Capacity[c]);
                        }
                    }
                    fillRatios.Add(fillRatio);
                    for (int i = 0; i < route.Schedule.Count; i++)
                    {
                        if (i < route.Schedule.Count - 1)
                        {
                            CommonGIS.Distance distance = distanceProvider.GetDistance(
                                                        locationsDict[route.Schedule[i].LocationId],
                                                        locationsDict[route.Schedule[i + 1].LocationId],
                                                        vehiclesDict[route.TrailerTruckId].RoadProperties);
                            TotalTravelTime += TimeSpan.FromSeconds(distance.Time);
                            Length += distance.Length / 1000;
                            routeLength += distance.Length;
                            routeTime += distance.Time;
                        }
                        if (vehicle != null)
                        {
                            MaxDelayTime = TimeSpan.FromSeconds(Math.Max(
                                MaxDelayTime.TotalSeconds,
                                route.Schedule[i].ArrivalTime - vehicle.AvailabilityEnd));
                        }
                        if (driver != null)
                        {
                            MaxDelayTime = TimeSpan.FromSeconds(Math.Max(
                                MaxDelayTime.TotalSeconds,
                                route.Schedule[i].ArrivalTime - driver.AvailabilityEnd));
                        }
                        foreach (var id in route.Schedule[i].UnloadedRequestsIds)
                        {
                            double early = requestsDict[id].DeliveryPreferedTimeWindowStart - route.Schedule[i].ArrivalTime;
                            MaxEarlyArrival = TimeSpan.FromSeconds(Math.Max(
                                MaxEarlyArrival.TotalSeconds,
                                early));
                            TotalEarlyArrival += TimeSpan.FromSeconds(Math.Max(0, early));
                            double late = route.Schedule[i].ArrivalTime - requestsDict[id].DeliveryPreferedTimeWindowEnd;
                            MaxDelayTime = TimeSpan.FromSeconds(Math.Max(
                                MaxDelayTime.TotalSeconds,
                                late));
                            MaxServiceDelayTime = TimeSpan.FromSeconds(Math.Max(
                                MaxDelayTime.TotalSeconds,
                                late));

                            TotalDelay += TimeSpan.FromSeconds(Math.Max(0, late));
                        }
                        foreach (var id in route.Schedule[i].LoadedRequestsIds)
                        {
                            double early = requestsDict[id].PickupAvailableTimeWindowStart - route.Schedule[i].ArrivalTime;
                            MaxEarlyArrival = TimeSpan.FromSeconds(Math.Max(
                                MaxEarlyArrival.TotalSeconds,
                                early));
                            TotalEarlyArrival += TimeSpan.FromSeconds(Math.Max(0, early));
                            double late = route.Schedule[i].ArrivalTime - requestsDict[id].PickupPreferedTimeWindowEnd;
                            MaxDelayTime = TimeSpan.FromSeconds(Math.Max(
                                MaxDelayTime.TotalSeconds,
                                late));
                            MaxServiceDelayTime = TimeSpan.FromSeconds(Math.Max(
                                MaxDelayTime.TotalSeconds,
                                late));
                            TotalDelay += TimeSpan.FromSeconds(Math.Max(0, late));
                        }
                    }
                    if (fillRatio < 0.5)
                    {
                        CountFillInHalf += 1;
                        LengthFillInHalf += routeLength;
                    }

                    EPSizeSumAtInitialLoad = epSums.Any() ? Math.Round(epSums.Average(), 2) : 0.0;
                    PackgeCountAtInitialLoad = packageCounts.Any() ? (int)packageCounts.Average() : 0;


                    Cost += Length < vehicle.VehicleMaxRouteLengthForFlatCost ? vehicle.VehicleFlatCostForShortRouteLength : (routeLength * vehicle.VehicleCostPerDistanceUnit);
                    Cost += (routeTime * vehicle.VehicleCostPerTimeUnit);
                    Cost += vehicle.VehicleCostPerRoute;
                    if (route.TractorId > 0)
                    {
                        vehicle = vehiclesDict[route.TractorId];
                        Cost += Length < vehicle.VehicleMaxRouteLengthForFlatCost ? vehicle.VehicleFlatCostForShortRouteLength : (routeLength * vehicle.VehicleCostPerDistanceUnit);
                        Cost += (routeTime * vehicle.VehicleCostPerTimeUnit);
                        Cost += vehicle.VehicleCostPerRoute;
                    }
                }
                LengthFillInHalf = Math.Round(LengthFillInHalf / 1000);
                if (fillRatios.Any())
                {
                    FillRatio = Math.Round(fillRatios.Average(), 2);
                }
                Length = Math.Round(Length);
                Routes = newRoutes.Count();
                TravelTimeStr = $"{TotalTravelTime.Days * 24 + TotalTravelTime.Hours}:{TotalTravelTime.Minutes:00}";
                MaxDelayStr = $"{MaxDelayTime.Days * 24 + MaxDelayTime.Hours}:{MaxDelayTime.Minutes:00}";
                MaxServiceDelayStr = $"{MaxServiceDelayTime.Days * 24 + MaxServiceDelayTime.Hours}:{MaxServiceDelayTime.Minutes:00}";
                TotalDelayStr = $"{TotalDelay.Days * 24 + TotalDelay.Hours}:{TotalDelay.Minutes:00}";
                MaxEarlyStr = $"{MaxEarlyArrival.Days * 24 + MaxEarlyArrival.Hours}:{MaxEarlyArrival.Minutes:00}";
                TotalEarlyStr = $"{TotalEarlyArrival.Days * 24 + TotalEarlyArrival.Hours}:{TotalEarlyArrival.Minutes:00}";
                Visits = newRoutes.Sum(rt => rt.Schedule.Count - 2);
                LeftEP = dtoWithSolution.Requests.Where(rq => dtoWithSolution.Solutions[0].LeftRequestsIds.Contains(rq.Id)).Sum(rq => rq.PackageCount);
                UniqueTrailersCount = newRoutes.Where(rt => rt.TractorId > 0).Select(rt => rt.TrailerTruckId).Distinct().Count();
                UniqueTrucksCount = newRoutes.Where(rt => rt.TractorId <= 0).Select(rt => rt.TrailerTruckId).Distinct().Count();
                UniqueTractorCount = newRoutes.Where(rt => rt.TractorId > 0).Select(rt => rt.TractorId).Distinct().Count();
                UniqueDriversCount= newRoutes.Where(rt => rt.DriverId > 0).Select(rt => rt.DriverId).Distinct().Count();
                NecessaryDriversCount = VRPCostFunction.ComputeDriversCount(routes);
                Cost += newRoutes.Select(rt => rt.TrailerTruckId).Distinct().Sum(id => vehiclesDict[id].VehicleCostPerUsage);
                Cost += newRoutes.Where(rt => rt.TractorId > 0).Select(rt => rt.TractorId).Distinct().Sum(id => vehiclesDict[id].VehicleCostPerUsage);
                Cost = Math.Round(Cost);
            }
        }

        public static void LogResultInCSV(string csvFileName, string inputFileName, string experimentName, string depotId, DateTime date, VRPResultStatistics stats, VRPCostFunction costFunction)
        {
            if (!File.Exists(csvFileName))
            {
                File.AppendAllText(csvFileName, "InputFile;");
                File.AppendAllText(csvFileName, "Experiment;");
                File.AppendAllText(csvFileName, "Timestamp;");
                File.AppendAllText(csvFileName, "Depot;");
                File.AppendAllText(csvFileName, "Date;");
                File.AppendAllText(csvFileName, "CostFunction;");
                File.AppendAllText(csvFileName, "Length;");
                File.AppendAllText(csvFileName, "Routes;");
                File.AppendAllText(csvFileName, "Trucks;");
                File.AppendAllText(csvFileName, "Tractors;");
                File.AppendAllText(csvFileName, "Trailers;");
                File.AppendAllText(csvFileName, "Drivers;");
                File.AppendAllText(csvFileName, "NecessaryDrivers;");
                File.AppendAllText(csvFileName, "LeftRequests;");
                File.AppendAllText(csvFileName, "LeftEP;");
                File.AppendAllText(csvFileName, "FillRatio;");
                File.AppendAllText(csvFileName, "PackageCount;");
                File.AppendAllText(csvFileName, "Costs;");
                File.AppendAllText(csvFileName, "MaxEarly;");
                File.AppendAllText(csvFileName, "TotalEarly;");
                File.AppendAllText(csvFileName, "MaxDelay;");
                File.AppendAllText(csvFileName, "MaxServiceDelay;");
                File.AppendAllText(csvFileName, "TotalDelay;");
                File.AppendAllText(csvFileName, "MaxSpread;");
                File.AppendAllText(csvFileName, "MaxLateStart;");
                File.AppendAllText(csvFileName, "FunctionParams");
                File.AppendAllText(csvFileName, Environment.NewLine);
            }

            string experimentDescription = $"{inputFileName};{experimentName};{DateTime.Now.ToLocalTime()};{depotId};{date.ToString("yyyy-MM-dd")};";
            StringWriter swCostFunctionEstimator = new StringWriter();
            JsonSerializer.Create().Serialize(swCostFunctionEstimator, costFunction);
            string costFunctionParams = swCostFunctionEstimator.ToString();

            var newEntryStringBuilder = new StringBuilder();
            newEntryStringBuilder.Append($"{Math.Round(stats.FunctionCost, 2)};");
            newEntryStringBuilder.Append($"{stats.Length};");
            newEntryStringBuilder.Append($"{stats.Routes};");
            newEntryStringBuilder.Append($"{stats.UniqueTrucksCount};");
            newEntryStringBuilder.Append($"{stats.UniqueTractorCount};");
            newEntryStringBuilder.Append($"{stats.UniqueTrailersCount};");
            newEntryStringBuilder.Append($"{stats.UniqueDriversCount};");
            newEntryStringBuilder.Append($"{stats.NecessaryDriversCount};");
            newEntryStringBuilder.Append($"{stats.LeftRequests};");
            newEntryStringBuilder.Append($"{stats.LeftEP};");
            newEntryStringBuilder.Append($"{stats.FillRatio};");
            newEntryStringBuilder.Append($"{stats.PackgeCountAtInitialLoad};");
            newEntryStringBuilder.Append($"{stats.Cost};");
            newEntryStringBuilder.Append($"{stats.MaxEarlyStr};");
            newEntryStringBuilder.Append($"{stats.TotalEarlyStr};");
            newEntryStringBuilder.Append($"{stats.MaxDelayStr};");
            newEntryStringBuilder.Append($"{stats.MaxServiceDelayStr};");
            newEntryStringBuilder.Append($"{stats.TotalDelayStr};");
            newEntryStringBuilder.Append($"{stats.MaxSpreadTimeStr};");
            newEntryStringBuilder.Append($"{stats.MaxLateStartStr};");
            newEntryStringBuilder.Append($"{costFunctionParams}");
            string newEntry = newEntryStringBuilder.ToString();
            File.AppendAllText(csvFileName, experimentDescription);
            File.AppendAllText(csvFileName, newEntry);
            File.AppendAllText(csvFileName, Environment.NewLine);
        }
    }
}