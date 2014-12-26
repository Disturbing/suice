using System;

namespace DTools.Suice
{
    /// <summary>
    /// Manually specified binding interface to manually specify a dependency configuration.
    /// 
    /// @author DisTurBinG
    /// </summary>
    public interface IBinding
    {
        Type TypeToBind { get; }
        Type BindedType { get; }
        Scope Scope { get; }
        object BindedInstance { get; }
    }
}