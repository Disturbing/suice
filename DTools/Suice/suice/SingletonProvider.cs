using System;

namespace DTools.Suice
{
    /// <summary>
    /// Container for a singleton instance
    /// 
    /// @author DisTurBinG
    /// </summary>
    internal class SingletonProvider : Provider
    {
        public readonly Scope Scope;
        internal object Instance;

        public SingletonProvider(Scope scope, Type providedType, Type[] dependencyTypes)
            : this(scope, providedType, providedType, dependencyTypes) { }

        public SingletonProvider(Scope scope, Type providedType, Type implementedType, Type[] dependencyTypes, object defualtInstance = null)
            : base(providedType, implementedType, dependencyTypes)
        {
            Scope = scope;
            Instance = defualtInstance;
        }

        public override object Provide()
        {
            return Instance ?? (Instance = ImplementedType.New(Dependencies));
        }
    }
}