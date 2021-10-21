using System;

namespace SE.Serialization.Converters
{
    public sealed class NullableIntConverter : Converter<int?>
    {
        public override Type Type => typeof(int?);

        public override int? DeserializeTBinary(Utf8Reader reader, ref DeserializeTask task)
        {
            if (!reader.ReadBoolean())
                return null;

            return reader.ReadInt32();
        }

        public override void SerializeBinary(int? obj, Utf8Writer writer, ref SerializeTask task)
        {
            bool hasValue = obj.HasValue;
            writer.Write(hasValue);
            if (hasValue) {
                writer.Write(obj.Value);
            }
        }
    }
}
