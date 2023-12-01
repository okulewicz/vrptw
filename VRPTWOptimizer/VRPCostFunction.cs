using CommonGIS;
using g3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using VRPTWOptimizer.Interfaces;

namespace VRPTWOptimizer
{
    /// <summary>
    /// Class for calculating solution costs
    /// </summary>
    public class VRPCostFunction
    {
        private const double DefaultCostFactor = 1e-6;
        private const double DefaultLateEarlyFactor = 1e-5;
        private const double DefaultLeftCargoFactor = 1e8;
        private const double DefaultRoutesCountFactor = 1e4;
        public const int SingleDriverRideTime = 9 * 3600;
        public const int SingleDriverWorkTime = 11 * 3600;
        public const int SingleDriverRestTime = 11 * 3600;
        private static readonly Dictionary<int, double> DefaultCarrierFactor = new Dictionary<int, double>();
        /// <summary>
        /// Weight of not getting min distance per carrier
        /// </summary>
        public double CarrierMinDistanceFactor { get; set; }
        /// <summary>
        /// Min distance per carrier
        /// </summary>
        public Dictionary<int, double> CarrierMinDistanceThreshold { get; set; }
        /// <summary>
        /// Weight of not getting desired division of distance between carriers
        /// </summary>
        public double CarrierShareFactor { get; set; }
        /// <summary>
        /// Desired distances division between carriers
        /// </summary>
        public Dictionary<int, double> CarrierShareRatio { get; set; }
        /// <summary>
        /// Weight of the distance costs in final route evaluations
        /// </summary>
        public double DistanceFactor { get; set; }
        /// <summary>
        /// Weight of the drive time costs in final route evaluations
        /// </summary>
        public double DriveTimeFactor { get; set; }
        /// <summary>
        /// Weight of sending empty vehicles out of the depot
        /// </summary>
        public double FillInFactor { get; set; }
        /// <summary>
        /// Weight for not delivering a single unit of cargo
        /// </summary>
        public double LeftCargoUnitFactor { get; set; }
        /// <summary>
        /// Weight on distance for not filling vehicle up to LowFillInThreshold
        /// </summary>
        public int LowFillInFactor { get; set; }
        /// <summary>
        /// Level of desired vehicle fill
        /// </summary>
        public double LowFillInThreshold { get; set; }


        /// <summary>
        /// Weight for max delay time en route
        /// </summary>
        public double MaxDelayFactor { get; set; }
        /// <summary>
        /// Weight for squared max delay time en route
        /// </summary>
        public double MaxDelaySquaredFactor { get; set; }
        /// <summary>
        /// Weight for max early arrival en route
        /// </summary>
        public double MaxEarlyArrivalFactor { get; set; }
        /// <summary>
        /// Weight for squared max early arrival en route
        /// </summary>
        public double MaxEarlyArrivalSquaredFactor { get; set; }
        /// <summary>
        /// Weight for max vehicle wait time in depot
        /// </summary>
        public double MaxVehicleSpreadFactor { get; set; }
        /// <summary>
        /// Weight of the number of routes in final evaluation
        /// </summary>
        public double RoutesCountFactor { get; set; }
        /// <summary>
        /// Weight for total delay time en route
        /// </summary>
        public double TotalDelayFactor { get; set; }
        /// <summary>
        /// Weight for squared total delay time en route
        /// </summary>
        public double TotalDelaySquaredFactor { get; set; }
        /// <summary>
        /// Weight for total early arrival en route
        /// </summary>
        public double TotalEarlyArrivalFactor { get; set; }
        /// <summary>
        /// Weight for squared total early arrival en route
        /// </summary>
        public double TotalEarlyArrivalSquaredFactor { get; set; }
        /// <summary>
        /// Weight of the vehicle usage costs in final route evaluations
        /// </summary>
        public double UsageFactor { get; set; }
        /// <summary>
        /// Weight for starting some vehicles at much later hour than others
        /// </summary>
        public double MaxVehicleLateStartFactor { get; set; }
        /// <summary>
        /// Max number of seconds from zero hour  for latest vehicle start before penalty is applied
        /// </summary>
        public double MaxVehicleLateStartThreshold { get; set; }
        /// <summary>
        /// Max number of seconds between vehicle routes before penalty is applied
        /// </summary>
        public double MaxVehicleSpreadThreshold { get; set; }
        /// <summary>
        /// Weight of the resource switching between routes of the same driver or tractor
        /// </summary>
        public double ResourceSwitchingFactor { get; set; }

        /// <summary>
        /// Max number of seconds of early arrival before penalty is applied
        /// </summary>
        public double MaxEarlyArrivalThreshold { get; set; }
        
