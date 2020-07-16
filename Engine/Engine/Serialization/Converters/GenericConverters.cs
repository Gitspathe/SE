using System;
using FastStream;

namespace SE.Serialization.Converters
{
    public sealed class ArrayConverter : GenericConverter
    {
        public override Type Type => typeof(Array);

        public override object Deserialize(FastReader reader, ref SerializerTask task)
        {
            //if (!reader.ReadBoolean()) 
            //    return null;

            int arrLength = reader.ReadInt32();
            Array val = Array.CreateInstance(InnerTypes[0], arrLength);
            Converter serializer = task.Settings.Resolver.GetConverter(InnerTypes[0]);
            for (int i = 0; i < arrLength; i++) {
                val.SetValue(Serializer.DeserializeReader(serializer, reader, ref task), i);
            }

            return val;
        }

        public override void Serialize(object obj, FastMemoryWriter writer, ref SerializerTask task)
        {
            Array val = (Array) obj;
            writer.Write(val.Length);

            Converter serializer = task.Settings.Resolver.GetConverter(InnerTypes[0]);
            for (int i = 0; i < val.Length; i++) {
                Serializer.SerializeWriter(val.GetValue(i), serializer, writer, ref task);
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

        public override object Deserialize(FastReader reader, ref SerializerTask task)
        {
            if (!reader.ReadBoolean()) 
                return null;

            Converter serializer = task.Settings.Resolver.GetConverter(InnerTypes[0]);
            return serializer?.Deserialize(reader, ref task);
        }

        public override void Serialize(object obj, FastMemoryWriter writer, ref SerializerTask task)
        {
            bool hasValue = obj != null;
            writer.Write(hasValue);
            if (hasValue) {
                task.Settings.Resolver.GetConverter(InnerTypes[0])?.Serialize(obj, writer, ref task);
            }
        }
    }
}
