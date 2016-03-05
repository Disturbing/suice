namespace DTools.Suice
{
    /// <summary>
    /// Enumeration of Dependency scope types.
    /// 
    /// @author DisTurBinG
    /// </summary>
    public enum Scope
    {
        /// <summary>
        /// Creates a new instance EVERY TIME the dependency is requested.
        /// </summary>
        NO_SCOPE,
        /// <summary>
        /// will create a single instance and only return that instance when the dependency is requested.
        /// </summary>
        SINGLETON,
        /// <summary>
        /// Create a singleton at the startup of the application, even if it is not a dependency of another type.
        /// </summary>
        EAGER_SINGLETON
    }
}