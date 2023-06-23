using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRPTWOptimizer.Interfaces;

namespace VRPTWOptimizer.Dto
{
    /// <summary>
    /// Complete picking schedule for one day
    /// </summary>
    public class PickingSchedule
    {
        private List<VehicleSchedule> vehicleSchedules = new List<VehicleSchedule>();
        /// <summary>
        /// URI where updated picking schedule should be sent back
        /// </summary>
        [JsonProperty("callbackUrl")]
        public string CallbackUrl { get; set; }
        /// <summary>
        /// Identifier of picking schedule
        /// </summary>
        [JsonProperty("ordersId")]
        public int Id { get; set; }
        /// <summary>
        /// DateTime when picking schedule was generated
        /// </summary>
        [JsonProperty("ordersCreateDate")]
        public DateTime OrdersCreateDate { get; set; }
        /// <summary>
        /// DateTime when first shift of picking starts
        /// </summary>
        [JsonProperty("ordersPickingStart")]
        public DateTime OrdersPickingStart { get; set; }
        /// <summary>
        /// Picking orders divided among transports
        /// </summary>
        [JsonProperty("vdPickingLists")]
        public List<TransportPickingLists> PickingLists { get; set; }
        /// <summary>
        /// Schedule of vehicles presence within the warehouse bounds
        /// </summary>
        [JsonProperty("vehiclesAvailability")]
        public List<VehicleSchedule> VehiclesAvailability { get; set; }

        /// <summary>
        /// Creates picking schedule from VRP definition and solution
        /// </summary>
        /// <param name="vrpDefinition"></param>
        /// <param name="vrpSolution"></param>
        /// <returns></returns>
        public static PickingSchedule GeneratePickingSchedule(VRPDefinition vrpDefinition, VRPSolution vrpSolution)
        {
            List<TransportPickingLists> transportPickingLists = new List<TransportPickingLists>();
            List<VehicleSchedule> vehicleSchedules = new List<VehicleSchedule>();
            foreach (var vehicle in vrpDefinition.Vehicles)
            {
                var vehicleSchedule = new VehicleSchedule();
                vehicleSchedule.CapacityVehicleType = vehicle.Type;
                vehicleSchedule.VehicleId = vehicle.Id;
                //HACK: this is not right - better to extend the model with informative variables
                vehicleSchedule.EpCapacity = vehicle.RoadProperties.EpCount;
                vehicleSchedule.YardAvailabilitySchedule = new List<TimeInterval>();
                double availabilityStart = vehicle.AvailabilityStart;
                double availabilityEnd = vehicle.AvailabilityEnd;
                foreach (var route in vrpSolution.Transports.Where(tr => tr.TractorId == vehicleSchedule.VehicleId || tr.TrailerTruckId == vehicleSchedule.VehicleId).OrderBy(tr => tr.AvailableForLoadingTime))
                {
                    availabilityEnd = route.AvailableForLoadingTime;
                    vehicleSchedule.YardAvailabilitySchedule.Add(
                        new TimeInterval()
                        {
                            AvailabilityStart = vrpDefinition.ZeroHour.AddSeconds(availabilityStart),
                            AvailabilityEnd = vrpDefinition.ZeroHour.AddSeconds(availabilityEnd)
                        });
                    availabilityStart = route.AvailableForNextAssignmentTime;
                }
                availabilityEnd = vehicle.AvailabilityEnd;
                vehicleSchedule.YardAvailabilitySchedule.Add(
                    new TimeInterval()
                    {
                        AvailabilityStart = vrpDefinition.ZeroHour.AddSeconds(availabilityStart),
                        AvailabilityEnd = vrpDefinition.ZeroHour.AddSeconds(availabilityEnd)
                    });
                vehicleSchedules.Add(vehicleSchedule);
            }
            foreach (var route in vrpSolution.Transports)
            {
                Vehicle vehicle = vrpDefinition.Vehicles.First(v => v.Id == route.TrailerTruckId);
                List<StorePickingList> storePickingLists = new List<StorePickingList>();
                var maxAllowedVehicleCapacityForTransport = int.MaxValue;
                foreach (var requestId in route.Schedule[0].LoadedRequestsIds)
                {
                    var request = vrpDefinition.Requests.First(rq => rq.Id == requestId);
                    maxAllowedVehicleCapacityForTransport = Math.Min(maxAllowedVehicleCapacityForTransport, request.MaxVehicleSize.EpCount);
                    var visitsDictionary = route.Schedule
                        .Select((sch, idx) => new KeyValuePair<int, string>(idx, sch.LocationId))
                        .ToList();

                    StorePickingList storePickingList = new StorePickingList()
                    {
                        DeliveryLocationId = request.DeliveryLocation.Id,
                        EpCount = request.Size[0],
                        LoadingOrder = visitsDictionary.Count - visitsDictionary.First(vd => vd.Value == request.DeliveryLocation.Id).Key - 1,
                        GoodsList = new List<CargoUnit>()
                    };
                    storePickingLists.Add(storePickingList);
                }
                storePickingLists = storePickingLists.OrderBy(st => st.LoadingOrder).ToList();
                for (int i = 0; i < storePickingLists.Count; i++)
                {
                    storePickingLists[i].LoadingOrder = i + 1;
                }
                if (storePickingLists.Count > 0)
                {
                    var transport = new TransportPickingLists()
                    {
                        TransportId = route.TransportId,
                        CapacityVehicleType = vehicle.Type,
                        DesiredDepartureTime = vrpDefinition.ZeroHour.AddSeconds(route.Schedule[0].DepartureTime),
                        EpCapacity = (int)vehicle.Capacity[0],
                        MaxEpCapacity = maxAllowedVehicleCapacityForTransport,
                        SemiTrailerTruckId = vehicle.Id,
                        StoreOrders = storePickingLists
                    };
                    transportPickingLists.Add(transport);
                }
            }

            return new PickingSchedule()
            {
                CallbackUrl = "http://fakeurl.com:5050/loadSchedule",
                Id = 1,
                OrdersCreateDate = vrpDefinition.Date.AddDays(-2),
                OrdersPickingStart = vrpDefinition.ZeroHour,
                PickingLists = transportPickingLists,
                VehiclesAvailability = vehicleSchedules
            };
        }

        /// <summary>
        /// Creates picking schedule from VRP routes and available vehicles
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="vehicles"></param>
        /// <returns></returns>
        public static PickingSchedule GeneratePickingSchedule(List<IRoute> routes, List<Vehicle> vehicles)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes picking schedule to designated JSON file
        /// </summary>
        /// <param name="filename"></param>
        public void TrySaveToFile(string filename)
        {
            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            var serializer = JsonSerializer.Create(settings);
            var writer = new StringWriter();
            serializer.Serialize(writer, this);
            try
            {
                File.WriteAllText(filename, writer.ToString());
            }
            catch (IOException)
            {
                Console.Write(writer.ToString());
            }
        }
    }
}