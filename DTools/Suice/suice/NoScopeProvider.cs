using System;

namespace DTools.Suice
{
    /// <summary>
    /// Provides no scoped dependencies
    /// 
    /// @author DisTurBinG
    /// </summary>
    internal class NoScopeProvider : Provider
    {
        public NoScopeProvider(Type providedType, Type[] dependencyTypes)
            : this(providedType, providedType, dependencyTypes) { }

        public NoScopeProvider(Type providedType, Type implementedType, Type[] dependencyTypes)
            : base(providedType, implementedType, dependencyTypes) { }

        public override object Provide()
        {
            return ImplementedType.New(Dependencies);
        }
    }
}