﻿using System;
using System.IO;
using FastStream;

namespace SE.Serialization
{
    public interface ISerializer
    {
        object Deserialize(FastReader reader, SerializerSettings settings);
        void Serialize(FastMemoryWriter writer, object obj, SerializerSettings settings);
    }

    public interface IValueSerializer : ISerializer
    {
        Type ValueType { get; }
    }

    public class IntValueSerializer : IValueSerializer
    {
        public Type ValueType => typeof(int);

        public object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadInt32();
        public void Serialize(FastMemoryWriter writer, object obj, SerializerSettings settings) 
            => writer.Write((int) obj);
    }

    public class FloatValueSerializer : IValueSerializer
    {
        public Type ValueType => typeof(float);

        public object Deserialize(FastReader reader, SerializerSettings settings) 
            => reader.ReadSingle();
        public void Serialize(FastMemoryWriter writer, object obj, SerializerSettings settings) 
            => writer.Write((float) obj);
    }
}
