using System;

namespace CommonGIS
{
    /// <summary>
    /// Represents properties of distance between 2 Location objects
    /// </summary>
    public class Distance : IEquatable<Distance>
    {
        private const double EPS_EQUALITY_TOLERANCE = 1e-2;

        /// <summary>
        /// Source identifier
        /// </summary>
        public string FromId { get; set; }

        /// <summary>
        /// Length (assumed to be in meters)
        /// </summary>
        public double Length { get; set; }

        /// <summary>
        /// Vehicle profile for which the distance has been found
        /// </summary>
        public VehicleRoadRestrictionProperties Profile { get; set; }
        /// <summary>
        /// Time (assumed to be in seconds)
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// Destination identifier
        /// </summary>
        public string ToId { get; set; }

        /// <summary>
        /// Creates Distance object
        /// </summary>
        /// <param name="fromId"></param>
        /// <param name="toId"></param>
        /// <param name="length"></param>
        /// <param name="time"></param>
        /// <param name="profile"></param>
        public Distance(string fromId, string toId, double length, double time, VehicleRoadRestrictionProperties profile)
        {
            Length = length;
            Time = time;
            FromId = fromId;
            ToId = toId;
            Profile = profile;
        }

        /// <summary>
        /// Verifies if two distances are identical in length and time
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Distance other)
        {
            return other.FromId == this.FromId &&
                other.ToId == this.ToId &&
                Math.Abs(this.Length - other.Length) < EPS_EQUALITY_TOLERANCE &&
                Math.Abs(this.Time - other.Time) < EPS_EQUALITY_TOLERANCE;
        }
    }
}