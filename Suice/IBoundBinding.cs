namespace Suice
{
    /// <summary>
    /// Exposed optional In scope for configuring dependency scope after they have been bound.
    /// 
    /// @author DisTurBinG
    /// </summary>
    public interface IBoundBinding : IBinding
    {
        IBinding In(Scope scope);
    }
}
