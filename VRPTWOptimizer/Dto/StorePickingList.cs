using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRPTWOptimizer.Dto
{
    /// <summary>
    /// Picking order details for single store
    /// </summary>
    public class StorePickingList
    {
        /// <summary>
        /// Store identifier
        /// </summary>
        [JsonProperty("storeId")]
        public string DeliveryLocationId { get; set; }
        /// <summary>
        /// Total cargo size in europallets
        /// </summary>
        [JsonProperty("epCount")]
        public double EpCount { get; set; }
        /// <summary>
        /// List of ordered goods
        /// </summary>
        [JsonProperty("vdPickingListPosition")]
        public List<CargoUnit> GoodsList { get; set; }
        /// <summary>
        /// Loading order 1st to load is the last to deliver (stores form a stack of deliveries within the truck cargo hold)
        /// </summary>
        [JsonProperty("loadingOrder")]
        public int LoadingOrder { get; set; }
    }
}