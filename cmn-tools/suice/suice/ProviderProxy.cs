using System;

namespace CmnTools.Suice {
    /// <summary>
    /// @author DisTurBinG
    /// </summary>
    public class ProviderProxy : AbstractProvider {
        private AbstractProvider provider;

        public readonly Type ProviderType;

        public ProviderProxy(Type providedType, Type providerType)
            : base(providedType, providedType) {
            ProviderType = providerType;
        }


        internal void SetProviderInstance(AbstractProvider provider) {
            if (!IsInitialized) {
                this.provider = provider;
                IsInitialized = true;
            } else {
                throw new Exception("Attempted to SetProviderInstance to ProviderProxy Twice!");
            }
        }

        protected override object ProvideObject() {
            return provider.Provide();
        }
    }
}