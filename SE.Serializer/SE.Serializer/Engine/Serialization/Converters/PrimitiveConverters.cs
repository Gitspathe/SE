using System;
using FastStream;
using SE.Core;

namespace SE.Serialization.Converters
{
    public sealed class BoolConverter : Converter<bool>
    {
        public override Type Type => typeof(bool);

        public override void SerializeBinary(object obj, FastMemoryWriter writer, ref SerializeTask task) 
            => writer.Write((bool) obj);
        public override object DeserializeBinary(FastReader reader, ref DeserializeTask task) 
            => reader.ReadBoolean();
        public override void SerializeBinary(bool obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override bool DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadBoolean();
    }

    public sealed class ByteConverter : Converter<byte>
    {
        public override Type Type => typeof(byte);
        
        public override void SerializeBinary(object obj, FastMemoryWriter writer, ref SerializeTask task) 
            => writer.Write((byte) obj);
        public override object DeserializeBinary(FastReader reader, ref DeserializeTask task) 
            => reader.ReadByte();
        public override void SerializeBinary(byte obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override byte DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadByte();
    }

    public sealed class SByteConverter : Converter<sbyte>
    {
        public override Type Type => typeof(sbyte);
        
        public override void SerializeBinary(object obj, FastMemoryWriter writer, ref SerializeTask task) 
            => writer.Write((sbyte) obj);
        public override object DeserializeBinary(FastReader reader, ref DeserializeTask task) 
            => reader.ReadSByte();
        public override void SerializeBinary(sbyte obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override sbyte DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadSByte();
    }

    public sealed class ShortConverter : Converter<short>
    {
        public override Type Type => typeof(short);
        
        public override void SerializeBinary(object obj, FastMemoryWriter writer, ref SerializeTask task) 
            => writer.Write((short) obj);
        public override object DeserializeBinary(FastReader reader, ref DeserializeTask task) 
            => reader.ReadInt16();
        public override void SerializeBinary(short obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override short DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadInt16();
    }

    public sealed class UShortConverter : Converter<ushort>
    {
        public override Type Type => typeof(ushort);
        
        public override void SerializeBinary(object obj, FastMemoryWriter writer, ref SerializeTask task) 
            => writer.Write((ushort) obj);
        public override object DeserializeBinary(FastReader reader, ref DeserializeTask task) 
            => reader.ReadUInt16();
        public override void SerializeBinary(ushort obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override ushort DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadUInt16();
    }

    public sealed class IntConverter : Converter<int>
    {
        public override Type Type => typeof(int);

        public override void SerializeBinary(object obj, FastMemoryWriter writer, ref SerializeTask task) 
            => writer.Write((int) obj);
        public override object DeserializeBinary(FastReader reader, ref DeserializeTask task) 
            => reader.ReadInt32();
        public override void SerializeBinary(int obj, FastMemoryWriter writer, ref SerializeTask task) 
            => writer.Write(obj);
        public override int DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadInt32();

        public override void SerializeText(object obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.WriteText(obj.ToString());
        public override object DeserializeText(FastReader reader, ref DeserializeTask task)
            => int.Parse(reader.ReadQuotedString());
        public override void SerializeText(int obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.WriteText(obj.ToString());
        public override int DeserializeTText(FastReader reader, ref DeserializeTask task)
            => int.Parse(reader.ReadQuotedString());
    }

    public sealed class UIntConverter : Converter<uint>
    {
        public override Type Type => typeof(uint);
        
        public override void SerializeBinary(object obj, FastMemoryWriter writer, ref SerializeTask task) 
            => writer.Write((uint) obj);
        public override object DeserializeBinary(FastReader reader, ref DeserializeTask task) 
            => reader.ReadUInt32();
        public override void SerializeBinary(uint obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override uint DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadUInt32();
    }

    public sealed class LongConverter : Converter<long>
    {
        public override Type Type => typeof(long);
       
        public override void SerializeBinary(object obj, FastMemoryWriter writer, ref SerializeTask task) 
            => writer.Write((long) obj);
        public override object DeserializeBinary(FastReader reader, ref DeserializeTask task) 
            => reader.ReadInt64();
        public override void SerializeBinary(long obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override long DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadInt64();
    }

    public sealed class ULongConverter : Converter<ulong>
    {
        public override Type Type => typeof(ulong);
        
        public override void SerializeBinary(object obj, FastMemoryWriter writer, ref SerializeTask task) 
            => writer.Write((ulong) obj);
        public override object DeserializeBinary(FastReader reader, ref DeserializeTask task) 
            => reader.ReadUInt64();
        public override void SerializeBinary(ulong obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override ulong DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadUInt64();
    }

    public sealed class FloatConverter : Converter<float>
    {
        public override Type Type => typeof(float);
        
        public override void SerializeBinary(object obj, FastMemoryWriter writer, ref SerializeTask task) 
            => writer.Write((float) obj);
        public override object DeserializeBinary(FastReader reader, ref DeserializeTask task) 
            => reader.ReadSingle();
        public override void SerializeBinary(float obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override float DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadSingle();
    }

    public sealed class DoubleConverter : Converter<double>
    {
        public override Type Type => typeof(double);
       
        public override void SerializeBinary(object obj, FastMemoryWriter writer, ref SerializeTask task) 
            => writer.Write((double) obj);
        public override object DeserializeBinary(FastReader reader, ref DeserializeTask task) 
            => reader.ReadDouble();
        public override void SerializeBinary(double obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override double DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadDouble();
    }

    public sealed class CharConverter : Converter<char>
    {
        public override Type Type => typeof(char);
        
        public override void SerializeBinary(object obj, FastMemoryWriter writer, ref SerializeTask task) 
            => writer.Write((char) obj);
        public override object DeserializeBinary(FastReader reader, ref DeserializeTask task) 
            => reader.ReadChar();
        public override void SerializeBinary(char obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override char DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadChar();
    }

    public sealed class StringConverter : Converter<string>
    {
        public override Type Type => typeof(string);
        
        public override void SerializeBinary(object obj, FastMemoryWriter writer, ref SerializeTask task) 
            => writer.Write((string) obj);
        public override object DeserializeBinary(FastReader reader, ref DeserializeTask task) 
            => reader.ReadString();
        public override void SerializeBinary(string obj, FastMemoryWriter writer, ref SerializeTask task)
            => writer.Write(obj);
        public override string DeserializeTBinary(FastReader reader, ref DeserializeTask task)
            => reader.ReadString();
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
