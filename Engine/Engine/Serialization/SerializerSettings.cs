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

    public enum NullValueHandling
    {
        Ignore,
        DefaultValue
    }

    public enum ConvertBehaviour
    {
        Name,
        Order,
        NameAndOrder
    }

    public enum DefaultValueHandling
    {
        Ignore,
        Serialize
    }
}
