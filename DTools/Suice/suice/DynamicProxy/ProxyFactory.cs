using System;
using Castle.DynamicProxy;

namespace DTools.Suice.DynamicProxy
{
    /// <summary>
    /// Creates transparent ProxyInterceptor using Castle's Dynamic ProxyInterceptor which will be used for circular dependencies.
    /// 
    /// @author DisTurBinG
    /// </summary>
    internal class ProxyFactory
    {
        private readonly ProxyGenerator proxyGenerator = new ProxyGenerator();
        
        /// <summary>
        /// Creates a ProxyInterceptor based on given interface interfaceType due to limitations of Castle's Dynamic ProxyInterceptor.
        /// </summary>
        /// <param name="interfaceType">Interface Type to proxy</param>
        /// <param name="proxyInterceptor">ProxyInterceptor Object</param>
        /// <param name="target">ProxyInterceptor Default Target</param>
        /// <returns>Transparent ProxyInterceptor</returns>
        public object CreateTransparentInterfaceProxy(Type interfaceType, out ProxyInterceptor proxyInterceptor, object target = null)
        {
            return proxyGenerator.CreateInterfaceProxyWithTargetInterface(
                interfaceType,
                target,
                proxyInterceptor = new ProxyInterceptor());
        }
    }
}
