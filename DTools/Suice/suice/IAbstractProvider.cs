namespace DTools.Suice
{
    /// <summary>
    /// Base class container for all dependencies.  Each dependency has a provider which manages 'providing' the object when requested.
    /// Contains all information, including required dependency instances for the specfic object.
    /// 
    /// @author DisTurBinG
    public interface IProvider
    {
        object Provide();
    }
}