using System;

namespace DTools.Suice
{
    /// <summary>
    /// Container for the Providers to be injected into dependencies for the use of the Factory Method.
    /// 
    /// @author DisTurBinG
    /// </summary>
    public class ProviderProxy : AbstractProvider
    {
        private AbstractProvider provider;

        public readonly Type ProviderType;

        public ProviderProxy(Type providedType, Type providerType)
            : base(providedType, providedType)
        {
            ProviderType = providerType;
        }


        internal void SetProviderInstance(AbstractProvider provider)
        {
            if (!IsInitialized) {
                this.provider = provider;
                IsInitialized = true;
            }
        }

        protected override object ProvideObject()
        {
            return provider.Provide();
        }
    }
}