using System;

namespace Suice
{
    /// <summary>
    /// Marked on Constructors and Fields to notify the Injector to inject required dependencies.
    /// 
    /// @author DisTurBinG
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Constructor)]
    public class Inject : Attribute { }
}