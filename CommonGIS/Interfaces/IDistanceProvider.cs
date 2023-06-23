namespace CommonGIS.Interfaces
{
    /// <summary>
    /// Provides Distance information between 2 Location objects for given VehicleRoadRestrictionProperties profile
    /// </summary>
    public interface IDistanceProvider
    {
        /// <summary>
        /// Gets distance between from Location and to Location
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="vehicleProperties"></param>
        /// <returns></returns>
        Distance GetDistance(Location from, Location to, VehicleRoadRestrictionProperties vehicleProperties);
    }
}