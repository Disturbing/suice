using System;

namespace CmnTools.Suice {
    /// <summary>
    /// Binding maps a dependency type to a implementation instance type.
    /// Option to set specific instance to bind to.
    /// Default scope is NO_SCOPE
    /// 
    /// @author DisTurBinG
    /// </summary>
    public class Binding<T> : IBinding {
        public Type TypeToBind { get; private set; }

        public Type BindedType { get; private set; }

        public Scope Scope { get; private set; }

        public object BindedInstance { get; private set; }

        public Binding() {
            TypeToBind = typeof (T);
            Scope = Scope.NO_SCOPE;
            BindedInstance = null;
        }

        public Binding<T> To<V>()
            where V : T {
            BindedType = typeof (V);

            return this;
        }

        public Binding<T> In(Scope scope) {
            Scope = scope;

            return this;
        }

        public Binding<T> ToInstance(T binded) {
            BindedInstance = binded;
            BindedType = binded.GetType();
            Scope = Scope.SINGLETON;
            return this;
        }
    }
}