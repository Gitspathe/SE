using SE.Serialization.Resolvers;

namespace SE.Serialization
{
    public sealed class SerializerSettings
    {
        public NullValueHandling NullValueHandling       = NullValueHandling.Ignore;
        public DefaultValueHandling DefaultValueHandling = DefaultValueHandling.Ignore;
        public ConvertBehaviour ConvertBehaviour         = ConvertBehaviour.Order;
        public ConverterResolver Resolver                = Serializer.DefaultResolver;
    }

    /// <summary>
    /// Controls how the serializer handles null values.
    /// </summary>
    public enum NullValueHandling
    {
        /// <summary>Null values are ignored.</summary>
        Ignore,

        /// <summary>When a null is encountered, the value is set to default.</summary>
        DefaultValue
    }

    /// <summary>
    /// Controls how generated serializers behave when converting data from and to bytes.
    /// </summary>
    public enum ConvertBehaviour
    {
        /// <summary>Only declaration order is stored with the data. This options is very fast but has lower resilience to parsing errors.
        ///          It's recommended to use SerializeOrder attributes on variables with this setting.</summary>
        Order,

        /// <summary>Declaration order and name is stored with the data. Both are used during deserialization.
        ///          This option is the slowest, but has the highest resilience to parsing errors.</summary>
        NameAndOrder
    }

    /// <summary>
    /// Controls how the serializer handles default values.
    /// </summary>
    public enum DefaultValueHandling
    {
        /// <summary>Default values are not serialized.</summary>
        Ignore,

        /// <summary>Default values are serialized.</summary>
        Serialize
    }
}