        /// <summary>
        /// Weight of inclusion of route point in another route's convex hull
        /// </summary>
        public double VisualAttractivenessFactor { get; set; }
        /// <summary>
        /// Weight of route cost in the cost function
        /// </summary>
        public double RouteCostFactor { get; set; }
        /// <summary>
        /// Weight of number of drivers
        /// </summary>
        public double DriversCountFactor { get; set; }
        /// <summary>
        /// Factor added to cost function if route is overloaded at any rate 
        /// </summary>
        public double OverloadedRouteFactor { get; set; }
        /// <summary>
        /// Factor added to cost function if route is overloaded over  ExtensiveRouteOverloadThreshold
        /// </summary>
        public double ExtensivelyOverloadedRouteFactor { get; set; }
        /// <summary>
        /// Ratio of overload considered to be excessive
        /// </summary>
        public double ExtensiveRouteOverloadThreshold { get; set; }
        /// <summary>
        /// Cost of every part of drivers work time imbalance
        /// </summary>
        public double DriversWorkImbalanceFactor { get; set; }

        /// <summary>
        /// Constructor for deserializer
        /// </summary>
        public VRPCostFunction()
        {
            CarrierMinDistanceThreshold = DefaultCarrierFactor;
            CarrierShareRatio = DefaultCarrierFactor;
            DistanceFactor = DefaultCostFactor;
            LeftCargoUnitFactor = DefaultLeftCargoFactor;
            MaxDelaySquaredFactor = DefaultLateEarlyFactor;
            MaxEarlyArrivalFactor = DefaultLateEarlyFactor;
            RoutesCountFactor = DefaultRoutesCountFactor;
            MaxVehicleLateStartThreshold = double.MaxValue;
            MaxEarlyArrivalThreshold = double.MaxValue;
            MaxVehicleSpreadThreshold = double.MaxValue;
        }

        /// <summary>
        /// Creates cost function object with the specified weight for given aspects of problem solution
        /// </summary>
        /// <param name="distanceFactor"></param>
        /// <param name="usageFactor"></param>
        /// <param name="driveTimeFactor"></param>
        /// <param name="leftUnitFactor"></param>
        /// <param name="maxDelaySquaredFactor"></param>
        /// <param name="maxEarlyArrivalFactor"></param>
        /// <param name="totalDelaySquaredFactor"></param>
        /// <param name="totalEarlyArrivalFactor"></param>
        /// <param name="carrierMinDistanceFactor"></param>
        /// <param name="carrierShareFactor"></param>
        /// <param name="fillInFactor"></param>
        /// <param name="carrierMinDistanceThreshold"></param>
        /// <param name="carrierShareRatio"></param>
        /// <param name="routesCountFactor"></param>
        /// <param name="totalEarlyArrivalSquaredFactor"></param>
        /// <param name="totalDelayFactor"></param>
        /// <param name="maxVehicleSpreadFactor"></param>
        /// <param name="maxEarlyArrivalSquaredFactor"></param>
        /// <param name="maxDelayFactor"></param>
        /// <param name="maxVehicleLateStartFactor"></param>
        /// <param name="maxVehicleLateStartThreshold"></param>
        /// <param name="maxEarlyArrivalThreshold"></param>
        /// <param name="maxVehicleSpreadThreshold"></param>
        [JsonConstructor]
        public VRPCostFunction(
            double distanceFactor,
            double usageFactor,
            double driveTimeFactor,
            double leftUnitFactor,
            double maxDelaySquaredFactor,
            double maxEarlyArrivalFactor,
            double totalDelaySquaredFactor,
            double totalEarlyArrivalFactor,
            double carrierMinDistanceFactor,
            double carrierShareFactor,
            double fillInFactor,
            Dictionary<int, double> carrierMinDistanceThreshold,
            Dictionary<int, double> carrierShareRatio,
            double routesCountFactor = 0,
            double totalEarlyArrivalSquaredFactor = 0,
            double totalDelayFactor = 0,
            double maxVehicleSpreadFactor = 0,
            double maxEarlyArrivalSquaredFactor = 0,
            double maxDelayFactor = 0,
            double maxVehicleLateStartFactor = 0,
            double maxVehicleLateStartThreshold = double.MaxValue,
            double maxEarlyArrivalThreshold = double.MaxValue,
            double maxVehicleSpreadThreshold = double.MaxValue,
            double resourceSwitchingCost = 0,
            int lowFillInFactor = 0,
            double lowFillInThreshold = 0.0,
            double visualAttractivenessFactor = 0,
            double routeCostFactor = 0,
            double driversCountFactor = 0,
            double driversWorkImbalanceFactor = 0)
        {
            DistanceFactor = distanceFactor;
            UsageFactor = usageFactor;
            DriveTimeFactor = driveTimeFactor;
            LeftCargoUnitFactor = leftUnitFactor;
            MaxDelaySquaredFactor = maxDelaySquaredFactor;
            MaxEarlyArrivalFactor = maxEarlyArrivalFactor;
            TotalDelaySquaredFactor = totalDelaySquaredFactor;
            TotalEarlyArrivalFactor = totalEarlyArrivalFactor;
            CarrierMinDistanceFactor = carrierMinDistanceFactor;
            CarrierMinDistanceThreshold = carrierMinDistanceThreshold;
            CarrierShareFactor = carrierShareFactor;
            CarrierShareRatio = carrierShareRatio;
            FillInFactor = fillInFactor;
            RoutesCountFactor = routesCountFactor;
            TotalEarlyArrivalSquaredFactor = totalEarlyArrivalSquaredFactor;
            TotalDelayFactor = totalDelayFactor;
            MaxVehicleSpreadFactor = maxVehicleSpreadFactor;
            MaxEarlyArrivalSquaredFactor = maxEarlyArrivalSquaredFactor;
            MaxDelayFactor = maxDelayFactor;
            MaxVehicleLateStartFactor = maxVehicleLateStartFactor;
            MaxVehicleLateStartThreshold = maxVehicleLateStartThreshold;
            MaxEarlyArrivalThreshold = maxEarlyArrivalThreshold;
            MaxVehicleSpreadThreshold = maxVehicleSpreadThreshold;
            ResourceSwitchingFactor = resourceSwitchingCost;
            LowFillInFactor = lowFillInFactor;
            LowFillInThreshold = lowFillInThreshold;
            VisualAttractivenessFactor = visualAttractivenessFactor;
            RouteCostFactor = routeCostFactor;
            DriversCountFactor = driversCountFactor;
            DriversWorkImbalanceFactor = driversWorkImbalanceFactor;
        }

