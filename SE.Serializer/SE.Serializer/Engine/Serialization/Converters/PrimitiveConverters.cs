using System;
using System.Globalization;
using System.Text;
using FastStream;
using SE.Core;

namespace SE.Serialization.Converters
{
    public sealed class BoolConverter : Converter<bool>
    {
        public override Type Type => typeof(bool);

        public override void SerializeBinary(bool obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override bool DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadBoolean();
        public override void SerializeText(bool obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.WriteText(obj.ToString());
        public override bool DeserializeTText(FastReader reader, ref DeserializeTask task)
            => bool.Parse(reader.ReadQuotedString());
    }

    public sealed class ByteConverter : Converter<byte>
    {
        public override Type Type => typeof(byte);
        
        public override void SerializeBinary(byte obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override byte DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadByte();
        public override void SerializeText(byte obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.WriteText(obj.ToString());
        public override byte DeserializeTText(FastReader reader, ref DeserializeTask task)
            => byte.Parse(reader.ReadQuotedString());
    }

    public sealed class SByteConverter : Converter<sbyte>
    {
        public override Type Type => typeof(sbyte);
        
        public override void SerializeBinary(sbyte obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override sbyte DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadSByte();
        public override void SerializeText(sbyte obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.WriteText(obj.ToString());
        public override sbyte DeserializeTText(FastReader reader, ref DeserializeTask task)
            => sbyte.Parse(reader.ReadQuotedString());
    }

    public sealed class ShortConverter : Converter<short>
    {
        public override Type Type => typeof(short);
        
        public override void SerializeBinary(short obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override short DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadInt16();
        public override void SerializeText(short obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.WriteText(obj.ToString());
        public override short DeserializeTText(FastReader reader, ref DeserializeTask task)
            => short.Parse(reader.ReadQuotedString());
    }

    public sealed class UShortConverter : Converter<ushort>
    {
        public override Type Type => typeof(ushort);
        
        public override void SerializeBinary(ushort obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override ushort DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadUInt16();
        public override void SerializeText(ushort obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.WriteText(obj.ToString());
        public override ushort DeserializeTText(FastReader reader, ref DeserializeTask task)
            => ushort.Parse(reader.ReadQuotedString());
    }

    public sealed class IntConverter : Converter<int>
    {
        public override Type Type => typeof(int);

        public override void SerializeBinary(int obj, FastMemoryWriter writer, ref SerializeTask task) 
            => writer.Write(obj);
        public override int DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadInt32();
        public override void SerializeText(int obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.WriteText(obj.ToString());
        public override int DeserializeTText(FastReader reader, ref DeserializeTask task)
            => int.Parse(reader.ReadQuotedString());
    }

    public sealed class UIntConverter : Converter<uint>
    {
        public override Type Type => typeof(uint);
        
        public override void SerializeBinary(uint obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override uint DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadUInt32();
        public override void SerializeText(uint obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.WriteText(obj.ToString());
        public override uint DeserializeTText(FastReader reader, ref DeserializeTask task)
            => uint.Parse(reader.ReadQuotedString());
    }

    public sealed class LongConverter : Converter<long>
    {
        public override Type Type => typeof(long);
        
        public override void SerializeBinary(long obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override long DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadInt64();
        public override void SerializeText(long obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.WriteText(obj.ToString());
        public override long DeserializeTText(FastReader reader, ref DeserializeTask task)
            => long.Parse(reader.ReadQuotedString());
    }

    public sealed class ULongConverter : Converter<ulong>
    {
        public override Type Type => typeof(ulong);
        
        public override void SerializeBinary(ulong obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override ulong DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadUInt64();
        public override void SerializeText(ulong obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.WriteText(obj.ToString());
        public override ulong DeserializeTText(FastReader reader, ref DeserializeTask task)
            => ulong.Parse(reader.ReadQuotedString());
    }

    public sealed class FloatConverter : Converter<float>
    {
        public override Type Type => typeof(float);
        
        public override void SerializeBinary(float obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override float DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadSingle();
        public override void SerializeText(float obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.WriteText(obj.ToString(CultureInfo.InvariantCulture));
        public override float DeserializeTText(FastReader reader, ref DeserializeTask task)
            => float.Parse(reader.ReadQuotedString());
    }

    public sealed class DoubleConverter : Converter<double>
    {
        public override Type Type => typeof(double);
        
        public override void SerializeBinary(double obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override double DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadDouble();
        public override void SerializeText(double obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.WriteText(obj.ToString(CultureInfo.InvariantCulture));
        public override double DeserializeTText(FastReader reader, ref DeserializeTask task)
            => double.Parse(reader.ReadQuotedString());
    }

    public sealed class CharConverter : Converter<char>
    {
        public override Type Type => typeof(char);
        
        public override void SerializeBinary(char obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override char DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadChar();
        public override void SerializeText(char obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.WriteText(obj.ToString());
        public override char DeserializeTText(FastReader reader, ref DeserializeTask task)
            => char.Parse(reader.ReadQuotedString());
    }

    public sealed class StringConverter : Converter<string>
    {
        public override Type Type => typeof(string);
        
        public override void SerializeBinary(string obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override string DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadString();

        public override void SerializeText(string obj, FastMemoryWriter writer, ref SerializeTask task)
        {
            writer.Write(Serializer._STRING_IDENTIFIER);
            writer.Write(Encoding.UTF8.GetBytes(obj));
            writer.Write(Serializer._STRING_IDENTIFIER);
        }

        public override string DeserializeTText(FastReader reader, ref DeserializeTask task)
            => reader.ReadQuotedString();
    }

    public sealed class ObjectConverter : Converter
    {
        public override Type Type => typeof(object);

        public override void SerializeBinary(object obj, FastMemoryWriter writer, ref SerializeTask task)
        {
            //throw new NotImplementedException();
        }

        public override object DeserializeBinary(FastReader reader, ref DeserializeTask task)
        {
            return null;
        }
    }
}
