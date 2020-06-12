using System;
using FastMember;

namespace SE.Serialization
{
    public class SerializedValue
    {
        public string Name;
        public bool Override;

        private TypeAccessor accessor;
        private object obj;

        public dynamic Value {
            get => accessor[obj, Name];
            set {
                if (Override)
                    accessor[obj, Name] = value;
            }
        }

        // Cached values from reflection.
        public Cache ReflectionInfo;

        internal void Restore(SerializedValueData serializedData)
        {
            Override = serializedData.Override;

            // Handle special cases where built-in json deserialization fails (floats for example).
            // If no special case is found, use built-in deserialization.
            if (serializedData.ValueConverter != null) {
                SerializerReflection.TypeTable.TryGetValue(serializedData.ValueConverter.Value, out Type t);
                serializedData.Type = t;
                if (SerializerReflection.RestoreTable.TryGetValue(serializedData.Type, out Action<SerializedValueData, SerializedValue> func)) { 
                    func.Invoke(serializedData, this);
                    return;
                }
            }
            Value = serializedData.Value;
        }

        public SerializedValue(TypeAccessor accessor, object obj, string name)
        {
            this.accessor = accessor;
            this.obj = obj;
            Name = name;
            Value = accessor[obj, Name];
        }

        public class Cache
        {
            public Type Type;
            public VarType ValueType;

            public Type genericType;
            public Type innerType;
        }

        public enum VarType
        {
            None,
            Class,
            GenericClass,
            Interface,
            GenericInterface
        }
    }
}
