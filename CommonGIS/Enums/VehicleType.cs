namespace CommonGIS.Enums
{
    /// <summary>
    /// Enumerator describing available types of vehicle (from a point of view of creating a schedule)
    /// </summary>
    public enum VehicleType
    {
        /// <summary>
        /// Semitrailer (capacity, but no engine) is encoded as 3
        /// </summary>
        SemiTrailer = 3,
        /// <summary>
        /// Straight (integrated) truck is encoded as 2
        /// </summary>
        StraightTruck = 2,
        /// <summary>
        /// Tractor unit (no capacity, just an engine) is encoded as 1
        /// </summary>
        Tractor = 1
    }
}