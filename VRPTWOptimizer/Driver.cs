using Newtonsoft.Json;
using System;

namespace VRPTWOptimizer
{
    /// <summary>
    /// Represents the truck/tractor driver
    /// </summary>
    public abstract class Driver : Resource
    {
        /// <summary>
        /// List of Vehicle Ids that the driver is able and allowed to drive
        /// </summary>
        public int[] CompatibileVehiclesIds { get; set; }

        /// <summary>
        /// List of owning companies for which the driver is working
        /// </summary>
        public int[] CarrierId { get; set; }
        /// <summary>
        /// Driver availability end after applying all legal extensions
        /// </summary>
        [JsonIgnore]
        public double MaxDriverAvailabilityEnd
        {
            get
            {
                return Math.Max(
                        AvailabilityEnd - AvailabilityStart,
                        2 * Driver.SingleWorkPeriod + 2 * Driver.DailyBreakPeriod + Driver.AdditionalDailyWorkPeriod)
                    + AvailabilityStart;
            }
        }

        /// <summary>
        /// Additional driving hour (60 min.)
        /// </summary>
        public const int AdditionalDailyWorkPeriod = 3600;
        /// <summary>
        /// Driver's single daily break (45 min.)
        /// </summary>
        public const int DailyBreakPeriod = 2700;
        /// <summary>
        /// Half of basic driver work period (1 x 4.5h)
        /// </summary>
        public const int SingleWorkPeriod = 4 * 3600 + 1800;
        /// <summary>
        /// Standard "night" break of 11 hours
        /// </summary>
        public const int NightBreakPeriod = 11 * 3600;

        /// <summary>
        /// Creates new Driver object
        /// </summary>
        /// <param name="id"></param>
        /// <param name="availabilityStart"></param>
        /// <param name="availabilityEnd"></param>
        /// <param name="compatibileVehiclesIds"></param>
        /// <param name="carrierId"></param>
        public Driver(int id, double availabilityStart, double availabilityEnd, int[] compatibileVehiclesIds, int[] carrierId)
                    : base(id, availabilityStart, availabilityEnd)
        {
            CompatibileVehiclesIds = compatibileVehiclesIds;
            CarrierId = carrierId;
        }
    }
}