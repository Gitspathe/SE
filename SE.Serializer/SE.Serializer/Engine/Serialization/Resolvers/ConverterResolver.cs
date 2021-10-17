using SE.Serialization.Converters;
using System;
using System.Collections.Generic;

namespace SE.Serialization.Resolvers
{
    public abstract class ConverterResolver
    {
        internal Dictionary<string, Converter> TypeCache = new Dictionary<string, Converter>();

        public abstract Converter GetConverter(Type objType);
        public abstract Converter<T> GetConverter<T>();
    }
}
