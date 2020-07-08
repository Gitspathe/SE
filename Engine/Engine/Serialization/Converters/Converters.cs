using System;
using FastStream;

namespace SE.Serialization.Converters
{
    /// <summary>
    /// Serializes non-generic (or generic with known types) objects of a specified type.
    /// Only one instance of each TypeSerializer is generated at runtime.
    /// </summary>
    public abstract class Converter
    {
        /// <summary>If true, the TypeSerializer is generated and stored at runtime using reflection.</summary>
        internal virtual bool StoreAtRuntime => true;

        public abstract Type Type { get; }
        public abstract object Deserialize(FastReader reader, SerializerSettings settings);
        public abstract void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings);

        public Converter() { /* Empty constructor for reflection. */ }
    }

    /// <summary>
    /// Serializes generic objects of a specified underlying generic type.
    /// One GenericTypeSerializer instance for EACH inner type combination is generated at runtime.
    /// </summary>
    public abstract class GenericConverter : Converter
    {
        /// <summary>Generic type arguments.</summary>
        protected internal Type[] InnerTypes;

        public GenericConverter() { /* Empty constructor for reflection. */ }
    }
}
