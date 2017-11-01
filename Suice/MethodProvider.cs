using System;
using System.Reflection;

namespace Suice
{
    /// <summary>
    /// Method provider provides instances of 
    /// 
    /// @author DisTurBinG
    /// </summary>
    internal class MethodProvider : NoScopeProvider
    {
        private readonly AbstractModule module;
        private readonly MethodInfo methodInfo;

        public MethodProvider(AbstractModule module, MethodInfo methodInfo, Type[] dependencyTypes)
            : base(methodInfo.ReturnType, dependencyTypes)
        {
            this.module = module;
            this.methodInfo = methodInfo;
        }

        public override object Provide()
        {
            return methodInfo.Invoke(module, Dependencies);
        }
    }
}