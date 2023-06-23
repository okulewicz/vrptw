using CommonGIS;
using CommonGIS.Enums;
using CommonGIS.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using VRPTWOptimizer.Enums;
using VRPTWOptimizer.Interfaces;

namespace VRPTWOptimizer
{
    /// <summary>
    /// Description of the request to move cargo from pickup to delivery Location
    /// </summary>
    public abstract class TransportRequest
    {
        /// <summary>
        /// Main cargo type
        /// </summary>
        public int CargoType => CargoTypes.FirstOrDefault();
        /// <summary>
        /// All cargo types within request
        /// </summary>
        public int[] CargoTypes { get; protected set; }
        /// <summary>
        /// Time window end while request can still be delivered
        /// </summary>
        public double DeliveryAvailableTimeWindowEnd { get; protected set; }
        /// <summary>
        /// Earlies time window start when request can be delivered
        /// </summary>
        public double DeliveryAvailableTimeWindowStart { get; protected set; }
        /// <summary>
        /// Cargo destination
        /// </summary>
        public Location DeliveryLocation { get; protected set; }

        /// <summary>
        /// Delivery time window end
        /// </summary>
        public double DeliveryPreferedTimeWindowEnd { get; protected set; }

        /// <summary>
        /// Delivery time window start
        /// </summary>
        public double DeliveryPreferedTimeWindowStart { get; protected set; }
        /// <summary>
        /// Numeric request identifier
        /// </summary>
        public int Id { get; protected set; }
        /// <summary>
        /// Upper bound restrictions of vehicle parameters
        /// </summary>
        public VehicleRoadRestrictionProperties MaxVehicleSize { get; protected set; }
        /// <summary>
        /// Dictionary of requests that can be visited only after certain time of departure from end location
        /// or must be served certain time before arrival to end location
        /// </summary>
        public Dictionary<int, double> MutuallyExclusiveRequestsIdTimeBufferDict { get; protected set; }
        /// <summary>
        /// Custom request identifier
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// List of domain specific features that the vehicle needs to have to serve this request (e.g. freezer, lift)
        /// vector needs to conform to Vehicle.SpecialProperties
        /// </summary>
        public int[] NecessaryVehicleSpecialProperties { get; protected set; }
        /// <summary>
        /// Total count of items to be delivered
        /// (e.g. Euro Pallet and DHP pallet are both a single item)
        /// </summary>
        public int PackageCount { get; protected set; }

        /// <summary>
        /// Subgroup of PackageCount property which are to be retrieved just after delivery
        /// </summary>
        public int PackageCountForImediateRetrieval { get; protected set; }

        /// <summary>
        /// Time window end while request can still be picked up
        /// </summary>
        public double PickupAvailableTimeWindowEnd { get; protected set; }

        /// <summary>
        /// Earlies time window start when request can be picked up
        /// </summary>
        public double PickupAvailableTimeWindowStart { get; protected set; }

        /// <summary>
        /// Location where the cargo is picked up
        /// </summary>
        public Location PickupLocation { get; protected set; }

        /// <summary>
        /// Time window end when request should be picked up
        /// </summary>
        public double PickupPreferedTimeWindowEnd { get; protected set; }

        /// <summary>
        /// Time window start when request should be picked up
        /// </summary>
        public double PickupPreferedTimeWindowStart { get; protected set; }

        /// <summary>
        /// Cargo types identifiers that could not be transported together with this request
        /// </summary>
        public int[] RestrictedCargoTypes { get; protected set; }

        /// <summary>
        /// Additional value gained if request is completed
        /// </summary>
        public double RevenueValue { get; protected set; }

        /// <summary>
        /// Total cargo size (Euro pallets count, mass, cubic meters etc.)
        /// Must conform to Vehicle capacity definition
        /// </summary>
        public double[] Size { get; protected set; }
        /// <summary>
        /// Type of request: distribution, backhauling etc.
        /// </summary>
        public RequestType Type { get; protected set; }

