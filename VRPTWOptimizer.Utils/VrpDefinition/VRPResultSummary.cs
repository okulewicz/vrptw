using CommonGIS.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRPTWOptimizer.Interfaces;
using VRPTWOptimizer.Utils.Model;
using static VRPTWOptimizer.Utils.Model.ViaTmsJSONDTOs;

namespace VRPTWOptimizer.Utils.VrpDefinition
{
    public class VRPResultSummary
    {

        public static Dictionary<string, List<string>> PrepareMultipleSummaries(VRPDefinitionJSONDTO dtoWithSolution, IDistanceProvider distanceProvider)
        {
            List<TransportRequest> transportRequests = dtoWithSolution.Requests.Select(r => r as TransportRequest).ToList();
            List<Vehicle> vehicles = dtoWithSolution.Vehicles.Select(v => v as Vehicle).ToList();
            List<Driver> drivers = (dtoWithSolution.Drivers ?? new List<DriverDTO>()).Select(d => d as Driver).ToList();
            if (dtoWithSolution.Solutions?.Count > 0)
            {
                List<IRoute> routes = dtoWithSolution.Solutions[0].Transports.Select(t => new RouteDto(t, transportRequests, vehicles, drivers, distanceProvider) as IRoute).ToList();
                List<TransportRequest> leftRequests = dtoWithSolution.Solutions[0].LeftRequestsIds.Select(id => transportRequests.FirstOrDefault(tr => tr.Id == id)).ToList();
                return PrepareMultipleSummaries(routes, leftRequests, transportRequests, vehicles, drivers, dtoWithSolution.DepotId, dtoWithSolution.ZeroHour);
            }
            else
            {
                Dictionary<string, List<string>> summaries = new();
                summaries.Add("requests-summary", RequestsSummary(transportRequests, dtoWithSolution.DepotId, dtoWithSolution.ZeroHour));
                return summaries;
            }
        }

        public static Dictionary<string, List<string>> PrepareMultipleSummaries(List<IRoute> routes, List<TransportRequest> leftRequests, List<TransportRequest> requests, List<Vehicle> vehicles, List<Driver> drivers, string HomeDepotId, DateTime zeroHour)
        {
            Dictionary<string, List<string>> summaries = new();
            summaries.Add("drivers-summary", DriversRoutesSummary(routes, zeroHour));
            summaries.Add("normalized-routes-summary", NormalizedRoutesSummary(routes, zeroHour));
            summaries.Add("routes-summary", RoutesSummary(routes, zeroHour));
            summaries.Add("routes-starts-aggregated", RoutesStarts(routes, zeroHour));
            summaries.Add("visits-summary", VisitsSummary(routes, zeroHour));
            summaries.Add("requests-summary", RequestsSummary(requests, HomeDepotId, zeroHour));
            summaries.Add("vehicles-summary", VehiclesSummary(routes, vehicles, zeroHour));
            return summaries;
        }

        private static List<string> RoutesStarts(List<IRoute> routes, DateTime zeroHour)
        {
            List<string> result = new List<string>();
            string title = "Time;LeavingRoutesCount;LeavingCargoUnits";
            result.Add(title);
            var groupedRoutes = routes
                .OrderBy(rt => rt.ArrivalTimes[0])
                .GroupBy(rt =>
            {
                var loadingStart = zeroHour.AddSeconds(Math.Round(rt.ArrivalTimes[0]));
                loadingStart = loadingStart.AddMinutes(-loadingStart.Minute);
                loadingStart = loadingStart.AddSeconds(-loadingStart.Second);
                return loadingStart.ToString("yyyy-MM-dd HH:mm");
            });
            foreach (var groupedRoute in groupedRoutes)
            {
                result.Add($"{groupedRoute.Key};{groupedRoute.Count()};{groupedRoute.Sum(rt => rt.LoadedRequests[0].Sum(rq => rq.PackageCount))}");
            }
            return result;
        }

