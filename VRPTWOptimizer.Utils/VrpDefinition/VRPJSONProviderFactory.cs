using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using VRPTWOptimizer.Utils.Model;
using static VRPTWOptimizer.Utils.Model.ViaTmsJSONDTOs;

namespace VRPTWOptimizer.Utils.VrpDefinition
{
    public class VRPJSONProviderFactory
    {
        public static readonly JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter> { new CustomIntConverter() }
        };

        public IVRPJSONProvider GetVrpJsonProvider(string request,
            InputTimeWindowTransformation transformation = InputTimeWindowTransformation.None,
            RequestPropertiesTransformation requestPropertiesTransformation = RequestPropertiesTransformation.None)
        {
            bool isVIATMSJSON = CheckIfViaTmsJSON(request);
            if (isVIATMSJSON)
            {
                VRPDefinitionViaTmsDTO vrpDTO = JsonConvert.DeserializeObject<VRPDefinitionViaTmsDTO>(request, settings);
                if (transformation == InputTimeWindowTransformation.To3Hours)
                {
                    foreach (var transportRequest in vrpDTO.TransportRequests)
                    {
                        if (transportRequest.TimeWindowStart.Hour > 6)
                        {
                            transportRequest.TimeWindowStart = transportRequest.TimeWindowStart.AddHours(-1);
                        }
                        else
                        {
                            transportRequest.TimeWindowEnd = transportRequest.TimeWindowEnd.AddHours(1);
                        }
                    }
                }
                if (transformation == InputTimeWindowTransformation.ToMorningAndEvening)
                {
                    foreach (var transportRequest in vrpDTO.TransportRequests)
                    {
                        if (transportRequest.TimeWindowStart.Hour <= 12)
                        {
                            transportRequest.TimeWindowStart = transportRequest.TimeWindowStart.AddHours(6 - transportRequest.TimeWindowStart.Hour);
                            transportRequest.TimeWindowEnd = transportRequest.TimeWindowEnd.AddHours(14 - transportRequest.TimeWindowEnd.Hour);
                        }
                        else
                        {
                            transportRequest.TimeWindowStart = transportRequest.TimeWindowStart.AddHours(14 - transportRequest.TimeWindowStart.Hour);
                            transportRequest.TimeWindowEnd = transportRequest.TimeWindowEnd.AddHours(22 - transportRequest.TimeWindowEnd.Hour);
                        }
                    }
                }
                if (transformation == InputTimeWindowTransformation.ToAllDay)
                {
                    foreach (var transportRequest in vrpDTO.TransportRequests)
                    {
                        transportRequest.TimeWindowStart = transportRequest.TimeWindowStart.AddHours(6 - transportRequest.TimeWindowStart.Hour);
                        transportRequest.TimeWindowEnd = transportRequest.TimeWindowEnd.AddHours(22 - transportRequest.TimeWindowEnd.Hour);
                    }
                }
                return new VRPDefinitionViaTmsDTOProvider(vrpDTO);
            }
            else
            {
                VRPDefinitionJSONDTO vrpDTO = JsonConvert.DeserializeObject<VRPDefinitionJSONDTO>(request, settings);
                if (requestPropertiesTransformation == RequestPropertiesTransformation.Clear)
                {
                    vrpDTO.Distances?.Clear();
                    if (vrpDTO.ServiceTimeEstimator.StopTime == 0 && vrpDTO.ServiceTimeEstimator.TimePerPickedUpPiece == 0 && vrpDTO.ServiceTimeEstimator.TimePerDeliveredPiece == 0)
                    {
                        vrpDTO.ServiceTimeEstimator.StopTime = 1200;
                        vrpDTO.ServiceTimeEstimator.TimePerPickedUpPiece = 60;
                        vrpDTO.ServiceTimeEstimator.TimePerDeliveredPiece = 90;
                    }
                    for (int i = 0; i < vrpDTO.Requests.Count; i++)
                    {
                        int requestId = vrpDTO.Requests[i].Id;
                        if (vrpDTO.Requests[i].PickupLocation.Id == vrpDTO.Requests[i].DeliveryLocation.Id)
                        {
                            foreach (var routes in vrpDTO.VIATMSSolution.VehicleRoutes)
                            {
                                foreach (var route in routes.Routes)
                                {
                                    route.Orders.Remove(requestId);
                                }
                            }
                            int removedCount = vrpDTO.Requests.RemoveAll(rq => rq.Id == requestId);
                            i -= removedCount;
                            continue;
                        }
                        if (vrpDTO.DepotId != vrpDTO.Requests[i].PickupLocation.Id && vrpDTO.DepotId != vrpDTO.Requests[i].DeliveryLocation.Id)
                        {
                            foreach (var routes in vrpDTO.VIATMSSolution.VehicleRoutes)
                            {
                                foreach (var route in routes.Routes)
                                {
                                    route.Orders.Remove(requestId);
                                }
                            }
                            int removedCount = vrpDTO.Requests.RemoveAll(rq => rq.Id == requestId);
                            i -= removedCount;
                            continue;
                        }
                        if (vrpDTO.VIATMSSolution != null && vrpDTO.VIATMSSolution.LeftRequestIds.Contains(requestId.ToString()))
                        {
                            foreach (var routes in vrpDTO.VIATMSSolution.VehicleRoutes)
                            {
                                foreach (var route in routes.Routes)
                                {
                                    route.Orders.Remove(requestId);
                                }
                            }
                            int removedCount = vrpDTO.Requests.RemoveAll(rq => rq.Id == requestId);
                            i -= removedCount;
                            continue;
                        }
                        if (vrpDTO.VIATMSSolution != null && vrpDTO.VIATMSSolution.VehicleRoutes.SelectMany(rt => rt.Routes.SelectMany(rt => rt.Orders)).Count(orderId => orderId == requestId) > 1)
                        {
                            foreach (var routes in vrpDTO.VIATMSSolution.VehicleRoutes)
                            {
                                foreach (var route in routes.Routes)
                                {
                                    route.Orders.Remove(requestId);
                                }
                            }
                            int removedCount = vrpDTO.Requests.RemoveAll(rq => rq.Id == requestId);
                            i -= removedCount;
                            continue;
                        }

                    }
                    if (vrpDTO.VIATMSSolution != null)
                    {
                        foreach (var routes in vrpDTO.VIATMSSolution.VehicleRoutes)
                        {
                            foreach (var route in routes.Routes)
                            {
                                route.Orders.RemoveAll(orderId => !vrpDTO.Requests.Any(rq => rq.Id == orderId));
                            }
                        }

                        foreach (var routes in vrpDTO.VIATMSSolution.VehicleRoutes)
                        {
                            routes.Routes.RemoveAll(rt => !rt.Orders.Any());
                        }
                        vrpDTO.VIATMSSolution.VehicleRoutes.RemoveAll(rts => !rts.Routes.Any());
                        vrpDTO.VIATMSSolution.LeftRequestIds.Clear();
                    }
                    foreach (var requestDTO in vrpDTO.Requests)
                    {
                        requestDTO.NecessaryVehicleSpecialProperties = new int[0];
                    }
                    foreach (var vehicle in vrpDTO.Vehicles)
                    {
                        vehicle.AvailabilityStart = 6 * 3600;
                        vehicle.AvailabilityEnd = 6 * 3600 + 8 * 3600;

                    }
                }
                return new VRPDefinitionJSONDTOProvider(vrpDTO);
            }
        }

        public static bool CheckIfViaTmsJSON(string request)
        {
            Dictionary<string, object> genericInputObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(request);
            var isVIATMSJSON = genericInputObject.ContainsKey("home_depots");
            return isVIATMSJSON;
        }
    }
}