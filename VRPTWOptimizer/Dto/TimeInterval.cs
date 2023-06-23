using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRPTWOptimizer.Dto
{
    /// <summary>
    /// Represents time interval for vehicle yard availability
    /// </summary>
    public class TimeInterval
    {
        /// <summary>
        /// End of the interval (vehicle leaves warehouse grounds)
        /// </summary>
        [JsonProperty("yardAvailabilityEnd")]
        public DateTime AvailabilityEnd { get; set; }
        /// <summary>
        /// Start of the interval (vehicle within warehouse grounds)
        /// </summary>
        [JsonProperty("yardAvailabilityStart")]
        public DateTime AvailabilityStart { get; set; }
    }
}