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
        private bool hasDefault;

        public abstract Type Type { get; }
        public abstract object Deserialize(FastReader reader, SerializerSettings settings);
        public abstract void Serialize(object obj, FastMemoryWriter writer, SerializerSettings settings);

        public virtual bool IsDefault(object obj)
        {
            if (!defaultCreated)
                GenerateDefaultValue();

            return hasDefault && obj.Equals(defaultInstance);
        }

        internal void GenerateDefaultValue()
        {
            try {
                if (Type != null) {
                    defaultInstance = Activator.CreateInstance(Type);
                    hasDefault = true;
                }
            } catch (Exception) {
                 /* ignored */
            } finally {
                defaultCreated = true;
            }
        }

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
