using CommonGIS.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRPTWOptimizer.Dto
{
    /// <summary>
    /// Schedules vehicle presence at warehouse grounds
    /// </summary>
    public class VehicleSchedule
    {
        /// <summary>
        /// Type of Vehicle (truck or semi-trailer)
        /// </summary>
        [JsonProperty("capacityVehicleType")]
        public VehicleType CapacityVehicleType { get; set; }
        /// <summary>
        /// Size of the vehicle in europallets
        /// </summary>
        [JsonProperty("epCapacity")]
        public int EpCapacity { get; set; }
        /// <summary>
        /// Id of the vehicle
        /// </summary>
        [JsonProperty("semiTrailerTruckId")]
        public int VehicleId { get; set; }
        /// <summary>
        /// List of intervals when the vehicle is planned to be available for the warehouse to operate on (wash, refuel, load or park on yard)
        /// </summary>
        [JsonProperty("schedule")]
        public List<TimeInterval> YardAvailabilitySchedule { get; set; }
    }
}