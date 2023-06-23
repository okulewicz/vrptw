using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRPTWOptimizer.Utils.Validation
{
    [Flags]
    public enum ErrorCode
    {
        ImpossibleDriveTime = 1,
        RequestPropertiesExcludeAllVehicles = 2,
        ImpossiblePreferredTimeWindow = 4,
        ImpossibleTimeWindow = 8,
        WrongType = 16,
        WrongProperties = 32,
        CapacityVehicleMissing = 64,
        TractorAssignedAsCapacityVehicle = 128,
        ExceededCapacity = 256,
        OverlappingRoutes = 512,
        TractorMissing = 1024,
        IdenticalDeliveryAndPickup = 2048,
        ZeroPackages = 4096,
        TooLargeVehicleSize = 8192,
        ImpossibleOperation = 16384,
        LeakyRoutes = 32768
    }
}