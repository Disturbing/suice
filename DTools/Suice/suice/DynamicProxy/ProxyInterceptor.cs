using Castle.DynamicProxy;

namespace DTools.Suice.DynamicProxy
{
    /// <summary>
    /// Handles overriding calls and forwarding them to proxie's designated target.
    /// Used as a container to set target dynamically.
    /// 
    /// @author DisTurBinG
    /// </summary>
    public class ProxyInterceptor : IInterceptor
    {
        private class ProxyNotInitializedException : System.Exception { }

        private object proxyTarget;

        public void Initialize(object target)
        {
            this.proxyTarget = target;
        }
    
        public void Intercept(IInvocation invocation)
        {
            if (proxyTarget != null) {
                ((IChangeProxyTarget) invocation).ChangeInvocationTarget(proxyTarget);
                invocation.Proceed();
            } else {
                throw new ProxyNotInitializedException();
            }
        }
    }
}
