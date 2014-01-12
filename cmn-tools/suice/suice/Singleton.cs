using System;

namespace Toolbox.Injection
{
    /// <summary>
    /// Singleton attribute flag, which marks a class should be instantiated as a singleton instance in the Injector.
    /// 
    /// @author DisTurBinG
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class Singleton : Attribute
    {
    }
}
