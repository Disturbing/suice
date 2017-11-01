using System;

namespace Suice
{
    /// <summary>
    /// Factory marks methods in Modules to specify methods that provide dependencies
    /// 
    /// @author DisTurBinG
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class Provides : Attribute
    {
        internal readonly Scope Scope;

        public Provides(Scope scope) {
            Scope = scope;
        }
    }
}