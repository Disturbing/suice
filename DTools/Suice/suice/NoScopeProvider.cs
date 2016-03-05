using System;

namespace DTools.Suice
{
    /// <summary>
    /// Provides no scoped dependencies
    /// 
    /// @author DisTurBinG
    /// </summary>
    public class NoScopeProvider : Provider
    {
        public NoScopeProvider(Type providedType)
            : this(providedType, providedType) { }

        public NoScopeProvider(Type providedType, Type implementedType)
            : base(providedType, implementedType) { }

        public override object Provide()
        {
            return Activator.CreateInstance(ImplementedType, Dependencies);
        }
    }
}