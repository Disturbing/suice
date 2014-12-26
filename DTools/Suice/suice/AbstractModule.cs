using System.Collections.Generic;

namespace DTools.Suice
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

        protected IConfigurableBinding<T> Bind<T>() 
        {
            Binding<T> binding = new Binding<T>();
            Bindings.Add(binding);
            return binding;
        }

        public abstract void Configure();
    }
}