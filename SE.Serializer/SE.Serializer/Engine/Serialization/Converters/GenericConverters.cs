using SE.Core;
using SE.Engine.Serialization;
using System;
using System.Collections.Generic;
using static SE.Serialization.Constants;

namespace SE.Serialization.Converters
{

    public sealed class EnumConverter : GenericConverter
    {
        public override Type Type => typeof(Enum);

        private static Dictionary<Type, EnumDataType> enumDataTypeLookup = new Dictionary<Type, EnumDataType>() {
            { typeof(sbyte), EnumDataType.SByte },
            { typeof(byte), EnumDataType.Byte },
            { typeof(short), EnumDataType.Short },
            { typeof(ushort), EnumDataType.UShort },
            { typeof(int), EnumDataType.Int },
            { typeof(uint), EnumDataType.UInt },
            { typeof(long), EnumDataType.Long },
            { typeof(ulong), EnumDataType.ULong }
        };

        private EnumDataType enumDataType;

        protected internal override void PostConstructor()
        {
            base.PostConstructor();
            Type enumType = Enum.GetUnderlyingType(TypeArguments[0]);
            enumDataType = enumDataTypeLookup[enumType];
        }

        public override object DeserializeBinary(Utf8Reader reader, ref DeserializeTask task)
        {
            EnumDataType type = (EnumDataType)reader.ReadByte();
            switch (type) {
                case EnumDataType.SByte:
                    return Enum.ToObject(TypeArguments[0], reader.ReadSByte());
                case EnumDataType.Byte:
                    return Enum.ToObject(TypeArguments[0], reader.ReadByte());
                case EnumDataType.Short:
                    return Enum.ToObject(TypeArguments[0], reader.ReadInt16());
                case EnumDataType.UShort:
                    return Enum.ToObject(TypeArguments[0], reader.ReadUInt16());
                case EnumDataType.Int:
                    return Enum.ToObject(TypeArguments[0], reader.ReadInt32());
                case EnumDataType.UInt:
                    return Enum.ToObject(TypeArguments[0], reader.ReadUInt32());
                case EnumDataType.Long:
                    return Enum.ToObject(TypeArguments[0], reader.ReadInt64());
                case EnumDataType.ULong:
                    return Enum.ToObject(TypeArguments[0], reader.ReadUInt64());
            }
            return null;
        }

        public override void SerializeBinary(object obj, Utf8Writer writer, ref SerializeTask task)
        {
            writer.Write((byte)enumDataType);
            switch (enumDataType) {
                case EnumDataType.SByte:
                    writer.Write((sbyte)obj);
                    break;
                case EnumDataType.Byte:
                    writer.Write((byte)obj);
                    break;
                case EnumDataType.Short:
                    writer.Write((short)obj);
                    break;
                case EnumDataType.UShort:
                    writer.Write((ushort)obj);
                    break;
                case EnumDataType.Int:
                    writer.Write((int)obj);
                    break;
                case EnumDataType.UInt:
                    writer.Write((uint)obj);
                    break;
                case EnumDataType.Long:
                    writer.Write((long)obj);
                    break;
                case EnumDataType.ULong:
                    writer.Write((ulong)obj);
                    break;
            }
        }

        public override object DeserializeText(Utf8Reader reader, ref DeserializeTask task)
        {
            return Enum.Parse(TypeArguments[0], reader.ReadQuotedString());
        }

        public override void SerializeText(object obj, Utf8Writer writer, ref SerializeTask task)
        {
            writer.WriteQuotedTextUtf8(Enum.GetName(TypeArguments[0], obj));
        }

        public enum EnumDataType : byte
        {
            SByte,
            Byte,
            Short,
            UShort,
            Int,
            UInt,
            Long,
            ULong
        }
    }

    public sealed class ArrayConverter : GenericConverter
    {
        public override Type Type => typeof(Array);

        public override object DeserializeBinary(Utf8Reader reader, ref DeserializeTask task)
        {
            return reader.ReadArrayBinaryUtf8(GetConverter(0, ref task), ref task);
        }

        public override void SerializeBinary(object obj, Utf8Writer writer, ref SerializeTask task)
        {
            writer.WriteArrayBinary((Array)obj, GetConverter(0, ref task), ref task);
        }

        public override object DeserializeText(Utf8Reader reader, ref DeserializeTask task)
        {
            return reader.ReadArrayTextUtf8(GetConverter(0, ref task), ref task);
        }

        public override void SerializeText(object obj, Utf8Writer writer, ref SerializeTask task)
        {
            writer.WriteArrayText((Array)obj, GetConverter(0, ref task), ref task);
        }

        public override bool IsDefault(object obj)
        {
            if (obj is Array array) {
                return array.Length < 1;
            }
            return true;
        }
    }

    public sealed class NullableConverter : GenericConverter
    {
        public override Type Type => typeof(Nullable<>);

        public override object DeserializeBinary(Utf8Reader reader, ref DeserializeTask task)
        {
            if (!reader.ReadBoolean())
                return null;

            Converter serializer = task.Settings.Resolver.GetConverter(TypeArguments[0]);
            return serializer?.DeserializeBinary(reader, ref task);
        }

        public override void SerializeBinary(object obj, Utf8Writer writer, ref SerializeTask task)
        {
            bool hasValue = obj != null;
            writer.Write(hasValue);
            if (hasValue) {
                task.Settings.Resolver.GetConverter(TypeArguments[0])?.SerializeBinary(obj, writer, ref task);
            }
        }
    }
}
