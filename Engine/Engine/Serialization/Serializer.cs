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

        private static SerializerSettings defaultSettings = new SerializerSettings {
            NullValueHandling = NullValueHandling.Ignore
        };

        private static bool isDirty = true;

        static Serializer()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (o, args) => {
                isDirty = true;
                return null;
            };
        }

        public static byte[] Serialize(object obj, SerializerSettings settings = null)
        {
            if (isDirty)
                Reset();
            if(settings == null)
                settings = defaultSettings;

            Type objType = obj.GetType();
            using(FastMemoryWriter writer = new FastMemoryWriter()) {
                ISerializer serializer = GetSerializer(objType);
                serializer.Serialize(writer, obj, settings);
                return writer.ToArray();
            }
        }

        public static T Deserialize<T>(byte[] data, SerializerSettings settings = null)
        {
            if (isDirty)
                Reset();
            if(settings == null)
                settings = defaultSettings;

            Type objType = typeof(T);
            MemoryStream stream = new MemoryStream(data);
            using (FastReader reader = new FastReader(stream)) {
                ISerializer serializer = GetSerializer(objType);
                return (T) serializer.Deserialize(reader, settings);
            }

            return default;
        }

        internal static ISerializer GetSerializer(Type objType)
        {
            // Step 1: Check if the concrete type is in the ValueSerializersType dictionary.
            if(ValueSerializersType.TryGetValue(objType, out IValueSerializer serializer)) {
                return serializer;
            } 
                
            // Step 2: Check if the type implements an interface within the ValueSerializersType dictionary.
            foreach (Type intType in objType.GetInterfaces()) {
                if (ValueSerializersType.TryGetValue(intType, out serializer)) {
                    return serializer;
                }
            }

            // Step 3: Try and retrieve or generated a GeneratedSerializer for the given type.
            GeneratedSerializer generatedSerializer = GetGeneratedSerializer(objType);
            return generatedSerializer;
        }

        private static GeneratedSerializer GetGeneratedSerializer(Type type)
        {
            if (!GeneratedSerializers.TryGetValue(type, out GeneratedSerializer serializer)) {
                serializer = GeneratedSerializer.Generate(type);
                GeneratedSerializers.Add(type, serializer);
            }
            return serializer;
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
