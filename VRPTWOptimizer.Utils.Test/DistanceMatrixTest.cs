using CommonGIS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRPTWOptimizer.Enums;
using VRPTWOptimizer.Utils.Provider;
using VRPTWOptimizer.Utils.Reader;
using VRPTWOptimizer.Utils.VrpDefinition;

namespace VRPTWOptimizer.Utils.Test
{
    [TestClass]
    public class DistanceMatrixTest
    {
        [TestMethod]
        public void TestDataRetrieval()
        {
            VehicleRoadRestrictionProperties smallProfile = new VehicleRoadRestrictionProperties(40000, 4, 3, 10, CommonGIS.Enums.VehicleTypeRouting.TractorWithTrailer);
            VehicleRoadRestrictionProperties mediumProfile = new VehicleRoadRestrictionProperties(40000, 4, 3, 16, CommonGIS.Enums.VehicleTypeRouting.TractorWithTrailer);
            VehicleRoadRestrictionProperties largeProfile = new VehicleRoadRestrictionProperties(40000, 4, 3, 22, CommonGIS.Enums.VehicleTypeRouting.TractorWithTrailer);
            Location a = new Location("A", 0, 0, LocationType.DC);
            Location b = new Location("B", 1, 0, LocationType.Store);
            Location c = new Location("C", 0, 1, LocationType.Store);
            Distance distanceABSmallTruck = new Distance(a.Id, b.Id, 2, 3, smallProfile);
            Distance distanceABLargeTruck = new Distance(a.Id, b.Id, 3, 4, largeProfile);
            Distance distanceACSmallTruck = new Distance(a.Id, c.Id, 4, 5, smallProfile);
            DistanceMatrixProvider provider = new DistanceMatrixProvider(new List<Distance>() { distanceABSmallTruck, distanceABLargeTruck, distanceACSmallTruck });
            Assert.AreEqual(distanceABLargeTruck.Length, provider.GetDistance(a, b, largeProfile).Length);
            Assert.AreEqual(distanceACSmallTruck.Length, provider.GetDistance(a, c, smallProfile).Length);
            Assert.AreEqual(distanceABSmallTruck.Length, provider.GetDistance(a, b, smallProfile).Length);
            Assert.AreEqual(double.MaxValue, provider.GetDistance(a, c, largeProfile).Length);
            Assert.AreEqual(double.MaxValue, provider.GetDistance(a, c, mediumProfile).Length);
            Assert.AreEqual(distanceABLargeTruck.Length, provider.GetDistance(a, b, mediumProfile).Length);

            Random random = new Random();

        }

        [TestMethod]
        public void TestOffsetTimeAvaialbility()
        {
            foreach (var item in new string[] {
                "sample.json"
            })
            {

                string request = FileWrapper.SafeReadAllText(item);
                VRPJSONProviderFactory vrpJSONProviderFactory = new();
                IVRPJSONProvider vrpProvider = vrpJSONProviderFactory.GetVrpJsonProvider(request, InputTimeWindowTransformation.None);
                Assert.AreEqual(vrpProvider.Drivers.Max(d => Math.Max(d.AvailabilityEnd - d.AvailabilityStart, 2 * Driver.SingleWorkPeriod + 2 * Driver.DailyBreakPeriod + Driver.AdditionalDailyWorkPeriod) + d.AvailabilityStart), vrpProvider.Vehicles.First(v => v.OwnerID == VehicleOwnership.Internal).AvailabilityEnd);
            }

        }
    }
}
