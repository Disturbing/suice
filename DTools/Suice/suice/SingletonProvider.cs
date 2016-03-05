using System;

namespace DTools.Suice
{
    /// <summary>
    /// Container for a singleton instance
    /// 
    /// @author DisTurBinG
    /// </summary>
    public class SingletonProvider : Provider
    {
        internal object Instance;

        public SingletonProvider(Type providedType)
            : this(providedType, providedType) { }

        public SingletonProvider(Type providedType, Type implementedType, object defualtInstance = null)
            : base(providedType, implementedType)
        {
            Instance = defualtInstance;
        }

        public override object Provide()
        {
            return Instance ?? (Instance = Activator.CreateInstance(ImplementedType, Dependencies));
        }
    }
}