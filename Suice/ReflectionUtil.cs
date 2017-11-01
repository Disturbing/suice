using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Suice
{
    /// <summary>
    /// Extensions for attribute and method information through Reflection
    /// 
    /// @author DisTurBinG
    /// </summary>
    internal static class ReflectionExtensions
    {
        public static IEnumerable<MethodInfo> GetMethodsWithAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetMethods().Where(mi => Attribute.GetCustomAttribute(mi, typeof (T)) != null);
        }

        public static T GetTypeAttribute<T>(this Type type, bool inherit = false) where T : Attribute
        {
            T typeAttribute = type.GetCustomAttributes(inherit).OfType<T>().FirstOrDefault();

            if (typeAttribute == null && inherit) {
                Type interfaceWithAttribute =
                    type.GetInterfaces().FirstOrDefault(i => i.GetTypeAttribute<T>(true) != null);
                
                if (interfaceWithAttribute != null) {
                    typeAttribute = interfaceWithAttribute.GetTypeAttribute<T>(true);
                }
            }

            return typeAttribute;
        }

        public static T GetMemberInfoAttribute<T>(this MemberInfo memberInfo, bool inherit = false) where T : Attribute
        {
            return memberInfo.GetCustomAttributes(inherit).OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Credit: http://stackoverflow.com/questions/5461295/using-isassignablefrom-with-generics
        /// </summary>
        /// <param name="givenType"></param>
        /// <param name="genericType"></param>
        /// <returns></returns>
        public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = givenType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, genericType);
        }
    }
}