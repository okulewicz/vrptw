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
    /// Picking orders for single transport
    /// </summary>
    public class TransportPickingLists
    {
        /// <summary>
        /// Type of capacity vehicle: integrated truck or semi-trailer
        /// </summary>
        [JsonProperty("capacityVehicleType")]
        public VehicleType CapacityVehicleType { get; set; }
        //TODO MO drukować datę bez strefy czasowej
        /// <summary>
        /// DateTime when the truck is designed to leave the warehouse
        /// </summary>
        [JsonProperty("desiredDepartureTime")]
        public DateTime DesiredDepartureTime { get; set; }
        /// <summary>
        /// Size of selected vehicle in europallets
        /// </summary>
        [JsonProperty("epCapacity")]
        public int EpCapacity { get; set; }
        /// <summary>
        /// Maximum allowed sized for the truck - minimum of stores upper limits
        /// </summary>
        [JsonProperty("maxEpCapacity")]
        public int MaxEpCapacity { get; set; }
        /// <summary>
        /// Identifier of truck or semi-trailer which will peroform the transport
        /// </summary>
        [JsonProperty("semiTrailerTruckId")]
        public int SemiTrailerTruckId { get; set; }
        /// <summary>
        /// List of all goods requested by store
        /// </summary>
        [JsonProperty("vdPickingListStores")]
        public List<StorePickingList> StoreOrders { get; set; }
        /// <summary>
        /// Identifier of transport (single route/loop)
        /// </summary>
        [JsonProperty("transportId")]
        public long TransportId { get; set; }
    }
}