        public static List<string> DriversRoutesSummary(List<IRoute> routes, DateTime zeroHour)
        {
            List<string> result = new List<string>();
            string title = "Tractor/VehicleId;DriverId;DriverStart;DriverEnd;TravelTime;TotalTime;BreakBetweenRoutes;Routes";
            result.Add(title);
            int routeId = 1;
            int driverId = 0;
            int previousVehicleId = -1;
            double driverStart = 0;
            double driverEnd = 0;
            double driveTime = 0;
            List<double> driversStarts = new List<double>();
            List<double> driversEnds = new List<double>();
            List<int> driversIds = new List<int>();
            int driverRouteCount = 0;
            double driverRide = 0;
            double breakTime = 0;
            IOrderedEnumerable<IRoute> noDriverRoutes = routes
                            .Where(rt => rt.VehicleDriver == null)
                            .OrderBy(rt => rt.VehicleTractor == null ? rt.Vehicle.Id : rt.VehicleTractor.Id)
                            .ThenBy(rt => rt.ArrivalTimes[0]);
            if (noDriverRoutes.Any())
            {
                foreach (var route in noDriverRoutes)
                {
                    int currentVehicleId = route.VehicleTractor == null ? route.Vehicle.Id : route.VehicleTractor.Id;
                    double routeDriveTime = route.Distances.Sum(d => d.Time);
                    double routeEndTime = route.DepartureTimes[^1];
                    if (currentVehicleId != previousVehicleId)
                    {
                        if (previousVehicleId != -1)
                        {
                            AddDriverLog(zeroHour, result, driverId, driverStart, driverEnd, driverRouteCount, driverRide, breakTime, previousVehicleId);
                        }
                        driverId = Math.Max(driversIds.Any() ? driversIds.Max() : 0, driverId) + 1;
                        driversEnds.Clear();
                        driversIds.Clear();
                        breakTime = 0;
                        driverStart = route.VehicleTractor == null ? route.ArrivalTimes[0] : route.DepartureTimes[0];
                        driverRouteCount = 0;
                        driverRide = routeDriveTime;
                        driversStarts.Add(driverStart);
                        driverEnd = route.DepartureTimes[^1];
                        driveTime = routeDriveTime;
                    }
                    else
                    {
                        if (driveTime + routeDriveTime > VRPCostFunction.SingleDriverRideTime || routeEndTime - driverStart > VRPCostFunction.SingleDriverWorkTime)
                        {
                            AddDriverLog(zeroHour, result, driverId, driverStart, driverEnd, driverRouteCount, driverRide, breakTime, previousVehicleId);
                            driversEnds.Add(driverEnd);
                            driversIds.Add(driverId);
                            driverId += 1;
                            driverStart = route.VehicleTractor == null ? route.ArrivalTimes[0] : route.DepartureTimes[0];
                            driverRouteCount = 0;
                            driverRide = routeDriveTime;
                            driveTime = routeDriveTime;
                            breakTime = 0;
                            if (driversEnds.Any() && driverStart - driversEnds[0] > VRPCostFunction.SingleDriverRestTime && driverStart - driversStarts[0] >= 24 * 3600)
                            {
                                driverId = driversIds[0];
                                driversIds.RemoveAt(0);
                                driversStarts.RemoveAt(0);
                                driversEnds.RemoveAt(0);
                            }
                        }
                        else
                        {
                            breakTime = Math.Max(route.ArrivalTimes[0] - driverEnd, breakTime);
                            driverRide += routeDriveTime;
                        }
                        driverEnd = route.DepartureTimes[^1];
                        driveTime += routeDriveTime;
                    }
                    previousVehicleId = currentVehicleId;
                    routeId++;
                    driverRouteCount++;
                }
                AddDriverLog(zeroHour, result, driverId, driverStart, driverEnd, driverRouteCount, driverRide, breakTime, previousVehicleId);
            }
            var driverRoutes = routes
                            .Where(rt => rt.VehicleDriver != null)
                            .GroupBy(rt => rt.VehicleDriver.Id);
            foreach (var droutes in driverRoutes)
            {
                var orderedRoutes = droutes.OrderBy(rt => rt.ArrivalTimes[0]).ToList();
                var relevantVehicle = orderedRoutes[0].VehicleTractor ?? orderedRoutes[0].Vehicle;
                breakTime = 0;
                for (int i = 0; i < orderedRoutes.Count - 1; i++)
                {
                    breakTime = Math.Max(orderedRoutes[i + 1].ArrivalTimes[^1] - orderedRoutes[i].DepartureTimes[^1], breakTime);
                }
                AddDriverLog(zeroHour,
                             result,
                             droutes.Key,
                             orderedRoutes[0].ArrivalTimes[0],
                             orderedRoutes[^1].DepartureTimes[^1],
                             orderedRoutes.Count(),
                             orderedRoutes.Sum(rt => rt.Distances.Sum(d => d.Time)),
                             breakTime,
                             relevantVehicle.Id);
            }
            return result;
        }

