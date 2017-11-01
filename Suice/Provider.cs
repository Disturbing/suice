using System;

namespace Suice
{
    /// <summary>
    /// Base class container for all dependencies.  Each dependency has a provider which manages 'providing' the object when requested.
    /// Contains all information, including required dependency instances for the specfic object.
    /// 
    /// @author DisTurBinG
    /// </summary>
    internal abstract class Provider : IProvider
    {
        public readonly object[] Dependencies;
        public readonly Type ProvidedType;
        public readonly Type ImplementedType;
        public readonly Type[] DependencyTypes; 

        internal bool IsInitialized;

        protected Provider(Type providedType, Type implementedType, Type[] dependencyTypes)
        {
            ProvidedType = providedType;
            ImplementedType = implementedType;
            DependencyTypes = dependencyTypes;
            Dependencies = new object[dependencyTypes.Length];
        }

        public abstract object Provide();
    }
}