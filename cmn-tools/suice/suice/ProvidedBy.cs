using System;

namespace Toolbox.Injection
{
	/// <summary>
	/// @author DisTurBinG
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface)]
	public class ProvidedBy : Attribute
	{
		internal readonly Type ProviderType;

		public ProvidedBy (Type providerType)
		{
			ProviderType = providerType;
		}
	}
}