        public VRPCostFunction Clone()
        {
            Dictionary<int, double> carrierMinDistanceThreshold = new();
            foreach (var key in this.CarrierMinDistanceThreshold.Keys)
            {
                carrierMinDistanceThreshold.Add(key, this.CarrierMinDistanceThreshold[key]);
            }
            Dictionary<int, double> carrierShareRatio = new();
            foreach (var key in this.CarrierShareRatio.Keys)
            {
                carrierShareRatio.Add(key, this.CarrierShareRatio[key]);
            }
            return new VRPCostFunction() {
                DistanceFactor = this.DistanceFactor,
                UsageFactor = this.UsageFactor,
                DriveTimeFactor = this.DriveTimeFactor,
                LeftCargoUnitFactor = this.LeftCargoUnitFactor,
                MaxDelaySquaredFactor = this.MaxDelaySquaredFactor,
                MaxEarlyArrivalFactor = this.MaxEarlyArrivalFactor,
                TotalDelaySquaredFactor = this.TotalDelaySquaredFactor,
                TotalEarlyArrivalFactor = this.TotalEarlyArrivalFactor,
                CarrierMinDistanceFactor = this.CarrierMinDistanceFactor,
                CarrierMinDistanceThreshold = carrierMinDistanceThreshold,
                CarrierShareFactor = this.CarrierShareFactor,
                CarrierShareRatio = carrierShareRatio,
                FillInFactor = this.FillInFactor,
                RoutesCountFactor = this.RoutesCountFactor,
                TotalEarlyArrivalSquaredFactor = this.TotalEarlyArrivalSquaredFactor,
                TotalDelayFactor = this.TotalDelayFactor,
                MaxVehicleSpreadFactor = this.MaxVehicleSpreadFactor,
                MaxEarlyArrivalSquaredFactor = this.MaxEarlyArrivalSquaredFactor,
                MaxDelayFactor = this.MaxDelayFactor,
                MaxVehicleLateStartFactor = this.MaxVehicleLateStartFactor,
                MaxVehicleLateStartThreshold = this.MaxVehicleLateStartThreshold,
                MaxEarlyArrivalThreshold = this.MaxEarlyArrivalThreshold,
                MaxVehicleSpreadThreshold = this.MaxVehicleSpreadThreshold,
                ResourceSwitchingFactor = this.ResourceSwitchingFactor,
                LowFillInFactor = this.LowFillInFactor,
                LowFillInThreshold = this.LowFillInThreshold,
                VisualAttractivenessFactor = this.VisualAttractivenessFactor,
                RouteCostFactor = this.RouteCostFactor,
                DriversCountFactor = this.DriversCountFactor
            };
        }

