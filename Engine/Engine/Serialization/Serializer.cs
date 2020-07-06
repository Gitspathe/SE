using SE.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using SE.Engine.Networking.Packets;
using static SE.Core.ReflectionUtil;
using System.IO;
using FastStream;

namespace SE.Serialization
{
    public static class Serializer
    {
        internal static Dictionary<Type, GeneratedSerializer> GeneratedSerializers = new Dictionary<Type, GeneratedSerializer>();

        internal static Dictionary<Type, IValueSerializer> ValueSerializersType = new Dictionary<Type, IValueSerializer>();
        internal static Dictionary<IValueSerializer, Type> TypeValueSerializers = new Dictionary<IValueSerializer, Type>();

        private static Func<Type, bool> serializerPredicate = myType 
            => myType.IsClass && !myType.IsAbstract && typeof(IValueSerializer).IsAssignableFrom(myType);

        private static bool isDirty = true;

        static Serializer()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (o, args) => {
                isDirty = true;
                return null;
            };
        }

        public static byte[] Serialize(object obj)
        {
            if (isDirty)
                Reset();

            Type objType = obj.GetType();
            using(FastMemoryWriter writer = new FastMemoryWriter()) {
                if(ValueSerializersType.TryGetValue(objType, out IValueSerializer valueSerializer)) {
                    valueSerializer.Serialize(writer, obj);
                } else {
                    // Check if type is a serializable class or struct.
                    // And generate an object to serialize it.
                    GeneratedSerializer serializer = GetSerializer(objType);
                    serializer.Serialize(writer, obj);
                }
                return writer.ToArray();
            }
        }

        internal static GeneratedSerializer GetSerializer(Type type)
        {
            if (!GeneratedSerializers.TryGetValue(type, out GeneratedSerializer serializer)) {
                serializer = GeneratedSerializer.Generate(type);
                GeneratedSerializers.Add(type, serializer);
            }
            return serializer;
        }

        public static T Deserialize<T>(byte[] data)
        {
            if (isDirty)
                Reset();

            Type objType = typeof(T);
            MemoryStream stream = new MemoryStream(data);
            using (FastReader reader = new FastReader(stream)) {
                if(ValueSerializersType.TryGetValue(objType, out IValueSerializer valueSerializer)) {
                    return (T) valueSerializer.Deserialize(reader);
                }

                // Check if type is a serializable class or struct.
                // And generate an object to serialize it.
                GeneratedSerializer serializer = GetSerializer(objType);

                if (serializer != null) {
                    return serializer.Deserialize<T>(reader);
                }
            }

            return default;
        }

        private static void Reset()
        {
            ValueSerializersType.Clear();
            TypeValueSerializers.Clear();

            IEnumerable<IValueSerializer> enumerable = GetTypeInstances<IValueSerializer>(serializerPredicate);
            foreach (IValueSerializer valSerializer in enumerable) {
                ValueSerializersType.Add(valSerializer.ValueType, valSerializer);
                TypeValueSerializers.Add(valSerializer, valSerializer.ValueType);
            }

            isDirty = false;
        }
    }
}
