using System;

namespace CmnTools.Suice {
    /// <summary>
    /// @author DisTurBinG
    /// </summary>
    public class SingletonProvider : AbstractProvider {
        internal object Instance;

        public SingletonProvider(Type providedType)
            : this(providedType, providedType) {
        }

        public SingletonProvider(Type providedType, Type implementedType)
            : base(providedType, implementedType) {
        }

        internal virtual void CreateSingletonInstance() {
            if (Instance == null) {
                SetInstance(Activator.CreateInstance(ImplementedType, ConstructorDependencies));
            } else {
                throw new Exception("Attempted to create singleton instance twice for SingletonProvider: " +
                                    ProvidedType);
            }
        }

        internal void SetInstance(object instance) {
            Instance = instance;
        }

        protected override object ProvideObject() {
            return Instance;
        }
    }
}