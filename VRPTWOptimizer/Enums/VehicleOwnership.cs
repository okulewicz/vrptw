using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRPTWOptimizer.Enums
{
    /// <summary>
    /// Type of vehicle ownership
    /// </summary>
    public class VehicleOwnership
    {
        /// <summary>
        /// Owned/exclusively managed by the company
        /// </summary>
        public const int Internal = 1;
        /// <summary>
        /// Hired for the particular tasks
        /// </summary>
        public const int External = 2;
    }
}
