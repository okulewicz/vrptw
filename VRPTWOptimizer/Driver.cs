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
        public int[] CompatibileVehiclesIds { get; protected set; }

        /// <summary>
        /// Creates new Driver object
        /// </summary>
        /// <param name="id"></param>
        /// <param name="availabilityStart"></param>
        /// <param name="availabilityEnd"></param>
        /// <param name="compatibileVehiclesIds"></param>
        public Driver(int id, double availabilityStart, double availabilityEnd, int[] compatibileVehiclesIds)
                    : base(id, availabilityStart, availabilityEnd)
        {
            CompatibileVehiclesIds = compatibileVehiclesIds;
        }
    }
}