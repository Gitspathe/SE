using System;
using FastStream;
using SE.Core;
using static SE.Serialization.Constants;

namespace SE.Serialization.Converters
{
    public sealed class ArrayConverter : GenericConverter
    {
        public override Type Type => typeof(Array);

        public override object DeserializeBinary(Utf8Reader reader, ref DeserializeTask task)
        {
            int arrLength = reader.ReadInt32();
            Array val = Array.CreateInstance(TypeArguments[0], arrLength);
            Converter serializer = GetSerializer(0, ref task);
            for (int i = 0; i < arrLength; i++) {
                val.SetValue(Serializer.DeserializeReader(reader, serializer, ref task), i);
            }

            return val;
        }

        public override void SerializeBinary(object obj, Utf8Writer writer, ref SerializeTask task)
        {
            Array val = (Array) obj;
            writer.Write(val.Length);

            Converter serializer = GetSerializer(0, ref task);
            for (int i = 0; i < val.Length; i++) {
                Serializer.SerializeWriter(writer, val.GetValue(i), serializer, ref task, false);
            }
        }

        public override object DeserializeText(Utf8Reader reader, ref DeserializeTask task)
        {
            ////if (!reader.ReadBoolean()) 
            ////    return null;

            //int arrLength = reader.ReadInt32();
            //Array val = Array.CreateInstance(TypeArguments[0], arrLength);
            //Converter serializer = GetSerializer(0, ref task);
            //for (int i = 0; i < arrLength; i++)
            //{
            //    val.SetValue(Serializer.DeserializeReader(serializer, reader, ref task), i);
            //}

            //return val;

            throw new NotImplementedException();
        }

        public override void SerializeText(object obj, Utf8Writer writer, ref SerializeTask task)
        {
            Array val = (Array)obj;
            Converter serializer = GetSerializer(0, ref task);

            writer.Write(_BEGIN_ARRAY);
            for (int i = 0; i < val.Length; i++) {
                Serializer.SerializeWriter(writer, val.GetValue(i), serializer, ref task, false);
                if (i + 1 < val.Length) {
                    writer.Write(_ARRAY_SEPARATOR);
                }
            }
            writer.Write(_END_ARRAY);
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

        public override object DeserializeBinary(Utf8Reader reader, ref DeserializeTask task)
        {
            if (!reader.ReadBoolean()) 
                return null;

            Converter serializer = task.Settings.Resolver.GetConverter(TypeArguments[0]);
            return serializer?.DeserializeBinary(reader, ref task);
        }

        public override void SerializeBinary(object obj, Utf8Writer writer, ref SerializeTask task)
        {
            bool hasValue = obj != null;
            writer.Write(hasValue);
            if (hasValue) {
                task.Settings.Resolver.GetConverter(TypeArguments[0])?.SerializeBinary(obj, writer, ref task);
            }
        }
    }
}
