using System.IO;
using FastStream;
using SE.Serialization.Converters;
using SE.Serialization.Resolvers;

namespace SE.Serialization
{
    public static class Serializer
    {
        private static readonly DefaultResolver defaultResolver = new DefaultResolver();

        private static readonly SerializerSettings defaultSettings = new SerializerSettings {
            NullValueHandling = NullValueHandling.Ignore,
            Resolver = defaultResolver
        };

        public static byte[] Serialize(object obj, SerializerSettings settings = null)
        {
            if(obj == null)
                return null;
            if(settings == null)
                settings = defaultSettings;
            else if(settings.Resolver == null)
                settings.Resolver = defaultResolver;

            Converter converter = settings.Resolver.GetConverter(obj.GetType());
            using(FastMemoryWriter writer = new FastMemoryWriter()) {
                if (converter == null)
                    return null;

                Serialize(writer, obj, converter, settings);
                return writer.ToArray();
            }
        }

        public static void Serialize(FastMemoryWriter writer, object obj, Converter serializer, SerializerSettings settings = null)
        {
            serializer.Serialize(obj, writer, settings);
        }

        public static T Deserialize<T>(byte[] data, SerializerSettings settings = null)
        {
            if(data == null)
                return default;
            if(settings == null)
                settings = defaultSettings;
            else if(settings.Resolver == null)
                settings.Resolver = defaultResolver;

            Converter serializer = settings.Resolver.GetConverter(typeof(T));
            MemoryStream stream = new MemoryStream(data);
            using (FastReader reader = new FastReader(stream)) {
                return (T) Deserialize(serializer, reader, settings);
            }
        }

        public static object Deserialize(Converter serializer, FastReader reader, SerializerSettings settings = null)
        {
            if(settings == null)
                settings = defaultSettings;

            return serializer.Deserialize(reader, settings);
        }
    }
}
