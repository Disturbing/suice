using System;
using System.Reflection;

namespace CmnTools.Suice {
    /// <summary>
    /// Method provider.
    /// </summary>
    public class MethodProvider : NoScopeProvider, IMethodConstructor {
        private readonly AbstractModule module;
        private readonly MethodInfo methodInfo;

        public MethodProvider(AbstractModule module, MethodInfo methodInfo)
            : base(methodInfo.ReturnType) {
            this.module = module;
            this.methodInfo = methodInfo;
        }

        protected override object ProvideObject() {
            return methodInfo.Invoke(module, ConstructorDependencies);
        }

        public MethodInfo GetMethodConstructor() {
            return methodInfo;
        }
    }
}