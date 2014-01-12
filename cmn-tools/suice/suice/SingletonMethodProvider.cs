using System;
using System.Reflection;

namespace Toolbox.Injection
{
	/// <summary>
	/// Singleton Method Provider provides dependencies from AbstractModule's @Provides methods with Singleton Scope
	/// 
	/// @author DisTurBinG
	/// </summary>
    public class SingletonMethodProvider : SingletonProvider, IMethodConstructor
	{
		private readonly AbstractModule module; 
		private readonly MethodInfo methodInfo;

		public SingletonMethodProvider (AbstractModule module, MethodInfo methodInfo)
			: base(methodInfo.ReturnType)
		{
			this.module = module;
			this.methodInfo = methodInfo;
		}

        protected override object ProvideObject()
        {
            return Instance ?? (Instance = methodInfo.Invoke(module, ConstructorDependencies));
        }

	    public MethodInfo GetMethodConstructor()
	    {
	        return methodInfo;
	    }

        internal override void CreateSingletonInstance()
        {
            // Do Nothing
        }
	}
}

