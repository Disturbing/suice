using System;
using System.Reflection;

namespace DTools.Suice
{
    /// <summary>
    /// Singleton Method Factory provides dependencies from AbstractModule's @Provides methods with Singleton Scope
    /// 
    /// @author DisTurBinG
    /// </summary>
    internal class SingletonMethodProvider : SingletonProvider
    {
        private readonly AbstractModule module;
        private readonly MethodInfo providerMethod;

        public SingletonMethodProvider(Scope scope, AbstractModule module, MethodInfo providerMethod, Type[] dependencyTypes)
            : base(scope, providerMethod.ReturnType, dependencyTypes)
        {
            this.module = module;
            this.providerMethod = providerMethod;
        }
        public override object Provide()
        {
            return Instance ?? (Instance = providerMethod.Invoke(module, Dependencies));
        }
    }
}