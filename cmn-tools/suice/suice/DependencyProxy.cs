using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;
using System.Reflection;
using System;

namespace Toolbox.Injection
{
    /// <summary>
    /// TODO: This will not work! Need new proxy solution
    /// @author DisTurBinG
    /// </summary>
	internal class DependencyProxy : RealProxy
	{
		internal object instance;

		internal DependencyProxy(Type proxyType)
			: base(proxyType)
		{
		}

        internal void SetInstance(object instance)
		{
			this.instance = instance;
		}

		public override IMessage Invoke(IMessage msg)
		{
			IMethodCallMessage methodCall = (IMethodCallMessage)msg;
			MethodInfo method = (MethodInfo)methodCall.MethodBase;

			try
			{
				object result = method.Invoke(instance, methodCall.InArgs);
				return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
			}
			catch (Exception e)
			{
				if (e is TargetInvocationException && e.InnerException != null)
				{
					return new ReturnMessage(e.InnerException, msg as IMethodCallMessage);
				}

				return new ReturnMessage(e, msg as IMethodCallMessage);
			}
		}
	}
}