        private static void AddDriverLog(DateTime zeroHour, List<string> result, int driverId, double driverStart, double driverEnd, int driverRouteCount, double driverRide, double breakTime, int vehicleId)
        {
            var routeDescription = $"{vehicleId};";
            routeDescription += $"{driverId};";
            routeDescription += $"{zeroHour.AddSeconds(Math.Round(driverStart))};";
            routeDescription += $"{zeroHour.AddSeconds(Math.Round(driverEnd))};";
            routeDescription += $"{TimeSpan.FromSeconds(Math.Round(driverRide))};";
            routeDescription += $"{TimeSpan.FromSeconds(Math.Round(driverEnd - driverStart))};";
            routeDescription += $"{TimeSpan.FromSeconds(Math.Round(breakTime))};";
            routeDescription += $"{driverRouteCount};";
            result.Add(routeDescription);
        }

        public static List<string> NormalizedRoutesSummary(List<IRoute> routes, DateTime zeroHour)
        {
            List<string> result = new List<string>();
            string title = "Lp;Oddzial;Laczenie;Ilosc palet;Odbior;Wyjazd z DC;Przyjazd na sklep;Powrot do DC;Id pojazdu;Ladownosc;Kierowca;Czas jazdy";
            result.Add(title);
            int routeId = 1;
            List<double> driversStarts = new List<double>();
            List<double> driversEnds = new List<double>();
            List<int> driversIds = new List<int>();
            foreach (var route in routes
                .OrderBy(rt => rt.ArrivalTimes[0]))
            {
                int currentVehicleId = route.VehicleTractor == null ? route.Vehicle.Id : route.VehicleTractor.Id;
                for (int i = 1; i < route.VisitedLocations.Count - 1; i++)
                {
                    //string title = "Lp;Oddział;Łączenie;Ilość palet;Odbiór;Wyjazd z DC;Przyjazd na sklep;Powrót do DC;Id pojazdu;Ładowność";
                    string routeDescription = $"{routeId};";
                    routeDescription += $"{route.VisitedLocations[i].Id};";
                    var connectionStr = (route.VisitedLocations.Count > 3) ? route.VisitedLocations[1].Id.ToString() : "";
                    routeDescription += $"{connectionStr};";
                    routeDescription += $"{route.UnloadedRequests[i].Sum(rq => rq.Size[0])};";
                    routeDescription += $"{route.LoadedRequests[i].Sum(rq => rq.Size[0])};";
                    routeDescription += $"{TimeSpan.FromSeconds(Math.Round(zeroHour.AddSeconds(route.DepartureTimes[0]).TimeOfDay.TotalSeconds / 900) * 900)};";
                    routeDescription += $"{TimeSpan.FromSeconds(Math.Round(zeroHour.AddSeconds(route.ArrivalTimes[i]).TimeOfDay.TotalSeconds / 900) * 900)};";
                    routeDescription += $"{TimeSpan.FromSeconds(Math.Round(zeroHour.AddSeconds(route.ArrivalTimes[^1]).TimeOfDay.TotalSeconds / 900) * 900)};";
                    routeDescription += $"{currentVehicleId};";
                    routeDescription += $"{route.Vehicle.Capacity[0]};";
                    routeDescription += $"{route.VehicleDriver?.Id};";
                    routeDescription += $"{TimeSpan.FromSeconds(Math.Round(route.Distances.Sum(d => d.Time)))};";
                    result.Add(routeDescription);
                }
                routeId++;
            }
            return result;
        }