        /// <summary>
        /// Creates generic TransportRequest when class is inherited from it will have to use this to define problem
        /// </summary>
        /// <param name="id"></param>
        /// <param name="size"></param>
        /// <param name="necessaryVehicleSpecialProperties"></param>
        /// <param name="packageCount"></param>
        /// <param name="packageCountForImediateRetrieval"></param>
        /// <param name="startLocation"></param>
        /// <param name="pickupAvailableTimeWindowStart"></param>
        /// <param name="pickupPreferedTimeWindowStart"></param>
        /// <param name="pickupPreferedTimeWindowEnd"></param>
        /// <param name="pickupAvailableTimeWindowEnd"></param>
        /// <param name="endLocation"></param>
        /// <param name="deliveryAvailableTimeWindowStart"></param>
        /// <param name="deliveryPreferedTimeWindowStart"></param>
        /// <param name="deliveryPreferedTimeWindowEnd"></param>
        /// <param name="deliveryAvailableTimeWindowEnd"></param>
        /// <param name="requestType"></param>
        /// <param name="cargoTypes"></param>
        /// <param name="maxVehicleSize"></param>
        /// <param name="restrictedGoodsTypes"></param>
        /// <param name="mutuallyExclusiveRequestsIdTimeBufferDict"></param>
        /// <param name="revenueValue"></param>
        /// <param name="name"></param>
        public TransportRequest(int id,
                                double[] size,
                                int[] necessaryVehicleSpecialProperties,
                                int packageCount,
                                int packageCountForImediateRetrieval,
                                Location startLocation,
                                double pickupAvailableTimeWindowStart,
                                double pickupPreferedTimeWindowStart,
                                double pickupPreferedTimeWindowEnd,
                                double pickupAvailableTimeWindowEnd,
                                Location endLocation,
                                double deliveryAvailableTimeWindowStart,
                                double deliveryPreferedTimeWindowStart,
                                double deliveryPreferedTimeWindowEnd,
                                double deliveryAvailableTimeWindowEnd,
                                RequestType requestType,
                                int[] cargoTypes,
                                VehicleRoadRestrictionProperties maxVehicleSize,
                                int[] restrictedGoodsTypes,
                                Dictionary<int, double> mutuallyExclusiveRequestsIdTimeBufferDict,
                                double revenueValue,
                                string name)
        {
            Size = new double[size.Length];
            Array.Copy(size, Size, size.Length);
            CargoTypes = new int[cargoTypes.Length];
            Array.Copy(cargoTypes, CargoTypes, cargoTypes.Length);

            RestrictedCargoTypes = new int[restrictedGoodsTypes.Length];
            Array.Copy(restrictedGoodsTypes, restrictedGoodsTypes, restrictedGoodsTypes.Length);
            PackageCount = packageCount;
            PickupLocation = startLocation;
            DeliveryLocation = endLocation;
            DeliveryPreferedTimeWindowStart = deliveryPreferedTimeWindowStart;
            DeliveryPreferedTimeWindowEnd = deliveryPreferedTimeWindowEnd;
            MaxVehicleSize = maxVehicleSize;
            Type = requestType;
            PackageCountForImediateRetrieval = packageCountForImediateRetrieval;
            Name = name;
            Id = id;
            MutuallyExclusiveRequestsIdTimeBufferDict = mutuallyExclusiveRequestsIdTimeBufferDict;
            DeliveryAvailableTimeWindowEnd = deliveryAvailableTimeWindowEnd;
            DeliveryAvailableTimeWindowStart = deliveryAvailableTimeWindowStart;
            PickupAvailableTimeWindowEnd = pickupAvailableTimeWindowEnd;
            PickupAvailableTimeWindowStart = pickupAvailableTimeWindowStart;
            PickupPreferedTimeWindowEnd = pickupPreferedTimeWindowEnd;
            PickupPreferedTimeWindowStart = pickupPreferedTimeWindowStart;
            RevenueValue = revenueValue;
            NecessaryVehicleSpecialProperties = necessaryVehicleSpecialProperties;
        }

        internal bool IsArrivalFeasible(double arrivalTimeAtRequest)
        {
            return arrivalTimeAtRequest >= this.DeliveryPreferedTimeWindowStart && arrivalTimeAtRequest <= this.DeliveryPreferedTimeWindowEnd;
        }

        /// <summary>
        /// Find requests that should be paired with this TransportRequest
        /// </summary>
        /// <param name="requests"></param>
        /// <param name="timeEstimator"></param>
        /// <param name="distanceProvider"></param>
        /// <returns></returns>
        public List<TransportRequest> ExtractBestFitRequests(List<TransportRequest> requests, ITimeEstimator timeEstimator, IDistanceProvider distanceProvider)
        {
            VehicleRoadRestrictionProperties defaultVehicleProperties = new VehicleRoadRestrictionProperties(
                VehicleRoadRestrictionProperties.MaxGrossVehicleWeight, 0, 0, 0, VehicleTypeRouting.TractorWithTrailer);
            var bestFitRequests = new List<TransportRequest>();
            double bestbackhaulingDistance = double.PositiveInfinity;
            foreach (TransportRequest request in requests)
            {
                if (this.Type == RequestType.Backhauling && request.Type == RequestType.GoodsDistribution)
                {
                    Distance distanceBetween = distanceProvider
                        .GetDistance(request.DeliveryLocation, this.PickupLocation, defaultVehicleProperties);
                    Distance backhaulingDistance = distanceProvider
                        .GetDistance(this.PickupLocation, this.DeliveryLocation, defaultVehicleProperties);
                    double unloadOldGoodsServiceTime = timeEstimator.EstimateLoadUnloadTime(request.PackageCount, 0, 0, 1);
                    double loadNewGoodsServiceTime = timeEstimator.EstimateLoadUnloadTime(0, this.PackageCount, 0, 1);
                    double arrivalTime = request.DeliveryPreferedTimeWindowStart + unloadOldGoodsServiceTime + distanceBetween.Time + loadNewGoodsServiceTime + backhaulingDistance.Time;
                    if (arrivalTime < this.DeliveryPreferedTimeWindowEnd && request.MaxVehicleSize.DoesVehicleFitIntoRestrictions(this.MaxVehicleSize) && this.MaxVehicleSize.DoesVehicleFitIntoRestrictions(request.MaxVehicleSize))
                    {
                        if (distanceBetween.Length < bestbackhaulingDistance)
                        {
                            bestFitRequests.Clear();
                            bestbackhaulingDistance = distanceBetween.Length;
                            bestFitRequests.Add(request);
                        }
                    }
                }
                else if (this.Type == RequestType.GoodsDistribution && request.Type == RequestType.ContainerRetrieval)
                {
                    Distance distance = distanceProvider
                        .GetDistance(this.DeliveryLocation, request.PickupLocation, defaultVehicleProperties);
                    if (distance.Length == 0)
                    {
                        if (this.DeliveryPreferedTimeWindowStart <= request.PickupPreferedTimeWindowEnd)
                        {
                            bestFitRequests.Add(request);
                        }
                    }
                }
            }
            return bestFitRequests;
        }
    }
}