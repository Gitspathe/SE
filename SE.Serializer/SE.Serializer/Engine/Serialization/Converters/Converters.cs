using System;
using System.Threading.Tasks;
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
        private bool isObject;

        public abstract Type Type { get; }
        public abstract void Serialize(object obj, FastMemoryWriter writer, ref SerializeTask task);
        public abstract object Deserialize(FastReader reader, ref DeserializeTask task);

        public virtual bool IsDefault(object obj)
        {
            if (!defaultCreated)
                GenerateDefaultValue();

            if (isObject) {
                return obj == null;
            }
            return obj.Equals(defaultInstance);
        }

        internal void GenerateDefaultValue()
        {
            try {
                if (Type == typeof(object)) {
                    isObject = true;
                } else if(Type != null) {
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
        protected internal Type[] TypeArguments;

        protected Converter GetSerializer(int typeArgumentIndex, ref SerializeTask task)
        {
            // TODO: Handle TypeHandling settings.
            return task.Settings.Resolver.GetConverter(TypeArguments[typeArgumentIndex]);
        }

        protected Converter GetSerializer(int typeArgumentIndex, ref DeserializeTask task)
        {
            // TODO: Handle TypeHandling settings.
            return task.Settings.Resolver.GetConverter(TypeArguments[typeArgumentIndex]);
        }

        public GenericConverter() { /* Empty constructor for reflection. */ }
    }
}
