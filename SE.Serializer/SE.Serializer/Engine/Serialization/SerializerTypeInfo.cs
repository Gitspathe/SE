using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using SE.Serialization.Exceptions;

namespace SE.Serialization
{
    public class SerializerTypeInfo
    {
        public Type Type { get; }
        public bool IsObject { get; }
        public bool IsValueType { get; }
        public bool IsNullable { get; }
        public bool PossiblePolymorphism { get; }
        public bool PossibleReferencing { get; }
        public object DefaultInstance { get;}

        internal SerializerTypeInfo(Type type)
        {
            Type = type;

            if (!IsPermittedType()) {
                throw new SerializerWhitelistException(type);
            }

            IsValueType = type.IsValueType;
            IsNullable = Nullable.GetUnderlyingType(type) != null;
            PossiblePolymorphism = !IsValueType && !IsNullable;
            PossibleReferencing = !IsValueType;

            try {
                if (Type == typeof(object)) {
                    IsObject = true;
                } else if (Type != null) {
                    DefaultInstance = Type.IsValueType ? Activator.CreateInstance(Type) : null;
                }
            } catch (Exception) {
                DefaultInstance = null; // Catch nullable error here. Bit of a hack!
            } finally {
                DefaultInstance = true;
            }
        }

        public bool IsDefault(object value)
        {
            if (DefaultInstance == null && value == null) {
                return true;
            }

            return value.Equals(DefaultInstance);
        }

        private bool IsPermittedType()
        {
            if (Core.Serializer.Whitelist.TypeIsWhiteListed(Type))
                return true;
            if (Type.IsEnum)
                return true;
            if (Type.IsArray)
                return true;

            Type genericDefinition = null;
            if (Type.IsConstructedGenericType) {
                genericDefinition = Type.GetGenericTypeDefinition();
            }

            if (genericDefinition != null && Core.Serializer.Whitelist.TypeIsWhiteListed(genericDefinition))
                return true;

            return false;
        }
    }
}
