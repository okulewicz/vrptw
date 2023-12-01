using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRPTWOptimizer.Enums
{
    /// <summary>
    /// Dictionary of special properties identified so far
    /// </summary>
    public enum VehicleSpecialProperties
    {
        /// <summary>
        /// Can be used to prevent utilization of rare small vehicles for places where larger vehicle can go
        /// </summary>
        NonSmallVehicle = 1,
        /// <summary>
        /// Specifies necessity of "plandeka"
        /// </summary>
        Tarp = 2,
        /// <summary>
        /// Specifies existence of seal ("plomba")
        /// </summary>
        Seal = 4
    }
}
