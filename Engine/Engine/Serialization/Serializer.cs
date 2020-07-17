using System;
using System.IO;
using FastStream;
using SE.Serialization.Converters;
using SE.Serialization.Exceptions;
using SE.Serialization.Resolvers;

namespace SE.Serialization
{
    public static class Serializer
    {
        public static DefaultResolver DefaultResolver { get; } = new DefaultResolver();
        public static SerializerSettings DefaultSettings { get; } = new SerializerSettings();

        public static byte[] Serialize(object obj) 
            => Serialize(obj, DefaultSettings);

        public static byte[] Serialize<T>(T obj) 
            => Serialize(obj, DefaultSettings);

        public static byte[] Serialize(object obj, SerializerSettings settings)
        {
            if(obj == null)
                return null;
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            SerializeTask task = new SerializeTask(settings);
            Converter converter = settings.Resolver.GetConverter(obj.GetType());
            using(FastMemoryWriter writer = new FastMemoryWriter()) {
                if (converter == null)
                    return null;

                SerializeWriter(obj, converter, writer, ref task);
                return writer.ToArray();
            }
        }

        public static void SerializeWriter(object obj, Converter converter, FastMemoryWriter writer, ref SerializeTask task, bool increment = true)
        {
            if(increment && task.CurrentDepth > task.Settings.MaxDepth)
                return;

            SerializeTask clone = task.Clone(increment ? 1 : 0);
            converter.Serialize(obj, writer, ref clone);
        }

        public static byte[] Serialize<T>(T obj, SerializerSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            SerializeTask task = new SerializeTask(settings);
            Converter<T> converterT = settings.Resolver.GetConverter<T>();
            using(FastMemoryWriter writer = new FastMemoryWriter()) {
                if (converterT != null) {
                    SerializeWriter(obj, converterT, writer, ref task);
                    return writer.ToArray();
                }

                // Get non-generic converter if above fails.
                Converter converter = settings.Resolver.GetConverter(typeof(T));
                if(converter == null)
                    return null;

                SerializeWriter(obj, converter, writer, ref task);
                return writer.ToArray();
            }
        }

        public static void SerializeWriter<T>(T obj, Converter<T> serializer, FastMemoryWriter writer, ref SerializeTask task, bool increment = true)
        {
            if(increment && task.CurrentDepth > task.Settings.MaxDepth)
                return;

            SerializeTask clone = task.Clone(increment ? 1 : 0);
            serializer.Serialize(obj, writer, ref clone);
        }

        public static T Deserialize<T>(byte[] data) 
            => Deserialize<T>(data, DefaultSettings);

        public static T Deserialize<T>(byte[] data, SerializerSettings settings)
        {
            if(data == null)
                return default;
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            DeserializeTask task = new DeserializeTask(settings);
            Converter<T> converterT = settings.Resolver.GetConverter<T>();
            MemoryStream stream = new MemoryStream(data);
            using (FastReader reader = new FastReader(stream)) {
                if (converterT != null) {
                    return DeserializeReader(converterT, reader, ref task);
                }

                // Get non-generic converter if above fails.
                Converter converter = settings.Resolver.GetConverter(typeof(T));
                return (T) DeserializeReader(converter, reader, ref task);
            }
        }

        public static T DeserializeReader<T>(Converter<T> converter, FastReader reader, ref DeserializeTask task)
        {
            return converter.DeserializeT(reader, ref task);
        }

        public static object DeserializeReader(Converter converter, FastReader reader, ref DeserializeTask task)
        {
            return converter.Deserialize(reader, ref task);
        }
    }
}
