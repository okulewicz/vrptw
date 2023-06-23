using CommonGIS.Interfaces;
using System;

namespace VRPTWOptimizer.Interfaces
{
    /// <summary>
    /// Optimizes Vehicle Routing Problem
    /// </summary>
    public interface IVRPOptimizer
    {
        /// <summary>
        /// Solves Vehicle Routing Problem
        /// </summary>
        /// <param name="problemDataProvider"></param>
        /// <param name="serviceTimeEstimator"></param>
        /// <param name="distanceProvider"></param>
        /// <returns></returns>
        [Obsolete("Please use Optimize with conscious costFunctionFactors definition")]
        VRPOptimizerResult Optimize(
           IVRPProvider problemDataProvider,
           ITimeEstimator serviceTimeEstimator,
           IDistanceProvider distanceProvider);

        /// <summary>
        /// Solves Vehicle Routing Problem
        /// </summary>
        /// <param name="problemDataProvider"></param>
        /// <param name="costFunctionFactors"></param>
        /// <param name="serviceTimeEstimator"></param>
        /// <param name="distanceProvider"></param>
        /// <returns></returns>
        VRPOptimizerResult Optimize(
           IVRPProvider problemDataProvider,
           VRPCostFunction costFunctionFactors,
           ITimeEstimator serviceTimeEstimator,
           IDistanceProvider distanceProvider);
    }
}