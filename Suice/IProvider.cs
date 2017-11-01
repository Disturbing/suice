namespace Suice
{
    /// <summary>
    /// Base class container for all dependencies.  Each dependency has a provider which manages 'providing' the object when requested.
    /// Contains all information, including required dependency instances for the specfic object.
    /// 
    /// @author DisTurBinG
    internal interface IProvider
    {
        object Provide();
    }

    public interface IProvider<T> where T : class
    {
        T Provide();
    }
}