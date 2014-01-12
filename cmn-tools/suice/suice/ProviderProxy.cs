using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CmnTools.Suice
{
    /// <summary>
    /// @author DisTurBinG
    /// </summary>
    public class ProviderProxy : AbstractProvider
    {
        private readonly AbstractProvider provider;

        public ProviderProxy(AbstractProvider provider, Type providedType)
            : base(providedType)
        {
            this.provider = provider;
        }

        protected override object ProvideObject()
        {
            return ((AbstractProvider)provider.Provide()).Provide();
        }
    }
}
