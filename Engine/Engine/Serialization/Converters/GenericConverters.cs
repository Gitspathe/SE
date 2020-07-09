using System;
using FastStream;

namespace SE.Serialization.Converters
{
    public sealed class ArrayConverter : GenericConverter
    {
        public override Type Type => typeof(Array);

        public override object Deserialize(FastReader reader, SerializerSettings settings)
        {
            //if (!reader.ReadBoolean()) 
            //    return null;

            int arrLength = reader.ReadInt32();
            Array val = Array.CreateInstance(InnerTypes[0], arrLength);
            Converter serializer = settings.Resolver.GetConverter(InnerTypes[0]);
            for (int i = 0; i < arrLength; i++) {
                val.SetValue(Serializer.Deserialize(serializer, reader, settings), i);
            }

            return val;
        }

        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings)
        {
            Array val = (Array) obj;
            writer.Write(val.Length);

            Converter serializer = settings.Resolver.GetConverter(InnerTypes[0]);
            for (int i = 0; i < val.Length; i++) {
                Serializer.Serialize(val.GetValue(i), settings, serializer, writer);
            }
        }

        public override bool IsDefault(object obj)
        {
            if (obj is Array array) {
                return array.Length < 1;
            }
            return true;
        }
    }

    public sealed class NullableConverter : GenericConverter
    {
        public override Type Type => typeof(Nullable<>);

        public override object Deserialize(FastReader reader, SerializerSettings settings)
        {
            if (!reader.ReadBoolean()) 
                return null;

            Converter serializer = settings.Resolver.GetConverter(InnerTypes[0]);
            return serializer?.Deserialize(reader, settings);
        }

        public override void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings)
        {
            bool hasValue = obj != null;
            writer.Write(hasValue);
            if (hasValue) {
                settings.Resolver.GetConverter(InnerTypes[0])?.Serialize(obj, writer, settings);
            }
        }
    }
}
