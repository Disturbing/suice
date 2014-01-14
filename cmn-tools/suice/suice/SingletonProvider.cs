using System;

namespace CmnTools.Suice
{
    /// <summary>
    /// @author DisTurBinG
    /// </summary>
	public class SingletonProvider : AbstractProvider
	{
		internal object Instance;

		public SingletonProvider (Type providedType)
			: base(providedType)
		{

		}

        internal virtual void CreateSingletonInstance()
        {
            if (Instance == null)
            {
                SetInstance(Activator.CreateInstance(ProvidedType, ConstructorDependencies));
            }
        }

        internal void SetInstance (object instance)
        {
            DependencyProxy dependencyProxy = Instance as DependencyProxy;

            if (dependencyProxy == null)
            {
                Instance = instance;
            }
            else
            {
                dependencyProxy.SetInstance(instance);
            }
        }

		protected override object ProvideObject()
		{
            return Instance ?? (Instance = new DependencyProxy (ProvidedType));
		}
	}
}

