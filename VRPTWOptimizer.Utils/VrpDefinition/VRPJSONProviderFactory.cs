using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using VRPTWOptimizer.Utils.Model;
using static VRPTWOptimizer.Utils.Model.ViaTmsJSONDTOs;

namespace VRPTWOptimizer.Utils.VrpDefinition
{
    public class VRPJSONProviderFactory
    {
        private readonly JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public IVRPJSONProvider GetVrpJsonProvider(string request, InputTimeWindowTransformation transformation)
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
                return new VRPDefinitionViaTmsDTOProvider(vrpDTO);
            }
            else
            {
                VRPDefinitionJSONDTO vrpDTO = JsonConvert.DeserializeObject<VRPDefinitionJSONDTO>(request, settings);
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