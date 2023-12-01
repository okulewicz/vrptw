using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRPTWOptimizer.Utils.Model
{
    public class DriverDTO : Driver
    {
        [JsonConstructor]
        public DriverDTO(int id,
                         double availabilityStart,
                         double availabilityEnd,
                         int[] compatibileVehiclesIds,
                         int[] carrierId) : base(id,
                                                              availabilityStart,
                                                              availabilityEnd,
                                                              compatibileVehiclesIds,
                                                              carrierId)
        {
        }
    }
}
