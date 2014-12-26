namespace DTools.Suice
{
    /// <summary>
    /// Configurable blinding requires you to set an active instance or specificy an instance type for your dependency.
    /// 
    /// @author DisTurBinG
    /// </summary>
    public interface IConfigurableBinding<T>
    {
        IBoundBinding To<V>() where V : T;
        IBoundBinding ToInstance(T binded);
    }
}