        /// <summary>
        /// Computes fill in factor at route start
        /// </summary>
        /// <param name="cargoOnRouteStart"></param>
        /// <param name="vehicleCapacity"></param>
        /// <returns></returns>
        public static double ComputeFillInFactor(List<double[]> cargoOnRouteStart, double[] vehicleCapacity, Enums.Aggregation[] aggregationType)
        {
            double maxFillIn = 0.0;
            for (int i = 0; i < vehicleCapacity.Length; i++)
            {
                double cargoSumI = 0.0;
                if (aggregationType[i] == Enums.Aggregation.Sum)
                {
                    cargoSumI = cargoOnRouteStart.Sum(c => c[i]);
                }
                else if (aggregationType[i] == Enums.Aggregation.Max)
                {
                    if (cargoOnRouteStart.Any())
                    {
                        cargoSumI = cargoOnRouteStart.Max(c => c[i]);
                    }
                }
                maxFillIn = Math.Max(maxFillIn, cargoSumI / vehicleCapacity[i]);
            }
            return maxFillIn;
        }

        /// <summary>
        /// Computes how many drivers where necessary to provide all routes
        /// </summary>
        /// <param name="routes"></param>
        /// <returns></returns>
        public static int ComputeDriversCount(List<IRoute> routes)
        {
            int straightForwardDrivers = routes
                .Where(rt => rt.VehicleDriver != null)
                .Select(rt => rt.VehicleDriver.Id)
                .Distinct()
                .Count();
            int driversCount = straightForwardDrivers;
            var sameTractorRoutes = routes
                .Where(rt => rt.VehicleDriver == null && rt.VehicleTractor != null)
                .GroupBy(rt => rt.VehicleTractor.Id);
            foreach (var sameTractor in sameTractorRoutes)
            {
                double rideTimeSoFar = 0.0;
                var sameTractorOrdered = sameTractor.OrderBy(rt => rt.ArrivalTimes[0]);
                double workStart = sameTractorOrdered.First().DepartureTimes[0];
                double workEnd = sameTractorOrdered.First().DepartureTimes[^1];
                driversCount += 1;
                List<double> driversWorkingHoursEnds = new List<double>();
                foreach (var tractorRoute in sameTractorOrdered)
                {
                    var tractorRouteDrive = tractorRoute.Distances.Sum(d => d.Time);
                    var tractorRouteEnd = tractorRoute.DepartureTimes[^1];
                    if (rideTimeSoFar + tractorRouteDrive <= SingleDriverRideTime && (tractorRouteEnd - workStart) <= SingleDriverWorkTime)
                    {
                        rideTimeSoFar += tractorRouteDrive;
                        workEnd = tractorRouteEnd;
                    }
                    else
                    {
                        workStart = tractorRoute.DepartureTimes[0];
                        rideTimeSoFar = tractorRouteDrive;
                        driversCount += 1;
                        driversWorkingHoursEnds.Add(workEnd);
                        workEnd = tractorRoute.DepartureTimes[^1];
                        if (driversWorkingHoursEnds.Any() && (workStart - driversWorkingHoursEnds[0]) >= SingleDriverRestTime)
                        {
                            //first driver is ready to go again
                            driversCount -= 1;
                            driversWorkingHoursEnds.RemoveAt(0);
                        }
                    }
                }
            }
            var sameTruckRoutes = routes
                .Where(rt => rt.VehicleDriver == null && rt.VehicleTractor == null)
                .GroupBy(rt => rt.Vehicle.Id);
            foreach (var sameTruck in sameTruckRoutes)
            {
                double rideTimeSoFar = 0.0;
                var sameTruckOrdered = sameTruck.OrderBy(rt => rt.ArrivalTimes[0]);
                double workStart = sameTruckOrdered.First().ArrivalTimes[0];
                double workEnd = sameTruckOrdered.First().DepartureTimes[^1];
                driversCount += 1;
                List<double> driversWorkingHoursEnds = new List<double>();
                foreach (var truckRoute in sameTruckOrdered)
                {
                    var truckRouteDrive = truckRoute.Distances.Sum(d => d.Time);
                    var truckRouteEnd = truckRoute.DepartureTimes[^1];
                    if (rideTimeSoFar + truckRouteDrive <= 9 * 3600 && (truckRouteEnd - workStart) <= 11)
                    {
                        rideTimeSoFar += truckRouteDrive;
                        workEnd = truckRouteEnd;
                    }
                    else
                    {
                        workStart = truckRoute.ArrivalTimes[0];
                        rideTimeSoFar = truckRouteDrive;
                        driversCount += 1;
                        driversWorkingHoursEnds.Add(workEnd);
                        workEnd = truckRoute.DepartureTimes[^1];
                        if (driversWorkingHoursEnds.Any() && (workStart - driversWorkingHoursEnds[0]) < 11 * 3600)
                        {
                            //first driver is ready to go again
                            driversCount -= 1;
                        }
                    }
                }
            }
            return driversCount;
        }

