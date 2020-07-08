using SE.Serialization.Resolvers;

namespace SE.Serialization
{
    public sealed class SerializerSettings
    {
        public NullValueHandling NullValueHandling;
        public ConverterResolver Resolver;
    }

    public enum NullValueHandling
    {
        Ignore,
        DefaultValue
    }
}
