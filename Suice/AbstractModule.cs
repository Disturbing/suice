using System.Collections.Generic;

namespace Suice
{
    /// <summary>
    /// Implement Abstract Module to register custom binding rules
    /// 
    /// @author DisTurBinG
    /// </summary>
    public abstract class AbstractModule
    {
        private Injector injector;

        internal HashSet<IBinding> Bindings = new HashSet<IBinding>();

        protected IConfigurableBinding<T> Bind<T>()  where T : class
        {
            Binding<T> binding = new Binding<T>();
            Bindings.Add(binding);
            return binding;
        }

        protected internal virtual void Configure() { }
    }
}