        /// <summary>
        /// Computes fill in factor for given IRoute
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public static double ComputeFillInFactor(IRoute route)
        {
            List<double[]> cargoOnRouteStart = route.LoadedRequests[0].Select(rq => rq.Size).ToList();
            double[] vehicleCapacity = route.Vehicle.Capacity;
            var aggregationType = route.Vehicle.CapacityAggregationType;
            return ComputeFillInFactor(cargoOnRouteStart, vehicleCapacity, aggregationType);
        }

        /// <summary>
        /// Computes earliest arrival before preferred time window start
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public static double ComputeMaxEarlyArrival(IRoute route)
        {
            double[] timeWindowsStart = route.TimeWindowStart
                .Select(tws => tws).ToArray();
            double[] arrivalTimes = route.ArrivalTimes
                .Select(art => art).ToArray();
            return ComputeMaxTimeDiff(timeWindowsStart, arrivalTimes, true);
        }

        /// <summary>
        /// Computes largest delay taking into account only pickups and deliveries not resource limits
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public static double ComputeMaxServiceDelay(IRoute route)
        {
            double delay = 0.0;
            double[] arrivalTimes = route.ArrivalTimes
                .Select(art => art).ToArray();
            double[] timeWindowsEnd = new double[arrivalTimes.Length];
            for (int i = 0; i < route.VisitedLocations.Count; i++)
            {
                timeWindowsEnd[i] = double.MaxValue;
                if (route.LoadedRequests[i].Any())
                {
                    timeWindowsEnd[i] = Math.Min(timeWindowsEnd[i], route.LoadedRequests[i].Min(rq => rq.PickupPreferedTimeWindowEnd));
                }
                if (route.UnloadedRequests[i].Any())
                {
                    timeWindowsEnd[i] = Math.Min(timeWindowsEnd[i], route.UnloadedRequests[i].Min(rq => rq.DeliveryPreferedTimeWindowEnd));
                }
            }
            return ComputeMaxTimeDiff(timeWindowsEnd, arrivalTimes, false);
        }

        /// <summary>
        /// Computes max unwanted time difference
        /// </summary>
        /// <param name="referenceValues">Bound values</param>
        /// <param name="trueValues">Real values</param>
        /// <param name="referenceIsLowerBound">Is reference a lower bound for true value?</param>
        /// <returns></returns>
        public static double ComputeMaxTimeDiff(double[] referenceValues, double[] trueValues, bool referenceIsLowerBound)
        {
            double timeDiff = 0.0;
            for (int i = 0; i < referenceValues.Length; i++)
            {
                if (referenceIsLowerBound)
                {
                    timeDiff = Math.Max(timeDiff, referenceValues[i] - trueValues[i]);
                }
                else
                {
                    timeDiff = Math.Max(timeDiff, trueValues[i] - referenceValues[i]);
                }
            }
            return timeDiff;
        }

        public static double ComputeMaxVehicleSpread(List<IRoute> routes)
        {
            var orderedRoutes = routes.OrderBy(rt => rt.VehicleTractor != null ? rt.VehicleTractor.Id : rt.Vehicle.Id)
                .ThenBy(rt => rt.ArrivalTimes[0]);
            IRoute previousRoute = null;
            double maxVehicleSpread = 0.0;
            foreach (var currentRoute in orderedRoutes)
            {
                if (previousRoute != null)
                {
                    if (currentRoute.VehicleTractor != null)
                    {
                        if (previousRoute.VehicleTractor != null && currentRoute.VehicleTractor.Id == previousRoute.VehicleTractor.Id)
                        {
                            maxVehicleSpread = Math.Max(maxVehicleSpread, currentRoute.DepartureTimes[0] - previousRoute.ArrivalTimes[^1]);
                        }
                    }
                    else if (currentRoute.Vehicle.Id == previousRoute.Vehicle.Id)
                    {
                        maxVehicleSpread = Math.Max(maxVehicleSpread, currentRoute.ArrivalTimes[0] - previousRoute.DepartureTimes[^1]);
                    }
                }
                previousRoute = currentRoute;
            }

            return maxVehicleSpread;
        }

        /// <summary>
        /// Computes sum of early arrivals within the route
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public static double ComputeTotalEarlyArrival(IRoute route)
        {
            double[] timeWindowsStart = route.TimeWindowStart
                .Select(tws => tws).ToArray();
            double[] arrivalTimes = route.ArrivalTimes
                .Select(art => art).ToArray();
            return ComputeTotalTimeDiff(timeWindowsStart, arrivalTimes, true);
        }

