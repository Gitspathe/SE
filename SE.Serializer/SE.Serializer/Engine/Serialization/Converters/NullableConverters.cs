using System;
using FastStream;

namespace SE.Serialization.Converters
{
    public sealed class NullableIntConverter : Converter<int?>
    {
        public override Type Type => typeof(int?);

        public override object DeserializeBinary(FastReader reader, ref DeserializeTask task)
        {
            if (!reader.ReadBoolean()) 
                return null;

            return reader.ReadInt32();
        }

        public override void SerializeBinary(object obj, FastMemoryWriter writer, ref SerializeTask task)
        {
            int? val = (int?) obj;
            bool hasValue = val.HasValue;
            writer.Write(hasValue);
            if (hasValue) {
                writer.Write(val.Value);
            }
        }

        public override int? DeserializeTBinary(FastReader reader, ref DeserializeTask task)
        {
            if (!reader.ReadBoolean()) 
                return null;

            return reader.ReadInt32();
        }

        public override void SerializeBinary(int? obj, FastMemoryWriter writer, ref SerializeTask task)
        {
            bool hasValue = obj.HasValue;
            writer.Write(hasValue);
            if (hasValue) {
                writer.Write(obj.Value);
            }
        }
    }
}
