using System;
using FastStream;

namespace SE.Serialization.Converters
{
    public sealed class BoolConverter : Converter<bool>
    {
        public override Type Type => typeof(bool);

        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((bool) obj);
        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadBoolean();
        public override void Serialize(bool obj, FastMemoryWriter writer, SerializerSettings settings)
            => writer.Write(obj);
        public override bool DeserializeT(FastReader reader, SerializerSettings settings)
            => reader.ReadBoolean();
    }

    public sealed class ByteConverter : Converter<byte>
    {
        public override Type Type => typeof(byte);
        
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((byte) obj);
        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadByte();
        public override void Serialize(byte obj, FastMemoryWriter writer, SerializerSettings settings)
            => writer.Write(obj);
        public override byte DeserializeT(FastReader reader, SerializerSettings settings)
            => reader.ReadByte();
    }

    public sealed class SByteConverter : Converter<sbyte>
    {
        public override Type Type => typeof(sbyte);
        
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((sbyte) obj);
        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadSByte();
        public override void Serialize(sbyte obj, FastMemoryWriter writer, SerializerSettings settings)
            => writer.Write(obj);
        public override sbyte DeserializeT(FastReader reader, SerializerSettings settings)
            => reader.ReadSByte();
    }

    public sealed class ShortConverter : Converter<short>
    {
        public override Type Type => typeof(short);
        
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((short) obj);
        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadInt16();
        public override void Serialize(short obj, FastMemoryWriter writer, SerializerSettings settings)
            => writer.Write(obj);
        public override short DeserializeT(FastReader reader, SerializerSettings settings)
            => reader.ReadInt16();
    }

    public sealed class UShortConverter : Converter<ushort>
    {
        public override Type Type => typeof(ushort);
        
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((ushort) obj);
        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadUInt16();
        public override void Serialize(ushort obj, FastMemoryWriter writer, SerializerSettings settings)
            => writer.Write(obj);
        public override ushort DeserializeT(FastReader reader, SerializerSettings settings)
            => reader.ReadUInt16();
    }

    public sealed class IntConverter : Converter<int>
    {
        public override Type Type => typeof(int);
        
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((int) obj);
        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadInt32();
        public override void Serialize(int obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write(obj);
        public override int DeserializeT(FastReader reader, SerializerSettings settings)
            => reader.ReadInt32();
    }

    public sealed class UIntConverter : Converter<uint>
    {
        public override Type Type => typeof(uint);
        
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((uint) obj);
        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadUInt32();
        public override void Serialize(uint obj, FastMemoryWriter writer, SerializerSettings settings)
            => writer.Write(obj);
        public override uint DeserializeT(FastReader reader, SerializerSettings settings)
            => reader.ReadUInt32();
    }

    public sealed class LongConverter : Converter<long>
    {
        public override Type Type => typeof(long);
       
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((long) obj);
        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadInt64();
        public override void Serialize(long obj, FastMemoryWriter writer, SerializerSettings settings)
            => writer.Write(obj);
        public override long DeserializeT(FastReader reader, SerializerSettings settings)
            => reader.ReadInt64();
    }

    public sealed class ULongConverter : Converter<ulong>
    {
        public override Type Type => typeof(ulong);
        
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((ulong) obj);
        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadUInt64();
        public override void Serialize(ulong obj, FastMemoryWriter writer, SerializerSettings settings)
            => writer.Write(obj);
        public override ulong DeserializeT(FastReader reader, SerializerSettings settings)
            => reader.ReadUInt64();
    }

    public sealed class FloatConverter : Converter<float>
    {
        public override Type Type => typeof(float);
        
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((float) obj);
        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadSingle();
        public override void Serialize(float obj, FastMemoryWriter writer, SerializerSettings settings)
            => writer.Write(obj);
        public override float DeserializeT(FastReader reader, SerializerSettings settings)
            => reader.ReadSingle();
    }

    public sealed class DoubleConverter : Converter<double>
    {
        public override Type Type => typeof(double);
       
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((double) obj);
        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadDouble();
        public override void Serialize(double obj, FastMemoryWriter writer, SerializerSettings settings)
            => writer.Write(obj);
        public override double DeserializeT(FastReader reader, SerializerSettings settings)
            => reader.ReadDouble();
    }

    public sealed class CharConverter : Converter<char>
    {
        public override Type Type => typeof(char);
        
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((char) obj);
        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadChar();
        public override void Serialize(char obj, FastMemoryWriter writer, SerializerSettings settings)
            => writer.Write(obj);
        public override char DeserializeT(FastReader reader, SerializerSettings settings)
            => reader.ReadChar();
    }

    public sealed class StringConverter : Converter<string>
    {
        public override Type Type => typeof(string);
        
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((string) obj);
        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadString();
        public override void Serialize(string obj, FastMemoryWriter writer, SerializerSettings settings)
            => writer.Write(obj);
        public override string DeserializeT(FastReader reader, SerializerSettings settings)
            => reader.ReadString();
    }
}