        public static List<string> RoutesSummary(List<IRoute> routes, DateTime zeroHour)
        {
            List<string> result = new List<string>();
            string title = "RouteId;TractorId;VehicleId;DriverId;RouteStart;RouteEnd;LocationId;TimeWindowStart;ArrivalTime;TravelVsMap";
            result.Add(title);
            int routeId = 1;
            int driverId = 1;
            int previousVehicleId = -1;
            double driverStart = 0;
            double driverEnd = 0;
            double driveTime = 0;
            List<double> driversEnds = new List<double>();
            List<int> driversIds = new List<int>();
            foreach (var route in routes
                .OrderBy(rt => rt.VehicleTractor == null ? rt.Vehicle.Id : rt.VehicleTractor.Id)
                .ThenBy(rt => rt.ArrivalTimes[0]))
            {
                int currentVehicleId = route.VehicleTractor == null ? route.Vehicle.Id : route.VehicleTractor.Id;
                double routeDriveTime = route.Distances.Sum(d => d.Time);
                double routeEndTime = route.DepartureTimes[^1];
                if (currentVehicleId != previousVehicleId)
                {
                    if (driversIds.Any())
                    {
                        driverId = Math.Max(driversIds.Max(), driverId) + 1;
                    }
                    driversEnds.Clear();
                    driversIds.Clear();
                    driverStart = route.VehicleTractor == null ? route.ArrivalTimes[0] : route.DepartureTimes[0];
                    driverEnd = route.DepartureTimes[^1];
                    driveTime = routeDriveTime;
                }
                else
                {
                    if (driveTime + routeDriveTime > VRPCostFunction.SingleDriverRideTime || routeEndTime - driverStart > VRPCostFunction.SingleDriverWorkTime)
                    {
                        driversEnds.Add(driverEnd);
                        driversIds.Add(driverId);
                        driverId += 1;
                        driverStart = route.VehicleTractor == null ? route.ArrivalTimes[0] : route.DepartureTimes[0];
                        driveTime = routeDriveTime;
                        if (driversEnds.Any() && driverStart - driversEnds[0] > VRPCostFunction.SingleDriverRestTime)
                        {
                            driverId = driversIds[0];
                            driversIds.RemoveAt(0);
                            driversEnds.RemoveAt(0);
                        }
                    }
                    else
                    {
                        driverEnd = route.DepartureTimes[^1];
                    }
                    driveTime += routeDriveTime;
                }
                previousVehicleId = currentVehicleId;
                string routeDescription = $"{routeId};";
                routeDescription += $"{(route.VehicleTractor == null ? -1 : route.VehicleTractor.Id)};";
                routeDescription += $"{(route.Vehicle == null ? -1 : route.Vehicle.Id)};";
                routeDescription += $"{driverId};";
                routeDescription += $"{zeroHour.AddSeconds(route.ArrivalTimes[0])};";
                routeDescription += $"{zeroHour.AddSeconds(route.DepartureTimes[^1])};";
                for (int i = 1; i < route.VisitedLocations.Count - 1; i++)
                {
                    routeDescription += $"{route.VisitedLocations[i].Id};";
                    routeDescription += $"{zeroHour.AddSeconds(route.TimeWindowStart[i])};";
                    routeDescription += $"{zeroHour.AddSeconds(route.ArrivalTimes[i])};";
                    routeDescription += $"{TimeSpan.FromSeconds((route.ArrivalTimes[i] - route.DepartureTimes[i - 1]) - route.Distances[i - 1].Time)};";
                }
                result.Add(routeDescription);
                routeId++;
            }
            return result;
        }

