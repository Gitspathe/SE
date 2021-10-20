using System;

namespace SE.Serialization.Converters
{
    public sealed class BoolConverter : Converter<bool>
    {
        public override Type Type => typeof(bool);

        public override void SerializeBinary(bool obj, Utf8Writer writer, ref SerializeTask task)
            => writer.Write(obj);
        public override bool DeserializeTBinary(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadBoolean();
        public override void SerializeText(bool obj, Utf8Writer writer, ref SerializeTask task)
            => writer.WriteUtf8(obj);
        public override bool DeserializeTText(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadBooleanString();
    }

    public sealed class ByteConverter : Converter<byte>
    {
        public override Type Type => typeof(byte);

        public override void SerializeBinary(byte obj, Utf8Writer writer, ref SerializeTask task)
            => writer.Write(obj);
        public override byte DeserializeTBinary(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadByte();
        public override void SerializeText(byte obj, Utf8Writer writer, ref SerializeTask task)
            => writer.WriteUtf8(obj);
        public override byte DeserializeTText(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadByteUtf8();
    }

    public sealed class SByteConverter : Converter<sbyte>
    {
        public override Type Type => typeof(sbyte);

        public override void SerializeBinary(sbyte obj, Utf8Writer writer, ref SerializeTask task)
            => writer.Write(obj);
        public override sbyte DeserializeTBinary(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadSByte();
        public override void SerializeText(sbyte obj, Utf8Writer writer, ref SerializeTask task)
            => writer.WriteUtf8(obj);
        public override sbyte DeserializeTText(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadSByteUtf8();
    }

    public sealed class ShortConverter : Converter<short>
    {
        public override Type Type => typeof(short);

        public override void SerializeBinary(short obj, Utf8Writer writer, ref SerializeTask task)
            => writer.Write(obj);
        public override short DeserializeTBinary(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadInt16();
        public override void SerializeText(short obj, Utf8Writer writer, ref SerializeTask task)
            => writer.WriteUtf8(obj);
        public override short DeserializeTText(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadShortUtf8();
    }

    public sealed class UShortConverter : Converter<ushort>
    {
        public override Type Type => typeof(ushort);

        public override void SerializeBinary(ushort obj, Utf8Writer writer, ref SerializeTask task)
            => writer.Write(obj);
        public override ushort DeserializeTBinary(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadUInt16();
        public override void SerializeText(ushort obj, Utf8Writer writer, ref SerializeTask task)
            => writer.WriteUtf8(obj);
        public override ushort DeserializeTText(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadUShortUtf8();
    }

    public sealed class IntConverter : Converter<int>
    {
        public override Type Type => typeof(int);

        public override void SerializeBinary(int obj, Utf8Writer writer, ref SerializeTask task)
            => writer.Write(obj);
        public override int DeserializeTBinary(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadInt32();
        public override void SerializeText(int obj, Utf8Writer writer, ref SerializeTask task)
            => writer.WriteUtf8(obj);
        public override int DeserializeTText(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadIntUtf8();
    }

    public sealed class UIntConverter : Converter<uint>
    {
        public override Type Type => typeof(uint);

        public override void SerializeBinary(uint obj, Utf8Writer writer, ref SerializeTask task)
            => writer.Write(obj);
        public override uint DeserializeTBinary(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadUInt32();
        public override void SerializeText(uint obj, Utf8Writer writer, ref SerializeTask task)
            => writer.WriteUtf8(obj);
        public override uint DeserializeTText(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadUIntUtf8();
    }

    public sealed class LongConverter : Converter<long>
    {
        public override Type Type => typeof(long);

        public override void SerializeBinary(long obj, Utf8Writer writer, ref SerializeTask task)
            => writer.Write(obj);
        public override long DeserializeTBinary(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadInt64();
        public override void SerializeText(long obj, Utf8Writer writer, ref SerializeTask task)
            => writer.WriteUtf8(obj);
        public override long DeserializeTText(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadLongUtf8();
    }

    public sealed class ULongConverter : Converter<ulong>
    {
        public override Type Type => typeof(ulong);

        public override void SerializeBinary(ulong obj, Utf8Writer writer, ref SerializeTask task)
            => writer.Write(obj);
        public override ulong DeserializeTBinary(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadUInt64();
        public override void SerializeText(ulong obj, Utf8Writer writer, ref SerializeTask task)
            => writer.WriteUtf8(obj);
        public override ulong DeserializeTText(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadULongUtf8();
    }

    public sealed class FloatConverter : Converter<float>
    {
        public override Type Type => typeof(float);

        public override void SerializeBinary(float obj, Utf8Writer writer, ref SerializeTask task)
            => writer.Write(obj);
        public override float DeserializeTBinary(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadSingle();
        public override void SerializeText(float obj, Utf8Writer writer, ref SerializeTask task)
            => writer.WriteUtf8(obj);
        public override float DeserializeTText(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadFloatUtf8();
    }

    public sealed class DoubleConverter : Converter<double>
    {
        public override Type Type => typeof(double);

        public override void SerializeBinary(double obj, Utf8Writer writer, ref SerializeTask task)
            => writer.Write(obj);
        public override double DeserializeTBinary(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadDouble();
        public override void SerializeText(double obj, Utf8Writer writer, ref SerializeTask task)
            => writer.WriteUtf8(obj);
        public override double DeserializeTText(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadDoubleUtf8();
    }

    public sealed class CharConverter : Converter<char>
    {
        public override Type Type => typeof(char);

        public override void SerializeBinary(char obj, Utf8Writer writer, ref SerializeTask task)
            => writer.Write(obj);
        public override char DeserializeTBinary(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadChar();
        public override void SerializeText(char obj, Utf8Writer writer, ref SerializeTask task)
            => writer.WriteQuotedTextUtf8(SerializerUtil.EscapeString(obj.ToString()));
        public override char DeserializeTText(Utf8Reader reader, ref DeserializeTask task)
            => char.Parse(reader.ReadQuotedString());
    }

    public sealed class StringConverter : Converter<string>
    {
        public override Type Type => typeof(string);

        public override void SerializeBinary(string obj, Utf8Writer writer, ref SerializeTask task)
            => writer.WriteQuotedTextUtf8(SerializerUtil.EscapeString(obj));
        public override string DeserializeTBinary(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadQuotedString();

        public override void SerializeText(string obj, Utf8Writer writer, ref SerializeTask task)
            => writer.WriteQuotedTextUtf8(SerializerUtil.EscapeString(obj));
        public override string DeserializeTText(Utf8Reader reader, ref DeserializeTask task)
            => reader.ReadQuotedString();
    }

    public sealed class ObjectConverter : Converter
    {
        public override Type Type => typeof(object);

        public override void SerializeBinary(object obj, Utf8Writer writer, ref SerializeTask task)
        {
            //throw new NotImplementedException();
        }

        public override object DeserializeBinary(Utf8Reader reader, ref DeserializeTask task)
        {
            return null;
        }
    }
}
