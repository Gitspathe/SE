using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SE.Serialization;
using SE.Serialization.Attributes;
using SE.Serialization.Converters;
using SE.Serialization.Resolvers;
using static SE.Serialization.Constants;

namespace SE.Core
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
            using(Utf8Writer writer = new Utf8Writer()) {
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
            using (Utf8Writer writer = new Utf8Writer()) {
                if (converter == null)
                    return null;

                SerializeWriter(writer, obj, converter, ref task);
                return writer.ToString();
            }
        }

        public static void SerializeWriter(Utf8Writer writer, object obj, Converter converter, ref SerializeTask task, bool increment = true)
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
            using(Utf8Writer writer = new Utf8Writer()) {
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

        public static void SerializeWriter<T>(Utf8Writer writer, T obj, Converter<T> serializer, ref SerializeTask task, bool increment = true)
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
            using (Utf8Reader reader = new Utf8Reader(stream)) {
                if (converterT != null) {
                    return DeserializeReader(reader, converterT, ref task);
                }

                // Get non-generic converter if above fails.
                Converter converter = settings.Resolver.GetConverter(typeof(T));
                return (T) DeserializeReader(reader, converter, ref task);
            }
        }

        public static T DeserializeReader<T>(Utf8Reader reader, Converter<T> converter, ref DeserializeTask task)
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

        public static object DeserializeReader(Utf8Reader reader, Converter converter, ref DeserializeTask task)
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

        internal static Converter GetConverterForTypeString(string typeString, SerializerSettings settings)
        {
            ConverterResolver resolver = settings.Resolver;
            if (resolver.TypeCache.TryGetValue(typeString, out Converter converter)) 
                return converter;

            Type type = Type.GetType(typeString);
            converter = settings.Resolver.GetConverter(type);
            resolver.TypeCache.Add(typeString, converter);
            return converter;
        }

        internal static bool ShouldWriteConverterType(Type objType, Type defaultType, SerializerSettings settings) 
        {
            if(settings.TypeHandling == TypeHandling.Ignore)
                return false;

            return settings.TypeHandling != TypeHandling.Auto || objType != defaultType;
        }

        internal static void WriteMetaText(Utf8Writer writer, SerializerSettings settings, string objTypeName, int? id)
        {
            if (objTypeName == null && id == null)
                return;

            writer.Write(_BEGIN_META);
            bool writeComma = false;
            if (objTypeName != null) {
                writer.WriteQuotedText("$type");
                writer.Write(_BEGIN_VALUE);
                writer.WriteQuotedText(objTypeName);
                writeComma = true;
            }
            if (id != null) {
                if (writeComma) {
                    writer.Write((byte)',');
                }
                writer.WriteQuotedText("$id");
                writer.Write(_BEGIN_VALUE);
                writer.WriteUtf8(id.Value);
            }
            writer.Write(_END_META);
        }

        internal static void WriteMetaBinary(Utf8Writer writer, SerializerSettings settings, string objTypeName, int? id)
        {
            if (objTypeName == null && id == null)
                return;

            writer.Write(_BEGIN_META);
            if (objTypeName != null) {
                writer.Write((byte)MetaBinaryIDs.ValueType);
                writer.Write(objTypeName);
            }
            if (id != null) {
                writer.Write((byte)MetaBinaryIDs.ID);
                writer.Write(id.Value);
            }
            writer.Write(_END_META);
        }

        internal static bool TryReadMetaBinary(Utf8Reader reader, SerializerSettings settings, out string valueType, out int? id)
        {
            valueType = null;
            id = null;

            while (true) {
                byte b = reader.ReadByte();
                switch (b) {
                    case _BEGIN_META:
                        goto FoundMetaTag;
                    case _TAB:
                        continue;
                    default:
                        reader.BaseStream.Position -= 1;
                        return false;
                }
            }

            FoundMetaTag:
            while (true) {
                byte b = reader.ReadByte();
                switch (b) {
                    case (byte)MetaBinaryIDs.ValueType: {
                        valueType = reader.ReadString();
                    } break;
                    case (byte)MetaBinaryIDs.ID: {
                        id = reader.ReadInt32();
                    } break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (reader.ReadByte() == _END_META) {
                    return true;
                }
                reader.BaseStream.Position -= 1;
            }
        }

        internal static bool TryReadMetaText(Utf8Reader reader, SerializerSettings settings, out string valueType, out int? id)
        {
            valueType = null;
            id = null;

            while (true) {
                byte b = reader.ReadByte();
                switch (b) {
                    case _BEGIN_META:
                        goto FoundMetaTag;
                    case _TAB:
                        continue;
                    default:
                        reader.BaseStream.Position -= 1;
                        return false;
                }
            }

            FoundMetaTag:
            while (true) {
                string str = reader.ReadQuotedString();
                switch (str) {
                    case "$type": {
                        reader.BaseStream.Position += 1;
                        reader.SkipWhiteSpace();
                        valueType = reader.ReadQuotedString();
                    } break;
                    case "$id": {
                        reader.BaseStream.Position += 1;
                        reader.SkipWhiteSpace();
                        id = reader.ReadIntUtf8();
                    } break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (reader.ReadByte() == _END_META) {
                    return true;
                }
            }
        }

        public static class Whitelist
        {
            internal static HashSet<Type> PolymorphicWhitelist = new HashSet<Type>();

            static Whitelist()
            {
                // Add user-defined types (GeneratedConverters).
                List<Type> customTypes = ReflectionUtil.GetTypes((z => z.GetCustomAttributes(typeof(SerializeObjectAttribute), true).Length > 0)).ToList();
                foreach (Type t in customTypes) {
                    PolymorphicWhitelist.Add(t);
                }
            }
        }
    }

    public enum MetaBinaryIDs : byte
    {
        ValueType = 1,
        ID = 2
    }
}
