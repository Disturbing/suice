using System;

namespace CmnTools.Suice
{
	/// <summary>
	/// Provides no scoped dependencies
	/// 
	/// @author DisTurBinG
	/// </summary>
	public class NoScopeProvider : AbstractProvider
	{
		public NoScopeProvider (Type providedType)
			: base(providedType)
		{

		}

		protected override object ProvideObject ()
		{
			return Activator.CreateInstance (ProvidedType, ConstructorDependencies, null);
		}
	}
}

