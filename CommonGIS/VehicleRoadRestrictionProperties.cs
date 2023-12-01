using CommonGIS.Enums;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace CommonGIS
{
    /// <summary>
    /// Vehicle profile utilized for find routes and specify restrictions at Location object
    /// </summary>
    public class VehicleRoadRestrictionProperties : IEquatable<VehicleRoadRestrictionProperties>
    {
        /// <summary>
        /// Max capacity for truck + trailer duo in europallets
        /// </summary>
        public const int MaxEPCount = 38;
        /// <summary>
        /// Max gross weight of the vehicle
        /// </summary>
        public const int MaxGrossVehicleWeight = 40000;
        /// <summary>
        /// Max height of the vehicle
        /// </summary>
        public const int MaxHeight = 400;
        /// <summary>
        /// Max width of the vehicle
        /// </summary>
        public const int MaxWidth = 260;
        /// <summary>
        /// Vehicle size given in europallets
        /// </summary>
        public int EpCount { get; }
        /// <summary>
        /// Vehicle max gross weight in kilograms
        /// </summary>
        public int GrossVehicleWeight { get; }
        /// <summary>
        /// Vehicle height in centimeters
        /// </summary>
        public int Height { get; }
        /// <summary>
        /// Vehicle type for the purpose of finding route on the map
        /// </summary>
        public VehicleTypeRouting VehicleType { get; }
        /// <summary>
        /// Vehicle width in centimeters
        /// </summary>
        public int Width { get; }

	/// <summary>
	/// The highest tunnel category the vehicle is forbidden to pass.
	/// </summary>
	/// The value <see cref="TunnelCategory.C"/> meant that passing tunnels
	/// of categories <see cref="TunnelCategory.D"/>, <see cref="TunnelCategory.C"/>, <see cref="TunnelCategory.E"/> is forbidden.
	public TunnelCategory ForbiddenTunnelCategory {get;}

        /// <summary>
        /// Creates vehicle profile
        /// </summary>
        /// <param name="grossVehicleWeight"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="epCount"></param>
        /// <param name="vehicleType"></param>
        public VehicleRoadRestrictionProperties(int grossVehicleWeight, int height, int width, int epCount, VehicleTypeRouting vehicleType)
        {
            GrossVehicleWeight = grossVehicleWeight;
            Height = height;
            Width = width;
            EpCount = epCount;
            VehicleType = vehicleType;
	    ForbiddenTunnelCategory = TunnelCategory.NoRestriction;
        }

	/// <summary>
        /// Creates vehicle profile
        /// </summary>
        /// <param name="grossVehicleWeight"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="epCount"></param>
        /// <param name="vehicleType"></param>
	/// <param name="forbiddenTunnelCategory"></param>
	[JsonConstructor]
        public VehicleRoadRestrictionProperties(int grossVehicleWeight, int height, int width, int epCount, VehicleTypeRouting vehicleType, TunnelCategory forbiddenTunnelCategory = TunnelCategory.NoRestriction)
        {
            GrossVehicleWeight = grossVehicleWeight;
            Height = height;
            Width = width;
            EpCount = epCount;
            VehicleType = vehicleType;
	    ForbiddenTunnelCategory = forbiddenTunnelCategory;
        }

        private bool IsNotLargerType(VehicleRoadRestrictionProperties vehicleProperties)
        {
            if ((int)vehicleProperties.VehicleType <= (int)VehicleType)
            {
                return true;
            }
            else if ((int)vehicleProperties.VehicleType > (int)VehicleType)
            {
                return false;
            }
            else if (VehicleType == VehicleTypeRouting.TractorWithTrailer)
            {
                return true;
            }
            else if (VehicleType == VehicleTypeRouting.StraightTruck && vehicleProperties.VehicleType == VehicleTypeRouting.StraightTruck)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static VehicleRoadRestrictionProperties BoundProperties(VehicleRoadRestrictionProperties roadProperties)
        {
            return new VehicleRoadRestrictionProperties(
                Math.Min(roadProperties.GrossVehicleWeight, MaxGrossVehicleWeight),
                Math.Min(roadProperties.Height, MaxHeight),
                Math.Min(roadProperties.Width, MaxWidth),
                Math.Min(roadProperties.EpCount, MaxEPCount),
                (VehicleTypeRouting)Math.Min((int)roadProperties.VehicleType, (int)VehicleTypeRouting.TractorWithTrailer),
		(TunnelCategory)Math.Min((int)TunnelCategory.B, (int)roadProperties.ForbiddenTunnelCategory)
                );
        }

        /// <summary>
        /// Verifies if vehicle with vehicleProperties fits into this restrictions
        /// </summary>
        /// <param name="vehicleProperties"></param>
        /// <returns></returns>
        public bool DoesVehicleFitIntoRestrictions(VehicleRoadRestrictionProperties vehicleProperties)
        {
            return (vehicleProperties.EpCount <= EpCount &&
                vehicleProperties.Height <= Height &&
                vehicleProperties.Width <= Width &&
                vehicleProperties.GrossVehicleWeight <= GrossVehicleWeight &&
                IsNotLargerType(vehicleProperties) &&
		IsNotMoreRestrictiveTunnelCategory(vehicleProperties.ForbiddenTunnelCategory));
        }

        private bool IsNotMoreRestrictiveTunnelCategory(TunnelCategory other)
        {
	    return (int)this.ForbiddenTunnelCategory >= (int)other;
        }

        /// <summary>
        /// Verfies is other VehicleRoadRestrictionProperties identical to this one
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(VehicleRoadRestrictionProperties other)
        {
            return other.GrossVehicleWeight == this.GrossVehicleWeight && other.Height == this.Height &&
                other.Width == this.Width && other.EpCount == this.EpCount && other.VehicleType == this.VehicleType &&
		this.ForbiddenTunnelCategory == other.ForbiddenTunnelCategory;
        }
        public static VehicleRoadRestrictionProperties GetMaxProfile()
        {
            return new VehicleRoadRestrictionProperties(
                MaxGrossVehicleWeight,
                MaxHeight,
                MaxWidth,
                MaxEPCount,
                VehicleTypeRouting.TractorWithTrailer);
        }
    }
}
