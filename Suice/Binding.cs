using System;

namespace Suice
{
    /// <summary>
    /// Binding maps a dependency type to a implementation instance type.
    /// Use ToInstance to manually map an instance for the type to be bound to.
    /// Default scope is NO_SCOPE
    /// 
    /// @author DisTurBinG
    /// </summary>
    internal class Binding<T> : IBoundBinding, IConfigurableBinding<T> where T : class
    {
        public Type TypeToBind { get; private set; }

        public Type BindedType { get; private set; }

        public Scope Scope { get; private set; }

        public object BindedInstance { get; private set; }

        public Binding()
        {
            TypeToBind = typeof (T);
            Scope = Scope.NO_SCOPE;
            BindedInstance = null;
        }

        public IBoundBinding To<V>() where V : T
        {
            BindedType = typeof (V);

            return this;
        }

        public IBinding ToInstance(T binded)
        {
            BindedInstance = binded;
            BindedType = binded.GetType();
            Scope = Scope.SINGLETON;
            return this;
        }

        public IBinding In(Scope scope)
        {
            Scope = scope;
            return this;
        }
    }
}