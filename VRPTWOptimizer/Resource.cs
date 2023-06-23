using System;

namespace VRPTWOptimizer
{
    /// <summary>
    /// Generalized time bound resource (driver, machine, vehicle)
    /// </summary>
    public class Resource
    {
        private const string FINITE_AVAILABILITY_ERROR = "Vehicle must have finite availability";
        /// <summary>
        /// Upper bound of Resource time availability (suggestion)
        /// </summary>
        public double AvailabilityEnd { get; protected set; }

        /// <summary>
        /// Lower bound of Resource time availability (strict)
        /// </summary>
        public double AvailabilityStart { get; protected set; }

        /// <summary>
        /// Identifier of the Resource
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        /// Creates generic Resource object
        /// </summary>
        /// <param name="id"></param>
        /// <param name="availabilityStart"></param>
        /// <param name="availabilityEnd"></param>
        public Resource(int id, double availabilityStart, double availabilityEnd)
        {
            AvailabilityEnd = availabilityEnd;
            AvailabilityStart = availabilityStart;
            Id = id;
            if (!double.IsFinite(AvailabilityStart))
                throw new ArgumentException(FINITE_AVAILABILITY_ERROR);
            if (!double.IsFinite(AvailabilityEnd))
                throw new ArgumentException(FINITE_AVAILABILITY_ERROR);
        }
    }
}