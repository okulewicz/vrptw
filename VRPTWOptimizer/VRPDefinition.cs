using CommonGIS;
using CommonGIS.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using VRPTWOptimizer.Interfaces;

namespace VRPTWOptimizer
{
    /// <summary>
    /// Describes data for a generalized Vehicle Routing Problem
    /// </summary>
    public class VRPDefinition
    {
        private class LawAbidingFloatConverter : JsonConverter
        {
            public override bool CanRead
            {
                get
                {
                    return false;
                }
            }
            public override bool CanWrite
            {
                get
                {
                    return true;
                }
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(double) || objectType == typeof(float);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var val = value as double? ?? (double?)(value as float?);
                if (val == null || Double.IsNaN((double)val) || Double.IsInfinity((double)val))
                {
                    writer.WriteNull();
                    return;
                }
                writer.WriteValue((double)val);
            }
        }

        /// <summary>
        /// Description of the company seeking solution to VRP problem
        /// </summary>
        public string Client { get; set; }
        /// <summary>
        /// Factors to be taken into account while optimizing the problem
        /// </summary>
        public VRPCostFunction CostFunctionFactors { get; set; }
        /// <summary>
        /// Version of the library containg data format definition
        /// </summary>
        public Version DataFormatVersion => Assembly.GetAssembly(new VRPDefinition().GetType()).GetName().Version;

        /// <summary>
        /// Date for which the problem is solved
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Depot for which the problem is solved
        /// </summary>
        public string DepotId { get; set; }
        /// <summary>
        /// Data for computing distances between points (straightforward matrix, formula description, API access information)
        /// </summary>
        public IDistanceProvider DistanceData { get; set; }
        /// <summary>
        /// List of drivers
        /// </summary>
        public List<Driver> Drivers { get; set; }
        /// <summary>
        /// List of TransportRequests from given day in given depot
        /// </summary>
        public List<TransportRequest> Requests { get; set; }
        /// <summary>
        /// List of Locations in given day
        /// </summary>
        public List<Location> Locations { get; set; }

        /// <summary>
        /// Object describing parameters of service (loading/unloading) time estimator model
        /// </summary>
        public ITimeEstimator ServiceTimeEstimator { get; set; }
        /// <summary>
        /// List of solutions (vehicles assignments and schedule)
        /// </summary>
        public List<VRPSolution> Solutions { get; set; }
        /// <summary>
        /// List of available vehicles
        /// </summary>
        public List<Vehicle> Vehicles { get; set; }
        /// <summary>
        /// Real time value to computer relative seconds against to retrieve real timestamps
        /// </summary>
        public DateTime ZeroHour { get; set; }

        /// <summary>
        /// Creates VRPProblemDefinition standard format from problem description
        /// </summary>
        /// <param name="vrpProvider"></param>
        /// <param name="billingDate"></param>
        /// <param name="distanceProvider"></param>
        /// <param name="timeEstimator"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        [Obsolete("Please use GenerateVRPDefintion with conscious costFunctionFactors definition")]
        public static VRPDefinition GenerateVRPDefintion(
            IVRPProvider vrpProvider,
            DateTime billingDate,
            IDistanceProvider distanceProvider,
            ITimeEstimator timeEstimator,
            string client)
        {
            VRPCostFunction costFunctionFactors = VRPCostFunction.GetDefaultParametersFunction();
            return GenerateVRPDefintion(
                vrpProvider,
                costFunctionFactors,
                billingDate,
                distanceProvider,
                timeEstimator,
                client);
        }

        /// <summary>
        /// Creates VRPProblemDefinition standard format from problem description
        /// </summary>
        /// <param name="vrpProvider"></param>
        /// <param name="costFunctionFactors"></param>
        /// <param name="billingDate"></param>
        /// <param name="distanceProvider"></param>
        /// <param name="timeEstimator"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static VRPDefinition GenerateVRPDefintion(
            IVRPProvider vrpProvider,
            VRPCostFunction costFunctionFactors,
            DateTime billingDate,
            IDistanceProvider distanceProvider,
            ITimeEstimator timeEstimator,
            string client)
        {
            List<VRPSolution> vrpSolutions = new();
            Dictionary<string,Location> locations = new Dictionary<string,Location>();
            locations.TryAdd(vrpProvider.HomeDepot.Id, vrpProvider.HomeDepot);
            foreach (var request in vrpProvider.Requests)
            {
                locations.TryAdd(request.PickupLocation.Id, request.PickupLocation);
                locations.TryAdd(request.DeliveryLocation.Id, request.DeliveryLocation);
            }
            foreach (var vehicle in vrpProvider.Vehicles)
            {
                locations.TryAdd(vehicle.InitialLocation.Id, vehicle.InitialLocation);
                locations.TryAdd(vehicle.FinalLocation.Id, vehicle.FinalLocation);
            }
            VRPDefinition vrpDefinition = new()
            {
                CostFunctionFactors = costFunctionFactors,
                Requests = vrpProvider.Requests,
                Locations = locations.Values.ToList(),
                ServiceTimeEstimator = timeEstimator,
                Vehicles = vrpProvider.Vehicles,
                Drivers = vrpProvider.Drivers,
                Solutions = vrpSolutions,
                Date = billingDate.Date,
                DepotId = vrpProvider.HomeDepot.Id,
                Client = client,
                ZeroHour = vrpProvider.ZeroHour,
                DistanceData = distanceProvider
            };

            return vrpDefinition;
        }

        /// <summary>
        /// Adds solution to the collection of problem solutions
        /// </summary>
        /// <param name="vrpSolution"></param>
        public void AddSolution(VRPSolution vrpSolution)
        {
            if (Solutions == null)
            {
                Solutions = new();
            }
            Solutions.Add(vrpSolution);
        }

        /// <summary>
        /// Generates indended JSON definition of VRP
        /// </summary>
        /// <returns></returns>
        public string ToPrettyJSONString()
        {
            var settings = new JsonSerializerSettings();
            var floatConverter = new LawAbidingFloatConverter();
            settings.Converters.Add(floatConverter);
            settings.Formatting = Formatting.Indented;
            var serializer = JsonSerializer.Create(settings);
            var writer = new StringWriter();
            serializer.Serialize(writer, this);
            return writer.ToString();
        }

        /// <summary>
        /// Writes VRPDefinition and VRPSolutions list to a JSON file using pretty formatter
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool TrySaveToFile(string filename)
        {
            string contents = this.ToPrettyJSONString();
            try
            {
                File.WriteAllText(filename, contents);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Cannot access {filename}");
                Console.Write(contents);
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine($"Designated location directory {filename} does not exists");
                Console.Write(contents);
                return false;
            }
            catch (IOException)
            {
                Console.WriteLine($"Cannot create {filename} for unknown reason");
                Console.Write(contents);
                return false;
            }
        }
    }
}