        public static List<string> VisitsSummary(List<IRoute> routes, DateTime zeroHour)
        {
            Dictionary<string, List<double[]>> visits = new ();
            foreach (var route in routes.OrderBy(rt => rt.ArrivalTimes[0]))
            {
                for (int i = 1; i < route.VisitedLocations.Count; i++)
                {
                    if (!visits.ContainsKey(route.VisitedLocations[i].Id))
                    {
                        visits.Add(route.VisitedLocations[i].Id, new());
                    }
                    if (!route.UnloadedRequests.Any() && !route.LoadedRequests.Any())
                    {
                        continue;
                    }
                    int maxSize = 33;
                    if (route.UnloadedRequests[i].Any())
                    {
                        maxSize = Math.Min(maxSize, route.UnloadedRequests[i].Min(rq => rq.MaxVehicleSize.EpCount));
                    }
                    if (route.LoadedRequests[i].Any())
                    {
                        maxSize = Math.Min(maxSize, route.LoadedRequests[i].Min(rq => rq.MaxVehicleSize.EpCount));
                    }
                    double strictTimeWindowStart = !route.UnloadedRequests[i].Any() ? 0 : route.UnloadedRequests[i].Max(rq => rq.DeliveryAvailableTimeWindowStart);
                    double strictTimeWindowEnd = !route.UnloadedRequests[i].Any() ? 5 * 86400 : route.UnloadedRequests[i].Min(rq => rq.DeliveryAvailableTimeWindowEnd);
                    double preferedTimeWindowStart = !route.UnloadedRequests[i].Any() ? 0 : route.UnloadedRequests[i].Max(rq => rq.DeliveryPreferedTimeWindowStart);
                    double preferedTimeWindowEnd = !route.UnloadedRequests[i].Any() ? 5 * 86400 : route.UnloadedRequests[i].Max(rq => rq.DeliveryPreferedTimeWindowEnd);
                    strictTimeWindowStart = Math.Max(strictTimeWindowStart, !route.LoadedRequests[i].Any() ? 0 : route.LoadedRequests[i].Max(rq => rq.PickupAvailableTimeWindowStart));
                    strictTimeWindowEnd = Math.Min(strictTimeWindowEnd, !route.LoadedRequests[i].Any() ? 5 * 86400 : route.LoadedRequests[i].Min(rq => rq.PickupAvailableTimeWindowEnd));
                    preferedTimeWindowStart = Math.Max(preferedTimeWindowStart, !route.LoadedRequests[i].Any() ? 0 : route.LoadedRequests[i].Max(rq => rq.PickupPreferedTimeWindowStart));
                    preferedTimeWindowEnd = Math.Min(preferedTimeWindowEnd, !route.LoadedRequests[i].Any() ? 5 * 86400 : route.LoadedRequests[i].Max(rq => rq.PickupPreferedTimeWindowEnd));
                    visits[route.VisitedLocations[i].Id].Add(new double[] {
                        route.Vehicle.Capacity[0],
                        route.ArrivalTimes[i],
                        Math.Max(0,route.TimeWindowStart[i]),
                        Math.Min(5 * 86400, route.TimeWindowEnd[i]),
                        route.UnloadedRequests[i].Sum(rq => rq.Size[0]),
                        route.LoadedRequests[i].Sum(rq => rq.Size[0]),
                    strictTimeWindowStart,
                    preferedTimeWindowStart,
                    preferedTimeWindowEnd,
                    strictTimeWindowEnd,
                    maxSize
                    });
                }
            }

            List<string> result = new List<string>();
            foreach (var item in visits
                .OrderBy(vs => vs.Key))
            {
                foreach (var visit in item.Value.OrderBy(vs => vs[6]))
                {
                    result.Add($"{item.Key};{visit[0]};" + 
                        $"{zeroHour.AddSeconds(visit[6])};{zeroHour.AddSeconds(visit[7])};{zeroHour.AddSeconds(visit[1])};{zeroHour.AddSeconds(visit[8])};{zeroHour.AddSeconds(visit[9])};" +
                        $"{zeroHour.AddSeconds(visit[2])};{zeroHour.AddSeconds(visit[3])};{visit[4]};{visit[5]};" +
                        $"{TimeSpan.FromSeconds(Math.Max(0, visit[1] - visit[8]))};{TimeSpan.FromSeconds(Math.Max(0, visit[1] - visit[9]))};" +
                        $"{Math.Round(visit[4] / visit[10], 2)}");
                }
            }
            return result;
        }

