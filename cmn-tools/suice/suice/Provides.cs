using System;

namespace CmnTools.Suice {
    /// <summary>
    /// Provider marks methods in Modules to provide instance a specific instance
    /// 
    /// @author DisTurBinG
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class Provides : Attribute {
        internal readonly Scope Scope;

        public Provides(Scope scope) {
            Scope = scope;
        }
    }
}