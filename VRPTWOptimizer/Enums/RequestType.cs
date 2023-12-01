namespace VRPTWOptimizer.Enums
{
    /// <summary>
    /// Type of TransportRequest possibly useful to set priorities
    /// </summary>
    public enum RequestType
    {
        /// <summary>
        /// Delivering ordered cargo from warehouse to final location
        /// </summary>
        GoodsDistribution = 1,
        /// <summary>
        /// Delivering reusable containers to warehouse
        /// </summary>
        ContainerRetrieval = 2,
        /// <summary>
        /// Getting goods from external entities (may constitute additional profit for company)
        /// </summary>
        Backhauling = 3,
        /// <summary>
        /// Delivering cargo between non-warehouse locations
        /// </summary>
        Transfer = 4,
        /// <summary>
        /// Driving without cargo
        /// </summary>
        TechnicalDrive = 5,
    }
}