        public static List<string> RequestsSummary(List<TransportRequest> requests, string HomeDepotId, DateTime zeroHour)
        {
            Dictionary<string, List<double[]>> visits = new();
            foreach (var request in requests.OrderBy(rq => rq.PickupLocation.Id == HomeDepotId ? rq.DeliveryPreferedTimeWindowStart : rq.PickupPreferedTimeWindowStart))
            {
                string requestLocationId = request.PickupLocation.Id == HomeDepotId ? request.DeliveryLocation.Id : request.PickupLocation.Id;
                double timeWindowStart = request.PickupLocation.Id == HomeDepotId ? request.DeliveryPreferedTimeWindowStart : request.PickupPreferedTimeWindowStart;
                double timeWindowEnd = request.PickupLocation.Id == HomeDepotId ? request.DeliveryPreferedTimeWindowEnd : request.PickupPreferedTimeWindowEnd;
                double strictTimeWindowStart = request.PickupLocation.Id == HomeDepotId ? request.DeliveryAvailableTimeWindowStart : request.PickupAvailableTimeWindowStart;
                double strictTimeWindowEnd = request.PickupLocation.Id == HomeDepotId ? request.DeliveryAvailableTimeWindowEnd : request.PickupAvailableTimeWindowEnd;
                int packageCount = request.PickupLocation.Id == HomeDepotId ? request.PackageCount : -request.PackageCount;
                if (!visits.ContainsKey(requestLocationId))
                    {
                        visits.Add(requestLocationId, new());
                    }
                    visits[requestLocationId].Add(new double[] {
                        request.Size[0],
                        packageCount,
                        request.PackageCountForImediateRetrieval,
                        Math.Max(0,timeWindowStart),
                        Math.Min(5 * 86400, timeWindowEnd),
                        Math.Max(0,strictTimeWindowStart),
                        Math.Min(5 * 86400, strictTimeWindowEnd),
                        });
            }

            List<string> result = new List<string>();
            result.Add($"Id oddzialu;L.m.pal;Jedn.log;Kontenery;Najwczesniejszy przyjazd;Preferowany przyjazd;Koniec preferowanego;Najpozniejszy przyjazd");
            foreach (var item in visits)
            {
                foreach (var visit in item.Value.OrderByDescending(v => v[1]))
                {
                    result.Add($"{item.Key};{visit[0]};{visit[1]};{visit[2]};{zeroHour.AddSeconds(visit[5])};{zeroHour.AddSeconds(visit[3])};{zeroHour.AddSeconds(visit[4])};{zeroHour.AddSeconds(visit[6])}");
                }
            }
            return result;
        }

        public static List<string> VehiclesSummary(List<IRoute> routes, List<Vehicle> vehicles, DateTime zeroHour)
        {
            var vehicleGroups = vehicles.GroupBy(vh => vh.RoadProperties.EpCount).OrderBy(gr => gr.Key);
            List<string> result = new List<string>();
            var normalizedHour = zeroHour.AddMinutes(-zeroHour.Minute).AddSeconds(-zeroHour.Second);
            string line = "";
            foreach (var vehicleGroup in vehicleGroups)
            {
                line += $";{vehicleGroup.Key}";
            }
            result.Add(line);
            line = "";
            foreach (var vehicleGroup in vehicleGroups)
            {
                line += $";{vehicleGroup.Count()}";
            }
            result.Add(line);

            var maxTime = routes.Max(rt => rt.DepartureTimes[^1]);
            var minTime = (normalizedHour - zeroHour).TotalSeconds;
            for (double relativeTime = minTime; relativeTime < maxTime; relativeTime += 900)
            {
                line = $"{zeroHour.AddSeconds(relativeTime)}";
                foreach (var vehicleGroup in vehicleGroups)
                {
                    if (vehicleGroup.Key > 0)
                    {
                        line += $";{routes.Count(rt => rt.Vehicle.RoadProperties.EpCount == vehicleGroup.Key && relativeTime >= rt.ArrivalTimes[0] && rt.DepartureTimes[^1] >= relativeTime)}";
                    }
                    else
                    {
                        line += $";{routes.Count(rt => rt.VehicleTractor != null && relativeTime >= rt.DepartureTimes[0] && rt.DepartureTimes[^1] >= relativeTime)}";
                    }
                }
                result.Add(line);
            }
            return result;
        }
    }
}
