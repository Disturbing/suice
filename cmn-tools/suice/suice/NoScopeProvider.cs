using System;

namespace CmnTools.Suice {
    /// <summary>
    /// Provides no scoped dependencies
    /// 
    /// @author DisTurBinG
    /// </summary>
    public class NoScopeProvider : AbstractProvider {
        public NoScopeProvider(Type providedType)
            : this(providedType, providedType) {
        }

        public NoScopeProvider(Type providedType, Type implementedType)
            : base(providedType, implementedType) {
        }

        protected override object ProvideObject() {
            return Activator.CreateInstance(ImplementedType, ConstructorDependencies);
        }
    }
}