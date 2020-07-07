using System;
using System.IO;
using FastStream;

// ReSharper disable UnusedMember.Global
#pragma warning disable 169
namespace SE.Serialization
{
    public abstract class TypeSerializer
    {
        internal virtual bool IsGeneric => false;
        internal virtual bool DoNotStore => false;

        public abstract Type Type { get; }
        public abstract object Deserialize(FastReader reader, SerializerSettings settings);
        public abstract void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings);
    }

    public abstract class GenericTypeSerializer : TypeSerializer
    {
        internal sealed override bool IsGeneric => true;

        public sealed override object Deserialize(FastReader reader, SerializerSettings settings) => throw new NotSupportedException();
        public sealed override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) => throw new NotSupportedException();

        public abstract object DeserializeGeneric(Type[] innerTypes, FastReader reader, SerializerSettings settings);
        public abstract void SerializeGeneric(object obj, Type[] innerTypes, FastMemoryWriter writer, SerializerSettings settings);
    }

    #region PRIMITIVES

    public sealed class BoolTypeSerializer : TypeSerializer
    {
        public override Type Type => typeof(bool);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadBoolean();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((bool) obj);
    }

    public sealed class ByteTypeSerializer : TypeSerializer
    {
        public override Type Type => typeof(byte);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadByte();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((byte) obj);
    }

    public sealed class SByteTypeSerializer : TypeSerializer
    {
        public override Type Type => typeof(sbyte);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadSByte();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((sbyte) obj);
    }

    public sealed class ShortTypeSerializer : TypeSerializer
    {
        public override Type Type => typeof(short);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadInt16();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((short) obj);
    }

    public sealed class UShortTypeSerializer : TypeSerializer
    {
        public override Type Type => typeof(ushort);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadUInt16();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((ushort) obj);
    }

    public sealed class IntTypeSerializer : TypeSerializer
    {
        public override Type Type => typeof(int);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadInt32();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((int) obj);
    }

    public sealed class UIntTypeSerializer : TypeSerializer
    {
        public override Type Type => typeof(uint);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadUInt32();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((uint) obj);
    }

    public sealed class LongTypeSerializer : TypeSerializer
    {
        public override Type Type => typeof(long);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadInt64();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((long) obj);
    }

    public sealed class ULongTypeSerializer : TypeSerializer
    {
        public override Type Type => typeof(ulong);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadUInt64();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((ulong) obj);
    }

    public sealed class FloatTypeSerializer : TypeSerializer
    {
        public override Type Type => typeof(float);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadSingle();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((float) obj);
    }

    public sealed class DoubleTypeSerializer : TypeSerializer
    {
        public override Type Type => typeof(double);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadDouble();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((double) obj);
    }

    public sealed class CharTypeSerializer : TypeSerializer
    {
        public override Type Type => typeof(char);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadChar();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((char) obj);
    }

    public sealed class StringTypeSerializer : TypeSerializer
    {
        public override Type Type => typeof(string);

        public override object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadString();
        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings) 
            => writer.Write((string) obj);
    }

    #endregion

    #region NULLABLES

    public sealed class NullableIntTypeSerializer : TypeSerializer
    {
        public override Type Type => typeof(int?);

        public override object Deserialize(FastReader reader, SerializerSettings settings)
        {
            if (!reader.ReadBoolean()) 
                return null;

            return reader.ReadInt32();
        }

        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings)
        {
            int? val = (int?) obj;
            bool hasValue = val.HasValue;
            writer.Write(hasValue);
            if (hasValue) {
                writer.Write(val.Value);
            }
        }
    }

    #endregion

    #region GENERICS

    public sealed class ArrayTypeSerializer : GenericTypeSerializer
    {
        public override Type Type => typeof(Array);
        internal override bool DoNotStore => true;

        public override object DeserializeGeneric(Type[] innerTypes, FastReader reader, SerializerSettings settings)
        {
            //if (!reader.ReadBoolean()) 
            //    return null;

            int arrLength = reader.ReadInt32();
            Array val = Array.CreateInstance(innerTypes[0], arrLength);
            TypeSerializer serializer = Serializer.GetSerializer(innerTypes[0]);
            for (int i = 0; i < arrLength; i++) {
                val.SetValue(Serializer.Deserialize(innerTypes[0], serializer, reader, settings), i);
            }

            return val;
        }

        public override void SerializeGeneric(object obj, Type[] innerTypes, FastMemoryWriter writer, SerializerSettings settings)
        {
            Array val = (Array) obj;
            writer.Write(val.Length);

            TypeSerializer serializer = Serializer.GetSerializer(innerTypes[0]);
            for (int i = 0; i < val.Length; i++) {
                Serializer.Serialize(writer, val.GetValue(i), innerTypes[0], serializer, settings);
            }
        }
    }

    public sealed class NullableTypeSerializer : GenericTypeSerializer
    {
        public override Type Type => typeof(Nullable<>);

        public override object DeserializeGeneric(Type[] innerTypes, FastReader reader, SerializerSettings settings)
        {
            if (!reader.ReadBoolean()) 
                return null;

            TypeSerializer serializer = Serializer.GetSerializer(innerTypes[0]);
            return serializer?.Deserialize(reader, settings);
        }

        public override void SerializeGeneric(object obj, Type[] innerTypes, FastMemoryWriter writer, SerializerSettings settings)
        {
            bool hasValue = obj != null;
            writer.Write(hasValue);
            if (hasValue) {
                Serializer.GetSerializer(innerTypes[0])?.Serialize(obj, writer, settings);
            }
        }
    }

    #endregion
}
#pragma warning restore 169