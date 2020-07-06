using System;
using System.IO;
using FastStream;

namespace SE.Serialization
{
    public interface ISerializer
    {
        object Deserialize(FastReader reader);
        void Serialize(FastMemoryWriter writer, object obj);
    }

    public interface IValueSerializer : ISerializer
    {
        Type ValueType { get; }
    }

    public class IntValueSerializer : IValueSerializer
    {
        public Type ValueType => typeof(int);

        public object Deserialize(FastReader reader)
        {
            return reader.ReadInt32();
        }

        public void Serialize(FastMemoryWriter writer, object obj)
        {
            writer.Write((int) obj);
        }
    }
}
