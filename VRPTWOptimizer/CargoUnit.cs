using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRPTWOptimizer.Enums;

namespace VRPTWOptimizer
{
    /// <summary>
    /// Describes a single position on an order list (depending on the context it could be a europallet or single type of product with its quantity)
    /// </summary>
    public class CargoUnit
    {
        /// <summary>
        /// Group of goods (e.g. fresh meat)
        /// </summary>
        [JsonProperty("zoneId")]
        public string CargoGroup { get; set; }
        /// <summary>
        /// Smallest considered piece of cargo (e.g. piece, box, europallet)
        /// </summary>
        [JsonProperty("cargoUnitType")]
        public CargoUnitType CargoUnitType { get; set; }
        /// <summary>
        /// Identifier of product
        /// </summary>
        [JsonProperty("goodId")]
        public int GoodsId { get; set; }
        /// <summary>
        /// Name of product
        /// </summary>
        [JsonProperty("goodName")]
        public string GoodsName { get; set; }
        /// <summary>
        /// The smaller the number the higher delivery priority
        /// </summary>
        [JsonProperty("importance")]
        public int Priority { get; set; }
        /// <summary>
        /// Cargo size specified in the units used to compute transportation capacity
        /// </summary>
        [JsonProperty("volume")]
        public double[] Size { get; set; }
        /// <summary>
        /// Number of cargo units (e.g. 12 boxes)
        /// </summary>
        [JsonProperty("quantity")]
        public int UnitsCount { get; set; }
    }
}