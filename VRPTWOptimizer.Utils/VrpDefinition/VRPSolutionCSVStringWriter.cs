using CommonGIS;
using CommonGIS.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VRPTWOptimizer.Utils.VrpDefinition
{
    public class VRPSolutionCSVStringWriter
    {
        private const string DateTimeFormat = "dd.MM HH:mm";
        private const string TimeSpanFormat = "hh\\:mm";
        public const string Sep = ";";

        private static StringBuilder CreateScheduleHeader(int capacityDim)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("TransportId");
            sb.Append(Sep);
            sb.Append("TractorId");
            sb.Append(Sep);
            sb.Append("TrailerTruckId");
            sb.Append(Sep);
            sb.Append("DriverId");
            sb.Append(Sep);
            for (int i = 0; i < capacityDim; i++)
            {
                sb.Append("CapacityDim");
                sb.Append(i + 1);
                sb.Append(Sep);
            }
            sb.Append("LocationId");
            sb.Append(Sep);
            sb.Append("MaxEpCount");
            sb.Append(Sep);
            sb.Append("RequestId");
            sb.Append(Sep);
            for (int i = 0; i < capacityDim; i++)
            {
                sb.Append("SizeDim");
                sb.Append(i);
                sb.Append(Sep);
            }
            sb.Append("ArrivalTime");
            sb.Append(Sep);
            sb.Append("DepartureTime");
            sb.Append(Sep);
            sb.Append("TimeWindowStart");
            sb.Append(Sep);
            sb.Append("TimeWindowEnd");
            sb.Append(Sep);
            sb.Append("EarlyArrival");
            sb.Append(Sep);
            sb.Append("DelayedArrival");
            sb.Append(Sep);
            sb.Append("DistanceTo");
            sb.Append(Sep);
            sb.Append("TravelTimeTo");
            return sb;
        }

        private static StringBuilder CreateScheduleLine(
            VRPDefinition definition,
            VRPSolution.TransportItem transport,
            Vehicle vehicle,
            VRPSolution.ScheduleItem scheduleItem,
            TransportRequest request,
            double timeWindowStart,
            double timeWindowEnd,
            double modifier,
            Distance distance)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(transport != null ? transport.TransportId : "");
            sb.Append(Sep);
            sb.Append(transport != null ? transport.TractorId : "");
            sb.Append(Sep);
            sb.Append(transport != null ? transport.TrailerTruckId : "");
            sb.Append(Sep);
            sb.Append(transport != null ? transport.DriverId : "");
            sb.Append(Sep);
            int sizeDims = vehicle != null ? vehicle.Capacity.Length : request.Size.Length;
            for (int i = 0; i < sizeDims; i++)
            {
                sb.Append(vehicle != null ? vehicle.Capacity[i] : "");
                sb.Append(Sep);
            }
            sb.Append(scheduleItem != null ? scheduleItem.LocationId : "");
            sb.Append(Sep);
            sb.Append(request != null ? request.MaxVehicleSize.EpCount : "");
            sb.Append(Sep);
            sb.Append(request != null ? request.Id : "");
            sb.Append(Sep);
            for (int i = 0; i < sizeDims; i++)
            {
                sb.Append(request != null ? Math.Floor(request.Size[i]) * modifier : "");
                sb.Append(Sep);
            }
            sb.Append(scheduleItem != null ? definition.ZeroHour.AddSeconds(scheduleItem.ArrivalTime).ToString(DateTimeFormat) : "");
            sb.Append(Sep);
            sb.Append(scheduleItem != null ? definition.ZeroHour.AddSeconds(scheduleItem.DepartureTime).ToString(DateTimeFormat) : "");
            sb.Append(Sep);
            sb.Append(definition.ZeroHour.AddSeconds(timeWindowStart).ToString(DateTimeFormat));
            sb.Append(Sep);
            sb.Append(definition.ZeroHour.AddSeconds(timeWindowEnd).ToString(DateTimeFormat));
            sb.Append(Sep);
            sb.Append(scheduleItem != null ? TimeSpan.FromSeconds(Math.Max(timeWindowStart - scheduleItem.ArrivalTime, 0)).ToString(TimeSpanFormat) : "");
            sb.Append(Sep);
            sb.Append(scheduleItem != null ? TimeSpan.FromSeconds(Math.Max(0, scheduleItem.ArrivalTime - timeWindowEnd)).ToString(TimeSpanFormat) : "");
            sb.Append(Sep);
            sb.Append(distance != null ? Math.Round(distance.Length) / 1000 : "");
            sb.Append(Sep);
            sb.Append(distance != null ? TimeSpan.FromSeconds(distance.Time).ToString(TimeSpanFormat) : "");
            return sb;
        }

        public static Dictionary<string, string[]> ProduceCSVLinesWithSemicolonSeparator(VRPDefinition definition, IDistanceProvider distanceProvider)
        {
            Dictionary<string, string[]> solutions = new();
            foreach (var solution in definition.Solutions)
            {
                var orderedTransports = solution.Transports
                    .OrderBy(tr => tr.TractorId)
                    .ThenBy(tr => tr.TrailerTruckId)
                    .ThenBy(tr => tr.AvailableForLoadingTime);
                var solutionKey = $"{definition.Client}-{definition.DepotId}-{solution.Algorithm}";
                solutions.Add(solutionKey, new string[0]);
                List<string> lines = new();
                lines.Add(CreateScheduleHeader(definition.Vehicles[0].Capacity.Length).ToString());
                foreach (var requestId in solution.LeftRequestsIds)
                {
                    var request = definition.Requests.First(rq => rq.Id == requestId);
                    double timeWindowStart = Math.Max(0, request.DeliveryPreferedTimeWindowStart);
                    double timeWindowEnd = Math.Min(3 * 86400, request.DeliveryPreferedTimeWindowEnd);
                    StringBuilder sb = CreateScheduleLine(definition, null, null, null, request, timeWindowStart, timeWindowEnd, -1, null);
                    lines.Add(sb.ToString());
                }
                foreach (var transport in orderedTransports)
                {
                    Vehicle vehicle = definition.Vehicles.First(vh => vh.Id == transport.TrailerTruckId);
                    Location previousLocation = null;
                    foreach (var scheduleItem in transport.Schedule)
                    {
                        var sampleDeliveryRequest = definition.Requests.FirstOrDefault(rq => rq.DeliveryLocation.Id == scheduleItem.LocationId);
                        var samplePickupRequest = definition.Requests.FirstOrDefault(rq => rq.PickupLocation.Id == scheduleItem.LocationId);
                        foreach (var requestId in scheduleItem.UnloadedRequestsIds)
                        {
                            var location = sampleDeliveryRequest != null ? sampleDeliveryRequest.DeliveryLocation : samplePickupRequest.PickupLocation;
                            var distance = previousLocation != null ? distanceProvider.GetDistance(previousLocation, location, vehicle.RoadProperties) : null;
                            var request = definition.Requests.First(rq => rq.Id == requestId);
                            double timeWindowStart = Math.Max(0, request.DeliveryPreferedTimeWindowStart);
                            double timeWindowEnd = Math.Min(3 * 86400, request.DeliveryPreferedTimeWindowEnd);
                            StringBuilder sb = CreateScheduleLine(definition, transport, vehicle, scheduleItem, request, timeWindowStart, timeWindowEnd, -1, distance);
                            lines.Add(sb.ToString());
                            previousLocation = location;
                        }
                        foreach (var requestId in scheduleItem.LoadedRequestsIds)
                        {
                            var location = sampleDeliveryRequest != null ? sampleDeliveryRequest.DeliveryLocation : samplePickupRequest.PickupLocation;
                            var distance = previousLocation != null ? distanceProvider.GetDistance(previousLocation, location, vehicle.RoadProperties) : null;
                            var request = definition.Requests.First(rq => rq.Id == requestId);
                            double timeWindowStart = Math.Max(0, request.PickupPreferedTimeWindowStart);
                            double timeWindowEnd = Math.Min(3 * 86400, request.PickupPreferedTimeWindowEnd);
                            StringBuilder sb = CreateScheduleLine(definition, transport, vehicle, scheduleItem, request, timeWindowStart, timeWindowEnd, 1, distance);
                            lines.Add(sb.ToString());
                            previousLocation = location;
                        }
                        if (scheduleItem.LoadedRequestsIds.Count == 0 && scheduleItem.UnloadedRequestsIds.Count == 0)
                        {
                            var location = sampleDeliveryRequest != null ? sampleDeliveryRequest.DeliveryLocation : samplePickupRequest.PickupLocation;
                            var distance = previousLocation != null ? distanceProvider.GetDistance(previousLocation, location, vehicle.RoadProperties) : null;
                            double timeWindowStart = Math.Max(0, vehicle.AvailabilityStart);
                            double timeWindowEnd = Math.Min(3 * 86400, vehicle.AvailabilityEnd);
                            StringBuilder sb = CreateScheduleLine(definition, transport, vehicle, scheduleItem, null, timeWindowStart, timeWindowEnd, 1, distance);
                            lines.Add(sb.ToString());
                            previousLocation = location;
                        }
                    }
                }
                solutions[solutionKey] = lines.ToArray();
            }
            return solutions;
        }
    }
}