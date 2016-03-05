using System.Reflection;

namespace DTools.Suice
{
    /// <summary>
    /// Singleton Method Factory provides dependencies from AbstractModule's @Provides methods with Singleton Scope
    /// 
    /// @author DisTurBinG
    /// </summary>
    public class SingletonMethodProvider : SingletonProvider, IMethodProvider
    {
        private readonly AbstractModule module;
        private readonly MethodInfo methodInfo;

        public SingletonMethodProvider(AbstractModule module, MethodInfo methodInfo)
            : base(methodInfo.ReturnType)
        {
            this.module = module;
            this.methodInfo = methodInfo;
        }

        public MethodInfo GetMethod()
        {
            return methodInfo;
        }

        public override object Provide()
        {
            return Instance ?? (Instance = methodInfo.Invoke(module, Dependencies));
        }
    }
}