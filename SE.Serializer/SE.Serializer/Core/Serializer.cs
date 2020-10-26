using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FastStream;
using SE.Serialization;
using SE.Serialization.Converters;
using SE.Serialization.Resolvers;

namespace SE.Core
{
    public static class Serializer
    {
        public static DefaultResolver DefaultResolver { get; } = new DefaultResolver();
        public static SerializerSettings DefaultSettings { get; } = new SerializerSettings();

        public const byte _BEGIN_VALUE = (byte)':';
        public const byte _BEGIN_CLASS = (byte)'{';
        public const byte _END_CLASS = (byte)'}';
        public const byte _BEGIN_ARRAY = (byte)'[';
        public const byte _END_ARRAY = (byte)']';
        public const byte _BEGIN_META = (byte)'(';
        public const byte _END_META = (byte)')';
        public const byte _ARRAY_SEPARATOR = (byte)',';
        public const byte _NEW_LINE = (byte)'\n';
        public const byte _TAB = (byte)' ';
        public const byte _STRING_IDENTIFIER = (byte)'"';

        public static readonly byte[] Tabs = { _TAB, _TAB };

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

                SerializeWriter(writer, obj, converter, ref task);
                return writer.ToArray();
            }
        }

        public static string SerializeString(object obj, SerializerSettings settings)
        {
            if (obj == null)
                return null;
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            SerializeTask task = new SerializeTask(settings);
            Converter converter = settings.Resolver.GetConverter(obj.GetType());
            using (FastMemoryWriter writer = new FastMemoryWriter()) {
                if (converter == null)
                    return null;

                SerializeWriter(writer, obj, converter, ref task);
                return writer.ToString();
            }
        }

        public static void SerializeWriter(FastMemoryWriter writer, object obj, Converter converter, ref SerializeTask task, bool increment = true)
        {
            if(increment && task.CurrentDepth > task.Settings.MaxDepth)
                return;

            SerializeTask clone = task.Clone(increment ? 1 : 0);
            switch (task.Settings.Formatting) {
                case Formatting.Binary:
                    converter.SerializeBinary(obj, writer, ref clone);
                    break;
                case Formatting.Text:
                    converter.SerializeText(obj, writer, ref clone);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static byte[] Serialize<T>(T obj, SerializerSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            SerializeTask task = new SerializeTask(settings);
            Converter<T> converterT = settings.Resolver.GetConverter<T>();
            using(FastMemoryWriter writer = new FastMemoryWriter()) {
                if (converterT != null) {
                    SerializeWriter(writer, obj, converterT, ref task);
                    return writer.ToArray();
                }

                // Get non-generic converter if above fails.
                Converter converter = settings.Resolver.GetConverter(typeof(T));
                if(converter == null)
                    return null;

                SerializeWriter(writer, obj, converter, ref task);
                return writer.ToArray();
            }
        }

        public static void SerializeWriter<T>(FastMemoryWriter writer, T obj, Converter<T> serializer, ref SerializeTask task, bool increment = true)
        {
            if(increment && task.CurrentDepth > task.Settings.MaxDepth)
                return;

            SerializeTask clone = task.Clone(increment ? 1 : 0);
            switch (task.Settings.Formatting) {
                case Formatting.Binary:
                    serializer.SerializeBinary(obj, writer, ref clone);
                    break;
                case Formatting.Text:
                    serializer.SerializeText(obj, writer, ref clone);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
                    return DeserializeReader(reader, converterT, ref task);
                }

                // Get non-generic converter if above fails.
                Converter converter = settings.Resolver.GetConverter(typeof(T));
                return (T) DeserializeReader(reader, converter, ref task);
            }
        }

        public static T DeserializeReader<T>(FastReader reader, Converter<T> converter, ref DeserializeTask task)
        {
            switch (task.Settings.Formatting) {
                case Formatting.Binary:
                    return converter.DeserializeTBinary(reader, ref task);
                case Formatting.Text:
                    return converter.DeserializeTText(reader, ref task);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static object DeserializeReader(FastReader reader, Converter converter, ref DeserializeTask task)
        {
            switch (task.Settings.Formatting) {
                case Formatting.Binary:
                    return converter.DeserializeBinary(reader, ref task);
                case Formatting.Text:
                    return converter.DeserializeText(reader, ref task);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Converter GetConverterForType(FastReader reader, SerializerSettings settings)
        {
            if(settings.TypeHandling == TypeHandling.Ignore)
                return null;

            switch (settings.Formatting) {
                case Formatting.Binary: {
                    if (!reader.ReadBoolean())
                        return null;

                    string typeString = reader.ReadString();
                    ConverterResolver resolver = settings.Resolver;
                    if (!resolver.TypeCache.TryGetValue(typeString, out Converter converter)) {
                        Type type = Type.GetType(typeString);
                        converter = settings.Resolver.GetConverter(type);
                        resolver.TypeCache.Add(typeString, converter);
                    }
                    return converter;
                }
                case Formatting.Text: {
                    throw new NotImplementedException(); // TODO.
                } break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal static Converter WriteAndGetConverterType(FastMemoryWriter writer, object obj, Type defaultType, SerializerSettings settings)
        {
            if (settings.TypeHandling == TypeHandling.Ignore)
                return null;

            switch (settings.Formatting) {
                case Formatting.Binary: {
                    Type objType = obj.GetType();
                    if (settings.TypeHandling == TypeHandling.Auto && objType == defaultType) {
                        writer.Write(false);
                        return null;
                    }

                    writer.Write(true);
                    writer.Write(objType.AssemblyQualifiedName);
                    return settings.Resolver.GetConverter(objType);
                }
                case Formatting.Text: {
                    throw new NotImplementedException(); // TODO.
                } break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void WriteConverterType(FastMemoryWriter writer, Type objType, Type defaultType, SerializerSettings settings)
        {
            if (settings.TypeHandling == TypeHandling.Ignore)
                return;

            switch (settings.Formatting) {
                case Formatting.Binary: {
                    if (settings.TypeHandling == TypeHandling.Auto && objType == defaultType) {
                        writer.Write(false);
                        return;
                    }
                    writer.Write(true);
                    writer.Write(objType.AssemblyQualifiedName);
                } break;
                case Formatting.Text: {
                    if (settings.TypeHandling == TypeHandling.Auto && objType == defaultType)
                        return;

                    // TODO: Should support meta info. Example format:
                    // class {
                    //   number: ($type="System.Int32", $id=0) 5
                    // }
                    //
                    // Elements within the parentheses are 'meta' tokens, describing stuff like type, id, etc.

                    writer.Write(_BEGIN_META);
                    writer.WriteText(objType.AssemblyQualifiedName);
                    writer.Write(_END_META);
                } break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
