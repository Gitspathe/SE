using System;
using System.Collections.Generic;
using System.Text;

namespace DeeZ.Core.Extensions.Internal
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
    }
}
