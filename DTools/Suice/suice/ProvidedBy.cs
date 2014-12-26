using System;

namespace DTools.Suice
{
    /// <summary>
    /// Attribute used to mark interfaces for dependency injection using the factory pattern
    /// 
    /// @author DisTurBinG
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class ProvidedBy : Attribute
    {
        internal readonly Type ProviderType;

        public ProvidedBy(Type providerType)
        {
            ProviderType = providerType;
        }
    }
}