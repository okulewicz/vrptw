using CommonGIS;
using CommonGIS.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VRPTWOptimizer;
using VRPTWOptimizer.Interfaces;
using VRPTWOptimizer.Utils.Model;
using VRPTWOptimizer.Utils.Provider;
using VRPTWOptimizer.Utils.Validation;
using VRPTWOptimizer.Utils.VrpDefinition;

namespace VRPTWVerifier
{
    internal class Program
    {
        private const string StatisticsOutputFilename = "index.csv";

        private static void Main(string[] args)
        {
            bool shouldFix = false;
            bool shouldRemoveLeftRequests = false;
            bool createDriversFromVehicles = false;
            foreach (var arg in args)
            {
                if (arg.Equals("fix=true"))
                {
                    shouldFix = true;
                }
                if (arg.Equals("thin=true"))
                {
                    shouldRemoveLeftRequests = true;
                }
                if (arg.Equals("create-drivers=true"))
                {
                    createDriversFromVehicles = true;
                }
            }
            foreach (var arg in args)
            {
                if (Directory.Exists(arg) && arg != "." && arg != "..")
                {
                    DirectoryInfo di = new DirectoryInfo(arg);
                    Main(di.GetFiles()
                        .Select(fi => fi.FullName)
                        .Concat(new string[] { $"fix={shouldFix.ToString().ToLowerInvariant()}", $"thin={shouldRemoveLeftRequests.ToString().ToLowerInvariant()}" }).ToArray());
                }
                else if (File.Exists(arg))
                {
                    try
                    {
                        VerifyFileAsVRP(arg, shouldFix, shouldRemoveLeftRequests, createDriversFromVehicles);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static void VerifyFileAsVRP(string filename, bool shouldFix, bool shouldRemoveLeftRequests, bool createDriversFromVehicles)
        {
            FileInfo fi = new FileInfo(filename);
            if (!fi.Extension.ToLower().EndsWith("json"))
            {
                return;
            }
            string json = File.ReadAllText(filename);
            Dictionary<string, object> rawDTO = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            Console.WriteLine();
            Console.WriteLine($"Validating {filename} file");
            Console.WriteLine($"--------------------------");
            if (!rawDTO.ContainsKey("Vehicles"))
            {
                Console.WriteLine("This is probably not a VRPDefinition JSON file");
                return;
            }
            try
            {
                VRPDefinitionJSONDTO dto = JsonConvert.DeserializeObject<VRPDefinitionJSONDTO>(json, VRPJSONProviderFactory.settings);
                VRPValidator validator = new VRPValidator();
                IDistanceProvider distanceProvider = new DistanceMatrixProvider(dto.Distances);
                bool isFixed = false;
                bool trim = true;
                List<VRPTWOptimizer.TransportRequest> transportRequests = (dto.Requests ?? new List<RequestDTO>()).Select(r => r as VRPTWOptimizer.TransportRequest).ToList();
                List<VRPTWOptimizer.Vehicle> vehicles = (dto.Vehicles ?? new List<VehicleDTO>()).Select(v => v as VRPTWOptimizer.Vehicle).ToList();
                List<VRPTWOptimizer.Driver> drivers = (dto.Drivers ?? new List<DriverDTO>()).Select(d => d as VRPTWOptimizer.Driver).ToList();
                var errors = new List<ValidationError>();
                errors.AddRange(VRPValidator.ValidateProblemData(transportRequests, vehicles, distanceProvider));

                if (dto.Solutions != null)
                {
                    try
                    {
                        var solutions = new List<Solution>();
                        solutions.AddRange(dto.Solutions);
                        if (!(new DirectoryInfo("output").Exists))
                        {
                            Directory.CreateDirectory("output");
                        }
                        for (int i = 0; i < solutions.Count; i++)
                        {
                            var solution = solutions[i];
                            var solutionErrors = ValidateSolution(distanceProvider, transportRequests, vehicles, drivers, solution);
                            errors.AddRange(solutionErrors);
                            dto.Solutions.Clear();
                            dto.Solutions.Add(solution);
                            var stats = new VRPResultStatistics(dto, distanceProvider);
                            var fileInfo = new FileInfo(filename);
                            foreach (var summary in VRPResultSummary.PrepareMultipleSummaries(dto, distanceProvider))
                            {
                                File.WriteAllLines("output/" + fileInfo.Name.Replace(".json", "-" + summary.Key + ".csv"), summary.Value);
                            }
                            VRPResultStatistics.LogResultInCSV("output/" + StatisticsOutputFilename, fileInfo.Name, solution.Algorithm, dto.DepotId, dto.Date, stats, dto.CostFunctionFactors, 0, new());
                        }
                        dto.Solutions.Clear();
                        dto.Solutions.AddRange(solutions);
                    }
                    catch (IOException ex)
                    {
                        Console.Error.WriteLine("Cannot write to file");
                    }
                }
                else
                {
                    distanceProvider = new StraightLineDistanceProvider();
                    foreach (var request in transportRequests)
                    {
                        errors.AddRange(VRPValidator.ValidateRequestProperties(request));
                        errors.AddRange(VRPValidator.ValidateRequestAgainstVehiclesProperties(request, vehicles, distanceProvider));
                    }
                    foreach (var vehicle in vehicles)
                    {
                        errors.AddRange(VRPValidator.ValidateVehicleProperties(vehicle));
                    }
                }
                foreach (var error in errors)
                {
                    if (!error.Description.StartsWith("[Acceptable"))
                    {
                        Console.WriteLine(error.Description);
                    }
                }
                if (shouldFix)
                {
                    isFixed = VRPFixer.FixWrongVehicleType(dto, isFixed, errors);
                    isFixed = VRPFixer.FixMissingVehicles(dto, isFixed, vehicles, errors);
                    isFixed = VRPFixer.FixVehicleSize(dto, isFixed, vehicles, errors);
                    isFixed = VRPFixer.RemoveTransfersAndLoops(dto, isFixed);
                    if (trim)
                    {
                        isFixed = VRPFixer.TrimLeftInHistoric(dto, isFixed);
                    }
                    isFixed = VRPFixer.FixRequestLimits(dto, isFixed, errors);
                    isFixed = VRPFixer.FixRequestSize(isFixed, errors);
                    isFixed = VRPFixer.FixRequestsProperties(dto, isFixed, errors);
                    dto.Vehicles = vehicles.Select(v => v as VehicleDTO).ToList();
                }
                if (createDriversFromVehicles)
                {
                    isFixed = VRPFixer.FillInDrivers(dto, isFixed);
                }

                if (isFixed)
                {
                    string path = filename.Replace(".json", "-fixed.json");
                    if (trim)
                    {
                        path = path.Replace("-fixed.json", "-fixed-trimmed.json");
                    }
                    File.WriteAllText(path, JsonConvert.SerializeObject(dto));
                }
                if (shouldRemoveLeftRequests && dto.Solutions != null)
                {
                    if (dto.Solutions.Any(s => s.Algorithm.Contains("historic_solution")))
                    {
                        var historicSolution = dto.Solutions.First(s => s.Algorithm.Contains("historic_solution"));
                        dto.Requests.RemoveAll(rq => historicSolution.LeftRequestsIds.Contains(rq.Id));
                        historicSolution.LeftRequestsIds.Clear();
                        dto.Solutions.RemoveAll(sl => sl != historicSolution);
                        File.WriteAllText(filename.Replace(".json", "-historic-thinned.json"), JsonConvert.SerializeObject(dto));
                    }
                }
            }
            catch (JsonSerializationException jsex)
            {
                Console.WriteLine(jsex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }


        private static List<ValidationError> ValidateSolution(IDistanceProvider distanceProvider, List<TransportRequest> transportRequests, List<Vehicle> vehicles, List<Driver> drivers, Solution solution)
        {
            List<ValidationError> errors = new List<ValidationError>();
            Console.WriteLine($"Validating {solution.Algorithm}");
            foreach (var transportDTO in solution.Transports)
            {
                errors.AddRange(VRPValidator.ValidateDeserializedRoutesAgainstVehiclesAndDriversExistence(transportDTO, vehicles, drivers));
            }

            errors.AddRange(VRPValidator.ValidateRoutesDelaysAndDistance(solution.Transports, transportRequests, vehicles, drivers, distanceProvider));

            List<IRoute> routes = solution.Transports.Select(t => new RouteDto(t, transportRequests, vehicles, drivers, distanceProvider) as IRoute).ToList();
            List<VRPTWOptimizer.TransportRequest> leftRequests = solution.LeftRequestsIds.Select(id => transportRequests.FirstOrDefault(tr => tr.Id == id)).ToList();
            errors.AddRange(VRPValidator.ValidateSolutionData(
                routes,
                leftRequests,
                transportRequests));
            /*
            var tempRequests = dto.Requests.Where(rq => solution.LeftRequestsIds.Contains(rq.Id));
            var leftRequestsIds = new List<int>();
            leftRequestsIds.AddRange(solution.LeftRequestsIds);

            dto.Requests.RemoveAll(rq => solution.LeftRequestsIds.Contains(rq.Id));
            solution.LeftRequestsIds.Clear();
            dto.Solutions.Clear();
            dto.Solutions.Add(solution);
            File.WriteAllText(filename.Replace(".json", $"-{solution.Algorithm}.json"), JsonConvert.SerializeObject(dto));

            dto.Solutions.Clear();
            dto.Solutions.AddRange(solutions);
            dto.Requests.AddRange(tempRequests);
            solution.LeftRequestsIds.AddRange(leftRequestsIds);
            */
            return errors;  
        }
    }
}