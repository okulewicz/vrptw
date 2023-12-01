using CommonGIS;
using CommonGIS.Interfaces;
using g3;
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
        public double Cost { get; private set; }
        public double LengthCost { get; private set; }
        public double FunctionCost { get; private set; }
        public double FillRatio { get; private set; }
        public int LeftEP { get; private set; }
        public object LeftRequests { get; private set; }
        public double Length { get; private set; }
        public string MaxDelayStr { get; private set; }
        public string MaxServiceDelayStr { get; private set; }
        public TimeSpan MaxDelayTime { get; private set; }
        public TimeSpan MaxServiceDelayTime { get; private set; }
        public TimeSpan MaxEarlyArrival { get; private set; }
        public string MaxEarlyStr { get; private set; }
        public TimeSpan MaxLateStart { get; private set; }
        public string MaxLateStartStr { get; private set; }
        public TimeSpan MaxSpreadTime { get; private set; }
        public string MaxSpreadTimeStr { get; private set; }
        public int Routes { get; private set; }
        public TimeSpan TotalDelay { get; private set; }
        public string TotalDelayStr { get; private set; }
        public string TotalImportantDelayStr { get; private set; }
        public TimeSpan TotalEarlyArrival { get; private set; }
        public string TotalEarlyStr { get; private set; }
        public TimeSpan TotalTravelTime { get; private set; }
        public string TravelTimeStr { get; private set; }
        public int UniqueTractorCount { get; private set; }
        public int UniqueTrailersCount { get; private set; }
        public int UniqueTrucksCount { get; private set; }
        public int Visits { get; private set; }
        public int CountFillInHalf { get; set; }
        public double LengthFillInHalf { get; set; }
        public double EPSizeSumAtInitialLoad { get; set; }
        public int PackgeCountAtInitialLoad { get; set; }
        public object UniqueDriversCount { get; private set; }
        public int NecessaryDriversCount { get; private set; }
        public int ConvexHullCount { get; private set; }
        public int MaxDCDeparturesCount { get; private set; }
        public int MaxDCDeparturesPackageCount { get; private set; }
        public TimeSpan TotalImportantDelay { get; private set; }
        public double CostWithoutUsage { get; private set; }
        public double TimeCost { get; private set; }
        public double RoutesCost { get; private set; }

        /// <summary>
        /// Creates object computing statistics over provided VRPResult object
        /// </summary>
        /// <param name="newResult"></param>
        public VRPResultStatistics(VRPOptimizerResult newResult, VRPCostFunction vrpCostFunction)
        {
            FillInStatistics(newResult, vrpCostFunction);
        }

        private void FillInStatistics(VRPOptimizerResult newResult, VRPCostFunction vrpCostFunction)
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
            TotalImportantDelay = TimeSpan.FromSeconds(newRoutes.Sum(rt => VRPCostFunction.ComputeImportantDelays(rt)));
            MaxDCDeparturesCount = 0;
            MaxDCDeparturesPackageCount = 0;
            if (newResult.Routes.Any())
            {
                for (double time = 0; time < newResult.Routes.Max(rt => rt.DepartureTimes[0]) + 1; time += 3600)
                {
                    var routesInHour = newResult.Routes.Where(rt => rt.DepartureTimes[0] >= time && rt.DepartureTimes[0] < time + 3600);
                    MaxDCDeparturesCount = Math.Max(MaxDCDeparturesCount, routesInHour.Count());
                    MaxDCDeparturesPackageCount = Math.Max(MaxDCDeparturesPackageCount, routesInHour.Sum(rt => rt.LoadedRequests[0].Sum(rq => rq.PackageCount)));
                }
            }
            TotalEarlyArrival = TimeSpan.FromSeconds(newRoutes.Sum(rt => VRPCostFunction.ComputeTotalEarlyArrival(rt)));
            Routes = newRoutes.Count();
            Length = Math.Round(newRoutes.Sum(rt => rt.Length) / 1000);
            Cost = 0.0;
            LengthCost = newRoutes.Sum(rt => (rt.Length < rt.Vehicle.VehicleMaxRouteLengthForFlatCost ? rt.Vehicle.VehicleFlatCostForShortRouteLength : rt.Vehicle.VehicleCostPerDistanceUnit * rt.Length));
            LengthCost += newRoutes.Where(rt => rt.VehicleTractor != null).Sum(rt => (rt.Length < rt.VehicleTractor.VehicleMaxRouteLengthForFlatCost ? rt.VehicleTractor.VehicleFlatCostForShortRouteLength : rt.VehicleTractor.VehicleCostPerDistanceUnit * rt.Length));
            Cost += LengthCost;
            TimeCost = newRoutes.Sum(rt => (rt.TravelTime * rt.Vehicle.VehicleCostPerTimeUnit));
            TimeCost += newRoutes.Where(rt => rt.VehicleTractor != null).Sum(rt => (rt.TravelTime * rt.VehicleTractor.VehicleCostPerTimeUnit));
            Cost += TimeCost;
            RoutesCost = newRoutes.Sum(rt => (rt.Vehicle.VehicleCostPerRoute));
            RoutesCost += newRoutes.Where(rt => rt.VehicleTractor != null).Sum(rt => (rt.VehicleTractor.VehicleCostPerRoute));
            Cost += RoutesCost;
            CostWithoutUsage = Math.Round(Cost);
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
            EPSizeSumAtInitialLoad = newRoutes.Any() ? Math.Round(newRoutes.Average(rt => rt.LoadedRequests[0].Sum(rq => rq.Size[0])), 2) : 0.0;
            MaxDelayStr = $"{MaxDelayTime.Days * 24 + MaxDelayTime.Hours}:{MaxDelayTime.Minutes:00}";
            MaxServiceDelayStr = $"{MaxServiceDelayTime.Days * 24 + MaxServiceDelayTime.Hours}:{MaxServiceDelayTime.Minutes:00}";
            TotalDelayStr = $"{TotalDelay.Days * 24 + TotalDelay.Hours}:{TotalDelay.Minutes:00}";
            TotalImportantDelayStr = $"{TotalImportantDelay.Days * 24 + TotalImportantDelay.Hours}:{TotalImportantDelay.Minutes:00}";
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
            ConvexHullCount = VRPCostFunction.FairlyCountIntersectingConvexHulls(newResult.Routes);
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
                }
                else
                {
                    dtoWithSolution.CostFunctionFactors = VRPCostFunction.GetDefaultParametersFunction();
                }
                FillInStatistics(new VRPOptimizerResult()
                { LeftRequests = leftRequests, Routes = routes }, dtoWithSolution.CostFunctionFactors);
            }
        }

        public static void LogResultInCSV(string csvFileName, string inputFileName, string experimentName, string depotId, DateTime date, VRPResultStatistics stats, VRPCostFunction costFunction, long evaluationsCount, Dictionary<string, object> config)
        {
            if (!File.Exists(csvFileName))
            {
                File.AppendAllText(csvFileName, "InputFile;");
                File.AppendAllText(csvFileName, "Experiment;");
                File.AppendAllText(csvFileName, "Timestamp;");
                File.AppendAllText(csvFileName, "Depot;");
                File.AppendAllText(csvFileName, "Date;");
                File.AppendAllText(csvFileName, "Evaluations;");
                File.AppendAllText(csvFileName, "CostFunction;");
                File.AppendAllText(csvFileName, "Length;");
                File.AppendAllText(csvFileName, "CHCount;");
                File.AppendAllText(csvFileName, "Visits;");
                File.AppendAllText(csvFileName, "Routes;");
                File.AppendAllText(csvFileName, "Trucks;");
                File.AppendAllText(csvFileName, "Tractors;");
                File.AppendAllText(csvFileName, "Trailers;");
                File.AppendAllText(csvFileName, "Drivers;");
                File.AppendAllText(csvFileName, "NecessaryDrivers;");
                File.AppendAllText(csvFileName, "MaxDepartures;");
                File.AppendAllText(csvFileName, "MaxDeparturesPackages;");
                File.AppendAllText(csvFileName, "LeftRequests;");
                File.AppendAllText(csvFileName, "LeftEP;");
                File.AppendAllText(csvFileName, "FillRatio;");
                File.AppendAllText(csvFileName, "PackageCount;");
                File.AppendAllText(csvFileName, "Costs;");
                File.AppendAllText(csvFileName, "CostsWithoutUsage;");
                File.AppendAllText(csvFileName, "CostsLength;");
                File.AppendAllText(csvFileName, "CostsTime;");
                File.AppendAllText(csvFileName, "CostsRoutes;");
                File.AppendAllText(csvFileName, "MaxEarly;");
                File.AppendAllText(csvFileName, "TotalEarly;");
                File.AppendAllText(csvFileName, "MaxDelay;");
                File.AppendAllText(csvFileName, "MaxServiceDelay;");
                File.AppendAllText(csvFileName, "TotalDelay;");
                File.AppendAllText(csvFileName, "TotalImportantDelay;");
                File.AppendAllText(csvFileName, "MaxSpread;");
                File.AppendAllText(csvFileName, "MaxLateStart;");
                File.AppendAllText(csvFileName, "FunctionParams;");
                File.AppendAllText(csvFileName, "AlgorithmParams");
                File.AppendAllText(csvFileName, Environment.NewLine);
            }

            string experimentDescription = $"{inputFileName};{experimentName};{DateTime.Now.ToLocalTime()};{depotId};{date.ToString("yyyy-MM-dd")};";
            StringWriter swCostFunctionEstimator = new StringWriter();
            JsonSerializer.Create().Serialize(swCostFunctionEstimator, costFunction);
            string costFunctionParams = swCostFunctionEstimator.ToString();
            StringWriter swAlgorithmConfig = new StringWriter();
            JsonSerializer.Create().Serialize(swAlgorithmConfig, config);
            string algorithmConfig = swAlgorithmConfig.ToString();

            var newEntryStringBuilder = new StringBuilder();
            newEntryStringBuilder.Append($"{evaluationsCount};");
            newEntryStringBuilder.Append($"{Math.Round(stats.FunctionCost, 2)};");
            newEntryStringBuilder.Append($"{stats.Length};");
            newEntryStringBuilder.Append($"{stats.ConvexHullCount};");
            newEntryStringBuilder.Append($"{stats.Visits};");
            newEntryStringBuilder.Append($"{stats.Routes};");
            newEntryStringBuilder.Append($"{stats.UniqueTrucksCount};");
            newEntryStringBuilder.Append($"{stats.UniqueTractorCount};");
            newEntryStringBuilder.Append($"{stats.UniqueTrailersCount};");
            newEntryStringBuilder.Append($"{stats.UniqueDriversCount};");
            newEntryStringBuilder.Append($"{stats.NecessaryDriversCount};");
            newEntryStringBuilder.Append($"{stats.MaxDCDeparturesCount};");
            newEntryStringBuilder.Append($"{stats.MaxDCDeparturesPackageCount};");
            newEntryStringBuilder.Append($"{stats.LeftRequests};");
            newEntryStringBuilder.Append($"{stats.LeftEP};");
            newEntryStringBuilder.Append($"{stats.FillRatio};");
            newEntryStringBuilder.Append($"{stats.PackgeCountAtInitialLoad};");
            newEntryStringBuilder.Append($"{stats.Cost};");
            newEntryStringBuilder.Append($"{stats.CostWithoutUsage};");
            newEntryStringBuilder.Append($"{stats.LengthCost};");
            newEntryStringBuilder.Append($"{stats.TimeCost};");
            newEntryStringBuilder.Append($"{stats.RoutesCost};");
            newEntryStringBuilder.Append($"{stats.MaxEarlyStr};");
            newEntryStringBuilder.Append($"{stats.TotalEarlyStr};");
            newEntryStringBuilder.Append($"{stats.MaxDelayStr};");
            newEntryStringBuilder.Append($"{stats.MaxServiceDelayStr};");
            newEntryStringBuilder.Append($"{stats.TotalDelayStr};");
            newEntryStringBuilder.Append($"{stats.TotalImportantDelayStr};");
            newEntryStringBuilder.Append($"{stats.MaxSpreadTimeStr};");
            newEntryStringBuilder.Append($"{stats.MaxLateStartStr};");
            newEntryStringBuilder.Append($"{costFunctionParams};");
            newEntryStringBuilder.Append($"{algorithmConfig}");
            string newEntry = newEntryStringBuilder.ToString();
            File.AppendAllText(csvFileName, experimentDescription);
            File.AppendAllText(csvFileName, newEntry);
            File.AppendAllText(csvFileName, Environment.NewLine);
        }
    }
}