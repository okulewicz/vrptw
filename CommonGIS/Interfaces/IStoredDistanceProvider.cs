using System.Collections.Generic;

namespace CommonGIS.Interfaces
{
    /// <summary>
    /// Provides Distance information between 2 Location objects for given VehicleRoadRestrictionProperties profile
    /// </summary>
    public interface IStoredDistanceProvider : IDistanceProvider
    {
        public List<Distance> StoredDistances { get; }
    }
}