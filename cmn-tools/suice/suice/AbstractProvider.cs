using System;

namespace CmnTools.Suice
{
	/// <summary>
	/// Interface for creating a Factory
	/// 
	/// @author DisTurBinG
	/// </summary>
	public abstract class AbstractProvider
	{
		protected object[] ConstructorDependencies { get; private set; }

		public readonly Type ProvidedType;

		internal bool IsInitialized;

		protected AbstractProvider(Type providedType)
		{
			ProvidedType = providedType;
		}

		internal virtual void SetConstructorDependencies(object[] constructorDependencies)
		{
			ConstructorDependencies = constructorDependencies;
		}

		internal object Provide()
		{
			return ProvideObject();
		}

		protected abstract object ProvideObject();

	}
}

