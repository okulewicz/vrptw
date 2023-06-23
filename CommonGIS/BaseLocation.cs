using Newtonsoft.Json;
using System;

namespace CommonGIS
{
    /// <summary>
    /// Basic implementation of abstract Location class
    /// </summary>
    [Obsolete(message: "Please use Location class instead")]
    public class BaseLocation : Location
    {
        /// <summary>
        /// Creates location object
        /// </summary>
        /// <param name="location"></param>
        public BaseLocation(Location location) : this(location.Id, location.Lng, location.Lat)
        {
        }

        /// <summary>
        /// Creates Location object
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lng"></param>
        /// <param name="lat"></param>
        [JsonConstructor]
        public BaseLocation(string id, double lng, double lat) : base(id, lng, lat)
        {
        }
    }
}