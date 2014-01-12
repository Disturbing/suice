namespace CmnTools.Suice
{
	/// <summary>
	/// Enumeration of Dependency scope types.
	/// 
	/// NO_SCOPE will create a new instance of a dependency everytime it is requested.
	/// SINGLETON will create a single instance and return only that instance when the dependency is requested.
	/// 
	/// @author DisTurBinG
	/// </summary>
    public enum Scope
    {
        NO_SCOPE,
        SINGLETON
    }
}
