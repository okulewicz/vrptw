using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonGIS
{
    /// <summary>
    /// Type defining characteristics of Location facility
    /// </summary>
    public enum LocationType
    {
        /// <summary>
        /// Distribution Center/Warehouse
        /// </summary>
        DC = 1,
        /// <summary>
        /// Store
        /// </summary>
        Store = 2,
        /// <summary>
        /// Facility of external entity (not controlled by our company)
        /// </summary>
        External = 3,
        /// <summary>
        /// Location type has not been defined
        /// </summary>
        Unknown = 0
    }
}
