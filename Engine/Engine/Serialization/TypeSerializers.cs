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

    #endregion

    #region NULLABLES

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