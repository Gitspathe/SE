using System;
using SE.Serialization.Converters;

namespace SE.Serialization.Resolvers
{
    public abstract class ConverterResolver
    {
        public abstract Converter GetConverter(Type objType);
    }
}
