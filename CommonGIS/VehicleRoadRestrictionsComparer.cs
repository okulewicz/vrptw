using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CommonGIS
{
    /// <summary>
    /// Compares if 2 vehicle profiles in VehicleRoadRestrictionProperties are identical
    /// </summary>
    public class VehicleRoadRestrictionsComparer : IEqualityComparer<VehicleRoadRestrictionProperties>, IComparer<VehicleRoadRestrictionProperties>
    {
        private static VehicleRoadRestrictionsComparer _instance = new VehicleRoadRestrictionsComparer();
        /// <summary>
        /// Instance of the comparer
        /// </summary>
        public static VehicleRoadRestrictionsComparer Instance => _instance;

        private VehicleRoadRestrictionsComparer()
        {
        }

        public int Compare(VehicleRoadRestrictionProperties x, VehicleRoadRestrictionProperties y)
        {
            if (x.Equals(y))
            {
                return 0;
            }
            else if (x.DoesVehicleFitIntoRestrictions(y))
            {
                return -1;
            }
            else if (y.DoesVehicleFitIntoRestrictions(x))
            {
                return 1;
            }
            else
            {
                //HACK: this may be wrong if vehicles are incomparable
                return 0;
            }
        }

        /// <summary>
        /// Are x and y instances of VehicleRoadRestrictionProperties identical
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(VehicleRoadRestrictionProperties x, VehicleRoadRestrictionProperties y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// Creates hash code of VehicleRoadRestrictionProperties object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode([DisallowNull] VehicleRoadRestrictionProperties obj)
        {
            return obj.GrossVehicleWeight + obj.EpCount;
        }
    }
}