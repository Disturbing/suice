using System;

namespace DTools.Suice
{
    /// <summary>
    /// Dynamic provider is instantiated to create a factory pattern for specific instances who need injectable dependencies.
    /// 
    /// @author DisTurBinG
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DynamicProvider<T> : NoScopeProvider, IProvider<T> where T : class
    {
        public DynamicProvider(Type providedType, Type implementedType, Type[] dependencyTypes)
            : base(providedType, implementedType, dependencyTypes) { }

        T IProvider<T>.Provide()
        {
            T instance = (T) base.Provide();

            if (instance is IInitializable) {
                ((IInitializable)instance).Initialize();
            }

            return instance;
        }

        public override object Provide()
        {
            return this;
        }
    }
}
