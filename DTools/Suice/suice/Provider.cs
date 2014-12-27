using System;

namespace DTools.Suice
{
    /// <summary>
    /// Base class container for all dependencies.  Each dependency has a provider which manages 'providing' the object when requested.
    /// Contains all information, including required dependency instances for the specfic object.
    /// 
    /// @author DisTurBinG
    /// </summary>
    public abstract class Provider : IProvider
    {
        protected object[] Dependencies { get; private set; }

        public readonly Type ProvidedType;

        public readonly Type ImplementedType;

        internal bool IsInitialized;

        protected Provider(Type providedType, Type implementedType)
        {
            ProvidedType = providedType;
            ImplementedType = implementedType;
        }

        internal void SetDependencies(object[] constructorDependencies)
        {
            Dependencies = constructorDependencies;
        }

        public object Provide()
        {
            return ProvideObject();
        }

        protected abstract object ProvideObject();
    }
}