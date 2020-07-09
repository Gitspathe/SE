using System;
using FastStream;

namespace SE.Serialization.Converters
{
    public sealed class BoolConverter : Converter
    {
        public override Type Type => typeof(bool);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadBoolean();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((bool) obj);
    }

    public sealed class ByteConverter : Converter
    {
        public override Type Type => typeof(byte);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadByte();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((byte) obj);
    }

    public sealed class SByteConverter : Converter
    {
        public override Type Type => typeof(sbyte);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadSByte();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((sbyte) obj);
    }

    public sealed class ShortConverter : Converter
    {
        public override Type Type => typeof(short);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadInt16();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((short) obj);
    }

    public sealed class UShortConverter : Converter
    {
        public override Type Type => typeof(ushort);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadUInt16();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((ushort) obj);
    }

    public sealed class IntConverter : Converter<int>
    {
        public override Type Type => typeof(int);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadInt32();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((int) obj);
        public override int DeserializeT(FastReader reader, SerializerSettings settings)
            => reader.ReadInt32();
        public override void Serialize(int obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write(obj);
    }

    public sealed class UIntConverter : Converter
    {
        public override Type Type => typeof(uint);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadUInt32();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((uint) obj);
    }

    public sealed class LongConverter : Converter
    {
        public override Type Type => typeof(long);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadInt64();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((long) obj);
    }

    public sealed class ULongConverter : Converter
    {
        public override Type Type => typeof(ulong);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadUInt64();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((ulong) obj);
    }

    public sealed class FloatConverter : Converter<float>
    {
        public override Type Type => typeof(float);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadSingle();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((float) obj);
        public override float DeserializeT(FastReader reader, SerializerSettings settings)
            => reader.ReadSingle();
        public override void Serialize(float obj, FastMemoryWriter writer, SerializerSettings settings)
            => writer.Write(obj);
    }

    public sealed class DoubleConverter : Converter
    {
        public override Type Type => typeof(double);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadDouble();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((double) obj);
    }

    public sealed class CharConverter : Converter
    {
        public override Type Type => typeof(char);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadChar();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((char) obj);
    }

    public sealed class StringConverter : Converter
    {
        public override Type Type => typeof(string);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadString();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((string) obj);
    }
}
