using System;

namespace CommonGIS
{
    [Obsolete("Please use Distance class instead")]
    public class TimeLengthDistance : Distance
    {
        [Obsolete("Please use Distance class instead")]
        public TimeLengthDistance(string fromId, string toId, double length, double time, VehicleRoadRestrictionProperties profile) : base(fromId, toId, length, time, profile)
        {
        }
    }
}