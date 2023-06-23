using System;

namespace CommonGIS
{
    /// <summary>
    /// Represents point in two dimensional geographical space with WGS'84 assumed as reference system
    /// </summary>
    public class Location : IEquatable<Location>
    {
        /// <summary>
        /// Point identifier
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Latitude in degrees ("+" is North, "-" is South)
        /// </summary>
        public double Lat { get; set; }
        /// <summary>
        /// Longitude in degrees ("+" is East, "-" is West)
        /// </summary>
        public double Lng { get; set; }

        /// <summary>
        /// Describes type of facility in the given Location
        /// </summary>
        public LocationType Type { get; set; }

        /// <summary>
        /// Creates location
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lng"></param>
        /// <param name="lat"></param>
        [Obsolete(message: "Please use constructor accepting 4 parameters - including type")]
        public Location(string id, double lng, double lat)
        {
            this.Id = id;
            this.Lng = lng;
            this.Lat = lat;
            this.Type = LocationType.Unknown;
        }

        /// <summary>
        /// Creates location object representing facility geographical location and type
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="type"></param>
#pragma warning disable CS0618 // Type or member is obsolete
        public Location(string id, double lat, double lng, LocationType type) : this(id, lat, lng)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            Type = type;
        }

        /// <summary>
        /// Verifies if two locations have identical Ids
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Location other)
        {
            return Id.Equals(other.Id);
        }
    }
}