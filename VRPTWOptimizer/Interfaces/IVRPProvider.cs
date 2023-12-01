using CommonGIS;
using System;
using System.Collections.Generic;

namespace VRPTWOptimizer.Interfaces
{
    /// <summary>
    /// Provides problem data for IVRPOptimizer
    /// </summary>
    public interface IVRPProvider
    {
        /// <summary>
        /// List of available Driver objects
        /// </summary>
        List<Driver> Drivers { get; set; }
        /// <summary>
        /// Location of the main warehouse or main depot
        /// </summary>
        Location HomeDepot { get; set; }
        /// <summary>
        /// List of TransportRequest objects to be served
        /// </summary>
        List<TransportRequest> Requests { get; set; }
        /// <summary>
        /// List of available Vehicle objects
        /// </summary>
        List<Vehicle> Vehicles { get; set; }
        /// <summary>
        /// Real world timestamp used to turn relative time of the problem to real world time
        /// </summary>
        DateTime ZeroHour { get; set; }

        /// <summary>
        /// Gets the data from underlying source
        /// </summary>
        /// <param name="billingDate"></param>
        /// <param name="homeDepotId"></param>
        void LoadData(DateTime billingDate, string homeDepotId);
    }
}