        /// <summary>
        /// Computes total unwanted time difference
        /// </summary>
        /// <param name="referenceValues">Bound values</param>
        /// <param name="trueValues">Real values</param>
        /// <param name="referenceIsLowerBound">Is reference a lower bound for true value?</param>
        /// <returns></returns>
        public static double ComputeTotalTimeDiff(double[] referenceValues, double[] trueValues, bool referenceIsLowerBound)
        {
            double timeDiff = 0.0;
            for (int i = 0; i < referenceValues.Length; i++)
            {
                if (referenceIsLowerBound)
                {
                    timeDiff += Math.Max(0, referenceValues[i] - trueValues[i]);
                }
                else
                {
                    timeDiff += Math.Max(0, trueValues[i] - referenceValues[i]);
                }
            }
            return timeDiff;
        }

        /// <summary>
        /// Creates cost function with parameters prioritizing in following order
        /// * not leaving any cargo
        /// * small number of routes
        /// * balanced sum of distance, delays and early arrivals
        /// </summary>
        /// <returns></returns>
        public static VRPCostFunction GetDefaultParametersFunction()
        {
            return new(
                carrierMinDistanceFactor: 0.0,
                carrierMinDistanceThreshold: DefaultCarrierFactor,
                carrierShareFactor: 0.0,
                carrierShareRatio: DefaultCarrierFactor,
                distanceFactor: DefaultCostFactor,
                driveTimeFactor: DefaultCostFactor,
                fillInFactor: 0.0,
                leftUnitFactor: DefaultLeftCargoFactor,
                maxDelaySquaredFactor: DefaultLateEarlyFactor,
                maxEarlyArrivalFactor: DefaultLateEarlyFactor,
                totalDelaySquaredFactor: 0.0,
                totalEarlyArrivalFactor: 0.0,
                routesCountFactor: DefaultRoutesCountFactor,
                usageFactor: DefaultCostFactor
                );
        }

        /// <summary>
        /// Evaluate cost of a single route
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public double ComputeSingleRouteValue(IRoute route)
        {
            double length = route.Length;
            double travelTime = route.TravelTime;
            Vehicle vehicle = route.Vehicle;
            Vehicle vehicleTractor = route.VehicleTractor;
            IEnumerable<double> revenueValues = route.LoadedRequests.SelectMany(rq => rq.Select(rq => rq.RevenueValue));
            double[] timeWindowsStart = route.TimeWindowStart
                .Select(tws => tws).ToArray();
            double[] arrivalTimes = route.ArrivalTimes
                .Select(art => art).ToArray();
            double vehicleDistanceCost = vehicle.ComputeDistanceCost(length);
            double vehicleTimeCost = vehicle.ComputeTimeCost(travelTime);
            double vehicleRouteCost = RouteCostFactor * route.Vehicle.VehicleCostPerRoute;
            if (vehicleTractor != null)
            {
                vehicleDistanceCost += route.VehicleTractor.ComputeDistanceCost(length);
                vehicleTimeCost += route.VehicleTractor.ComputeTimeCost(travelTime);
                vehicleRouteCost += RouteCostFactor * route.VehicleTractor.VehicleCostPerRoute;
            }
            //cost of being too early
            double routeTotalEarly = ComputeTotalTimeDiff(timeWindowsStart, arrivalTimes, true);
            double earlyArrivalsCost = TotalEarlyArrivalFactor * routeTotalEarly + TotalEarlyArrivalSquaredFactor * routeTotalEarly * routeTotalEarly;
            //cost of delay
            double routeTotalDelay = route.TotalDelay;
            double lateArrivalsCost = TotalDelayFactor * routeTotalDelay + TotalDelaySquaredFactor * routeTotalDelay * routeTotalDelay;
            double fillInFactor = ComputeFillInFactor(route);
            return
                DistanceFactor * vehicleDistanceCost * (1 + LowFillInFactor * Math.Max(0, LowFillInThreshold - fillInFactor)) +
                //cost of cargo cooling
                DriveTimeFactor * vehicleTimeCost +
                //cost of using vehicle at all
                lateArrivalsCost +
                earlyArrivalsCost -
                //gain of serving profit generating requests
                revenueValues.Sum(val => val);
            ;
        }

