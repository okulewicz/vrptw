namespace VRPTWOptimizer.Enums
{
    /// <summary>
    /// Type of cargo that would determine if it can be transported with other types
    /// </summary>
    public enum CargoType
    {
        /// <summary>
        /// Food is encoded as 1
        /// </summary>
        Food = 1,
        /// <summary>
        /// Empty coolboxes and other containers are encoded as 2
        /// </summary>
        EmptyBoxes = 2,
        /// <summary>
        /// Garbage is encoded as 3 (it is assumed that food cannot be transported with garbage)
        /// </summary>
        Garbage = 3,
    }
}