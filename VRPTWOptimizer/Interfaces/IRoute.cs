using CommonGIS;
using System.Collections.Generic;

namespace VRPTWOptimizer.Interfaces
{
    /// <summary>
    /// Represents route assigned to Vehicle
    /// </summary>
    public interface IRoute
    {
        /// <summary>
        /// Array of arrival times to subsequent Location objects
        /// </summary>
        List<double> ArrivalTimes { get; }
        /// <summary>
        /// Array of departure times from subsequent Location objects
        /// </summary>
        List<double> DepartureTimes { get; }
        /// <summary>
        /// Distance objects between subsequent location in route
        /// </summary>
        List<Distance> Distances { get; }
        /// <summary>
        /// Total length of route in meters
        /// </summary>
        double Length { get; }
        /// <summary>
        /// TransportRequest objects that are loaded on given location
        /// </summary>
        List<List<TransportRequest>> LoadedRequests { get; }
        /// <summary>
        /// Timespan in seconds of largest delay againts PreferedTimeWindowEnd
        /// </summary>
        double MaxDelay { get; }
        /// <summary>
        /// Array of upper time limits for starting visits in subsequent Location objects
        /// </summary>
        List<double> TimeWindowEnd { get; }
        /// <summary>
        /// Array of lower time limits for starting visits in subsequent Location objects
        /// </summary>
        List<double> TimeWindowStart { get; }
        /// <summary>
        /// Sum of all delays againts PreferedTimeWindowEnd
        /// </summary>
        double TotalDelay { get; }
        /// <summary>
        /// Total duration of drive within route in seconds
        /// </summary>
        double TravelTime { get; }
        /// <summary>
        /// TransportRequest objects that are unloaded on given location
        /// </summary>
        List<List<TransportRequest>> UnloadedRequests { get; }
        /// <summary>
        /// Vehicle with capacity serving the route
        /// </summary>
        Vehicle Vehicle { get; }
        /// <summary>
        /// Tractor serving the route (if necessary)
        /// </summary>
        Driver VehicleDriver { get; }
        /// <summary>
        /// Driver (if the problem assumes drivers management)
        /// </summary>
        Vehicle VehicleTractor { get; }
        /// <summary>
        /// List of subsequent Location objects in route
        /// </summary>
        List<Location> VisitedLocations { get; }
        /// <summary>
        /// Identifier of route
        /// </summary>
        long Id { get; }
    }
}