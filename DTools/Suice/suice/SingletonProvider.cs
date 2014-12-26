using System;

namespace DTools.Suice
{
    /// <summary>
    /// Container for a singleton instance
    /// 
    /// @author DisTurBinG
    /// </summary>
    public class SingletonProvider : AbstractProvider
    {
        internal object Instance;

        public SingletonProvider(Type providedType)
            : this(providedType, providedType) { }

        public SingletonProvider(Type providedType, Type implementedType)
            : base(providedType, implementedType) { }

        internal virtual void CreateSingletonInstance()
        {
            if (Instance == null) {
                SetInstance(Activator.CreateInstance(ImplementedType, Dependencies));
            }
        }

        internal void SetInstance(object instance)
        {
            Instance = instance;
        }

        protected override object ProvideObject()
        {
            return Instance;
        }
    }
}