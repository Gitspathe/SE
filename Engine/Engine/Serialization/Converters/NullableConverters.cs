﻿using System;
using FastStream;

namespace SE.Serialization.Converters
{
    public sealed class NullableIntConverter : Converter<int?>
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

        public override int? DeserializeT(FastReader reader, SerializerSettings settings)
        {
            if (!reader.ReadBoolean()) 
                return null;

            return reader.ReadInt32();
        }

        public override void Serialize(int? obj, FastMemoryWriter writer, SerializerSettings settings)
        {
            bool hasValue = obj.HasValue;
            writer.Write(hasValue);
            if (hasValue) {
                writer.Write(obj.Value);
            }
        }
    }
}
