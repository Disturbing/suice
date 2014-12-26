using System;

namespace DTools.Suice
{
    /// <summary>
    /// Base class for all providers
    /// 
    /// @author DisTurBinG
    /// </summary>
    public abstract class AbstractProvider
    {
        protected object[] Dependencies { get; private set; }

        public readonly Type ProvidedType;

        public readonly Type ImplementedType;

        internal bool IsInitialized;

        protected AbstractProvider(Type providedType, Type implementedType)
        {
            ProvidedType = providedType;
            ImplementedType = implementedType;
        }

        internal void SetDependencies(object[] constructorDependencies)
        {
            Dependencies = constructorDependencies;
        }

        internal object Provide()
        {
            return ProvideObject();
        }

        protected abstract object ProvideObject();
    }
}