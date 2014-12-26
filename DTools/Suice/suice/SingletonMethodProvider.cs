using System.Reflection;

namespace DTools.Suice
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

        public SingletonMethodProvider(AbstractModule module, MethodInfo methodInfo)
            : base(methodInfo.ReturnType)
        {
            this.module = module;
            this.methodInfo = methodInfo;
        }

        public MethodInfo GetMethodConstructor()
        {
            return methodInfo;
        }

        internal override void CreateSingletonInstance()
        {
            if (Instance == null) {
                Instance = methodInfo.Invoke(module, Dependencies);
            }
        }
    }
}