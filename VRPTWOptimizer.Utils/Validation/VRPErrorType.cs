namespace VRPTWOptimizer.Utils.Validation
{
    public enum VRPErrorType
    {
        /// <summary>
        /// When one of object properties does not match the other or its logical bounds
        /// </summary>
        ImprobableValue,
        /// <summary>
        /// When one of object properties makes it impossible to be part of solution
        /// </summary>
        ImpossibleProblem,
        /// <summary>
        /// When solution is physically impossible or violates strict constraints
        /// </summary>
        InfeasibleSolution,
    }
}