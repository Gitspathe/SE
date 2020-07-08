using System;
using System.IO;
using FastStream;
using SE.Serialization.Converters;
using SE.Serialization.Resolvers;

namespace SE.Serialization
{
    public static class Serializer
    {
        public static DefaultResolver DefaultResolver { get; } = new DefaultResolver();
        public static SerializerSettings DefaultSettings { get; } = new SerializerSettings();

        public static byte[] Serialize(object obj) 
            => Serialize(obj, DefaultSettings);

        public static byte[] Serialize(object obj, SerializerSettings settings)
        {
            if(obj == null)
                return null;
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            Converter converter = settings.Resolver.GetConverter(obj.GetType());
            using(FastMemoryWriter writer = new FastMemoryWriter()) {
                if (converter == null)
                    return null;

                Serialize(obj, settings, converter, writer);
                return writer.ToArray();
            }
        }

        public static void Serialize(object obj, SerializerSettings settings, Converter serializer, FastMemoryWriter writer) 
            => serializer.Serialize(obj, writer, settings);

        public static T Deserialize<T>(byte[] data) 
            => Deserialize<T>(data, DefaultSettings);

        public static T Deserialize<T>(byte[] data, SerializerSettings settings)
        {
            if(data == null)
                return default;
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            Converter serializer = settings.Resolver.GetConverter(typeof(T));
            MemoryStream stream = new MemoryStream(data);
            using (FastReader reader = new FastReader(stream)) {
                return (T) Deserialize(serializer, reader, settings);
            }
        }

        public static object Deserialize(Converter serializer, FastReader reader, SerializerSettings settings) 
            => serializer.Deserialize(reader, settings);
    }
}
