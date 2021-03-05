using System;

namespace SE.Serialization.Exceptions
{
    /// <summary>
    /// Thrown when a Type isn't within the serializers' polymorphic deserialization whitelist.
    /// </summary>
    public class SerializerWhitelistException : Exception
    {
        public SerializerWhitelistException(Type type) : base("Type not polymorphic whitelist: " + type.AssemblyQualifiedName) { }
    }
}