        /// <summary>
        /// Evaluate cost of all routes
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="leftRequests"></param>
        /// <returns></returns>
        public double Value(List<IRoute> routes, List<TransportRequest> leftRequests)
        {
            /*
             *             double vehicleUsageCost = route.Vehicle.VehicleCostPerUsage;
            vehicleUsageCost += route.VehicleTractor.VehicleCostPerUsage;

             */

            double pointsCountInConvexHulls = CountIntersectingConvexHulls(routes);

            double maxVehicleSpread = ComputeMaxVehicleSpread(routes);
            double maxVehicleSpreadCost = MaxVehicleSpreadFactor * maxVehicleSpread;
            double maxVehicleLateStart = ComputeMaxLateVehicleStart(routes);
            double maxVehicleLateStartCost = maxVehicleLateStart * MaxVehicleLateStartFactor;
            double maxEarlyArrival = routes.Any() ? routes.Max(route => ComputeMaxEarlyArrival(route)) : 0.0;

            var realDistanceDict = routes
                .GroupBy(rt => rt.Vehicle.OwnerID)
                .ToDictionary(gr => gr.Key, gr => gr.Sum(rt => rt.Length));
            var totalDistance = routes.Sum(rt => rt.Length);
            var lowFillInCost = routes.Any() ? FillInFactor * routes.Average(route => (1 - ComputeFillInFactor(route))) : 0.0;
            var maxEarlyArrivalCost = routes.Any() ? routes.Max(route =>
            {
                double routeMaxEarlyArrival = ComputeMaxEarlyArrival(route);
                return MaxEarlyArrivalFactor * routeMaxEarlyArrival + MaxEarlyArrivalSquaredFactor * routeMaxEarlyArrival * routeMaxEarlyArrival;
            }) : 0.0;
            var maxLaterArrivalCost = routes.Any() ? routes.Max(route =>
            {
                double maxDelay = route.MaxDelay;
                return MaxDelayFactor * maxDelay + MaxDelaySquaredFactor * maxDelay * maxDelay;
            }) : 0.0;
            var routesCost = routes.Sum(
                            //cost of routes
                            rt => ComputeSingleRouteValue(rt));
            routesCost += UsageFactor * routes.Select(rt => rt.Vehicle).Distinct().Sum(v => v.VehicleCostPerUsage);
            routesCost += UsageFactor * routes.Where(rt => rt.VehicleTractor != null).Select(rt => rt.VehicleTractor).Distinct().Sum(v => v.VehicleCostPerUsage);
            //cost of not delivering certain amount of cargo
            var leftCargoCost = LeftCargoUnitFactor * leftRequests.Sum(lr => lr.PackageCount);
            var carrierCost =  //cost of not getting equal routes distance share between car owners
                routes.Any() ? CarrierShareFactor * CarrierShareRatio.Keys.Sum(csrDictKey =>
                    Math.Abs(totalDistance * CarrierShareRatio[csrDictKey] - realDistanceDict[csrDictKey]) / 2.0) +
                //cost of not providing enough kilometers per car owner
                CarrierMinDistanceFactor * CarrierMinDistanceThreshold.Keys.Sum(cmdtDictKey =>
                    Math.Abs(Math.Max(0, CarrierMinDistanceThreshold[cmdtDictKey] - realDistanceDict[cmdtDictKey]))) : 0.0;
            double routeCountCost = RoutesCountFactor * routes.Count;
            double resourceSwitchingCost = ComputeResouceSwitchingCost(routes);

            double totalResultMultiplier = 1 *
                (maxVehicleLateStart > MaxVehicleLateStartThreshold ? 2.0 : 1.0) *
                (maxVehicleSpread > MaxVehicleSpreadThreshold ? 2.0 : 1.0) *
                (maxEarlyArrival > MaxEarlyArrivalThreshold ? 2.0 : 1.0);

            int driversCount = ComputeDriversCount(routes);

            return (
                VisualAttractivenessFactor * pointsCountInConvexHulls +
                DriversCountFactor * driversCount +
                routesCost +
                lowFillInCost +
                routeCountCost +
                resourceSwitchingCost +
                maxVehicleSpreadCost +
                maxVehicleLateStartCost +
                maxEarlyArrivalCost +
                maxLaterArrivalCost +
                carrierCost +
                leftCargoCost) * totalResultMultiplier;
        }

        private int CountIntersectingConvexHulls(List<IRoute> routes)
        {
            int countPointsInForeignHulls = 0;
            List<Polygon2d> routePolygons = new List<Polygon2d>();
            List<List<Vector2d>> routePointsSet = new List<List<Vector2d>>();
            foreach (var route in routes)
            {
                List<Vector2d> vectors = route.VisitedLocations.Skip(1).Select(vl => new Vector2d(vl.Lat, vl.Lng)).ToList();
                ConvexHull2 convexHull2 = new ConvexHull2(vectors, 1e-8, QueryNumberType.QT_DOUBLE);
                routePolygons.Add(convexHull2.GetHullPolygon());
                routePointsSet.Add(vectors.SkipLast(1).ToList());
            }
            for (int i = 0; i < routePolygons.Count; i++)
            {
                for (int j = 0; j < routePointsSet.Count; j++)
                {
                    if (i != j && routePolygons[i] != null)
                    {
                        countPointsInForeignHulls += routePointsSet[j].Count(pt => routePolygons[i].Contains(pt));
                    }
                }

            }
            return countPointsInForeignHulls;
        }

