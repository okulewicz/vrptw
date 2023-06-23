using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRPTWOptimizer.Utils.Model
{
    public class VIATMSSolutionDTO
    {
        public List<string> LeftRequestIds { get; set; }
        public List<VehicleRoutes> VehicleRoutes { get; set; }
    }
}