using System;

namespace CmnTools.Suice {
    /// <summary>
    /// Marker for interfaces to be registered as dependencies in the Injector.
    /// Specified ImplementedType will determine the implementation
    ///
    /// @author DisTurBinG
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class ImplementedBy : Attribute {
        internal readonly Type ImplementedType;

        public ImplementedBy(Type implementedType) {
            ImplementedType = implementedType;
        }
    }
}