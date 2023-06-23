using System.Collections.Generic;

namespace VRPTWOptimizer.Interfaces
{
    /// <summary>
    /// Provides new instance of optimizer (follows Abstract Factory desing pattern)
    /// </summary>
    public interface IVRPOptimizerFactory
    {
        /// <summary>
        /// Creates new instance of a VRP optimizer with default parameters
        /// </summary>
        /// <returns></returns>
        IVRPOptimizer CreateOptimizer();

        /// <summary>
        /// Creates new instance of a VRP optimizer
        /// </summary>
        /// <returns></returns>
        IVRPOptimizer CreateOptimizer(Dictionary<string, object> configuration);
    }
}