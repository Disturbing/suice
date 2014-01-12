using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace CmnTools.Suice
{
	/// <summary>
	/// Helper class with reflection
	/// 
	/// @author DisTurBinG
	/// </summary>
	public static class ReflectionUtil
	{
		public static IEnumerable<MethodInfo> GetMethodsWithAttribute<T>(this Type type)
			where T : Attribute
		{
			return type.GetMethods().Where(mi => Attribute.GetCustomAttribute(mi, typeof(T)) != null);
		}

		public static T GetTypeAttribute<T>(this Type type, bool inherit = false)
			where T : Attribute
		{
			return type.GetCustomAttributes(inherit).OfType<T>().FirstOrDefault(); 
		}

		public static T GetMemberInfoAttribute<T>(this MemberInfo memberInfo, bool inherit = false)
			where T : Attribute
		{
			return memberInfo.GetCustomAttributes (inherit).OfType<T> ().FirstOrDefault ();
		}
	}
}