        public static int FairlyCountIntersectingConvexHulls(List<IRoute> routes)
        {
            int countPointsInForeignHulls = 0;
            List<Polygon2d> routePolygons = new List<Polygon2d>();
            List<Vehicle> vehicleFilters = new List<Vehicle>();
            List<List<Vector2d>> routePointsSet = new List<List<Vector2d>>();
            List<List<List<TransportRequest>>> roadRestrictionPropertiesFilters = new List<List<List<TransportRequest>>>();
            foreach (var route in routes)
            {
                List<Vector2d> vectors = route.VisitedLocations.Skip(1).Select(vl => new Vector2d(vl.Lat, vl.Lng)).ToList();
                ConvexHull2 convexHull2 = new ConvexHull2(vectors, 1e-8, QueryNumberType.QT_DOUBLE);
                routePolygons.Add(convexHull2.GetHullPolygon());
                vehicleFilters.Add(route.Vehicle);
                routePointsSet.Add(vectors.SkipLast(1).ToList());
                List<List<TransportRequest>> unifiedLoadedAndUnloaded = new List<List<TransportRequest>>();
                for (int i = 1; i < route.UnloadedRequests.Count - 1; i++)
                {
                    unifiedLoadedAndUnloaded.Add(new List<TransportRequest>());
                    unifiedLoadedAndUnloaded[^1].AddRange(route.UnloadedRequests[i]);
                    unifiedLoadedAndUnloaded[^1].AddRange(route.LoadedRequests[i]);
                }
                roadRestrictionPropertiesFilters.Add(unifiedLoadedAndUnloaded);
            }
            for (int j = 0; j < routePointsSet.Count; j++)
            {
                bool isContained = false;
                for (int i = 0; i < routePolygons.Count; i++)
                { 
                    if (i != j && routePolygons[i] != null)
                    {
                        for (int visitIdx = 0; visitIdx < roadRestrictionPropertiesFilters[j].Count; visitIdx++)
                        {
                            //licz tylko te punkty, które mogłyby być obsłużone przez pojazd
                            if (!roadRestrictionPropertiesFilters[j][visitIdx].Any(rq => !vehicleFilters[i].CanHandleRequest(rq))
                                && routePolygons[i].Contains(routePointsSet[j][visitIdx]))
                            {
                                isContained = true;
                                break;
                            }
                        }
                    }
                    if (isContained)
                    {
                        break;
                    }
                }
                if (isContained)
                {
                    countPointsInForeignHulls += 1;
                }

            }
            return countPointsInForeignHulls;
        }

        private double ComputeResouceSwitchingCost(List<IRoute> routes)
        {
            int driverSwitches = routes.Where(rt => rt.VehicleDriver != null)
                .GroupBy(rt => rt.VehicleDriver)
                .Sum(rtg => rtg.Select(rt2 => rt2.Vehicle.Id).Distinct().Count() - 1);
            int trailerSwitches = routes.Where(rt => rt.VehicleTractor != null)
                .GroupBy(rt => rt.VehicleTractor)
                .Sum(rtg => rtg.Select(rt2 => rt2.Vehicle.Id).Distinct().Count() - 1);
            return ResourceSwitchingFactor * (driverSwitches + trailerSwitches);
        }

        public static double ComputeMaxLateVehicleStart(List<IRoute> newRoutes)
        {
            var routesGroup = newRoutes.GroupBy(rt => rt.VehicleTractor != null ? rt.VehicleTractor.Id : rt.Vehicle.Id);
            return routesGroup.Any() ? routesGroup.Max(rtg => rtg.Min(rt => rt.ArrivalTimes[0])) : 0.0;
        }

        public static double ComputeImportantDelays(IRoute route)
        {
            double delay = 0.0;
            double[] arrivalTimes = route.ArrivalTimes
                .Select(art => art).ToArray();
            double[] timeWindowsEnd = new double[arrivalTimes.Length];
            for (int i = 0; i < route.VisitedLocations.Count; i++)
            {
                timeWindowsEnd[i] = double.MaxValue;
                if (route.LoadedRequests[i].Any())
                {
                    timeWindowsEnd[i] = Math.Min(timeWindowsEnd[i], route.LoadedRequests[i].Min(rq => rq.PickupAvailableTimeWindowEnd));
                }
                if (route.UnloadedRequests[i].Any())
                {
                    timeWindowsEnd[i] = Math.Min(timeWindowsEnd[i], route.UnloadedRequests[i].Min(rq => rq.DeliveryAvailableTimeWindowEnd));
                }
                delay += Math.Max(0, arrivalTimes[i] - timeWindowsEnd[i]);
            }
            return delay;
        }
    }
}