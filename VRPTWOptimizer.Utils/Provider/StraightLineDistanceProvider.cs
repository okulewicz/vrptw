using CommonGIS;
using CommonGIS.Interfaces;
using System;

namespace VRPTWOptimizer.Utils.Provider
{
    public class StraightLineDistanceProvider : IDistanceProvider
    {
        //11.11 m/s = 40 km/h
        private const double ASSUMED_SPEED = 11.11;

        public Distance GetDistance(Location from, Location to, VehicleRoadRestrictionProperties vehicleProperties)
        {
            double R = 6371e3; // metres
            double fi1 = from.Lat * Math.PI / 180; // φ, λ in radians
            double fi2 = to.Lat * Math.PI / 180;
            double deltaFi = (to.Lat - from.Lat) * Math.PI / 180;
            double deltaLambda = (to.Lng - from.Lng) * Math.PI / 180;

            double a = Math.Sin(deltaFi / 2) * Math.Sin(deltaFi / 2) +
                      Math.Cos(fi1) * Math.Cos(fi2) *
                      Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double d = R * c; // in metres
            //2 * d - more realistic upper bound on distance
            return new TimeLengthDistance(from.Id, to.Id, 2 * d, 2 * d / ASSUMED_SPEED, vehicleProperties);
        }
    }
}