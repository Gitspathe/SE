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

        private object defaultInstance;
        private bool defaultCreated;

        public abstract Type Type { get; }
        public abstract void Serialize(object obj, FastMemoryWriter writer, ref SerializeTask task);
        public abstract object Deserialize(FastReader reader, ref DeserializeTask task);

        public virtual bool IsDefault(object obj)
        {
            if (!defaultCreated)
                GenerateDefaultValue();

            return obj.Equals(defaultInstance);
        }

        internal void GenerateDefaultValue()
        {
            try {
                if (Type != null) {
                    defaultInstance = Type.IsValueType ? Activator.CreateInstance(Type) : null;
                }
            } catch (Exception) {
                defaultInstance = null;
            } finally {
                defaultCreated = true;
            }
        }

        public Converter() { /* Empty constructor for reflection. */ }
    }

    public abstract class Converter<T> : Converter
    {
        public abstract void Serialize(T obj, FastMemoryWriter writer, ref SerializeTask task);
        public abstract T DeserializeT(FastReader reader, ref DeserializeTask task);
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

    /// <summary>
    /// Serializes generic objects of a specified underlying generic type.
    /// One GenericTypeSerializer instance for EACH inner type combination is generated at runtime.
    /// </summary>
    public abstract class GenericConverter<T> : Converter<T>
    {
        /// <summary>Generic type arguments.</summary>
        protected internal Type[] InnerTypes;

        public GenericConverter() { /* Empty constructor for reflection. */ }
    }
}
