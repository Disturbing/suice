using System.Reflection;

namespace DTools.Suice
{
    /// <summary>
    /// Method provider provides instances of 
    /// 
    /// @author DisTurBinG
    /// </summary>
    public class MethodProvider : NoScopeProvider, IMethodProvider
    {
        private readonly AbstractModule module;
        private readonly MethodInfo methodInfo;

        public MethodProvider(AbstractModule module, MethodInfo methodInfo)
            : base(methodInfo.ReturnType)
        {
            this.module = module;
            this.methodInfo = methodInfo;
        }

        public override object Provide()
        {
            return methodInfo.Invoke(module, Dependencies);
        }

        public MethodInfo GetMethod()
        {
            return methodInfo;
        }
    }
}