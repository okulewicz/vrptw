using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRPTWOptimizer.Enums
{
    [Flags]
    public enum ValidationCode
    {
        RequestTooLarge = 1,
        StrangeCapacity = 2,
        FarAwayLocation = 4,
        LowVehicleAvailability = 8,
    }
}
