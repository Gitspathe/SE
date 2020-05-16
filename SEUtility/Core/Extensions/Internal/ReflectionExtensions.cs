using System;
using System.Reflection;

namespace SE.Core.Extensions.Internal
{
    public static class ReflectionExtensions
    {
        public static bool InheritsFrom(this Type type, Type baseType)
        {
            return type.IsSubclassOf(baseType);
        }

        public static bool InheritsFromGeneric(this Type type, Type baseType, bool includeDeclaringType = false)
        {
            // check all base types
            var currentType = type;
            if (!includeDeclaringType) {
                currentType = currentType.BaseType;
            }
            while (currentType != null) {
                if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == baseType) {
                    return true;
                }
                currentType = currentType.BaseType;
            }
            return false;
        }

        public static Type[] GetParameterTypes(this MethodInfo methodInfo)
        {
            ParameterInfo[] paramsInfo = methodInfo.GetParameters();
            Type[] types = new Type[paramsInfo.Length];
            for (int i = 0; i < types.Length; i++) {
                types[i] = paramsInfo[i].ParameterType;
            }
            return types;
        }
    }
}
