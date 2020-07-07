using SE.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using static SE.Core.ReflectionUtil;
using System.IO;
using FastStream;

namespace SE.Serialization
{
    public static class Serializer
    {
        private static Dictionary<Type, TypeSerializer> serializerCache = new Dictionary<Type, TypeSerializer>();
        private static Dictionary<Type, Type[]> genericInnerTypeCache = new Dictionary<Type, Type[]>();

        private static Dictionary<Type, GeneratedSerializer> generatedSerializers = new Dictionary<Type, GeneratedSerializer>();
        private static Dictionary<Type, TypeSerializer> typeSerializers = new Dictionary<Type, TypeSerializer>();
        private static Dictionary<Type, GenericTypeSerializer> genericTypeSerializers = new Dictionary<Type, GenericTypeSerializer>();

        private static ArrayTypeSerializer arraySerializer = new ArrayTypeSerializer();

        private static Func<Type, bool> serializerPredicate = myType 
            => myType != typeof(GeneratedSerializer) 
               && myType.IsClass 
               && !myType.IsAbstract 
               && typeof(TypeSerializer).IsAssignableFrom(myType);

        private static Func<Type, bool> genericSerializerPredicate = myType 
            => myType != typeof(GeneratedSerializer) 
               && myType.IsClass 
               && !myType.IsAbstract 
               && typeof(GenericTypeSerializer).IsAssignableFrom(myType);

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
            ResetIfNeeded();
            if(obj == null)
                return null;
            if(settings == null)
                settings = defaultSettings;

            Type objType = obj.GetType();
            using(FastMemoryWriter writer = new FastMemoryWriter()) {
                TypeSerializer serializer = GetSerializer(objType);
                if (serializer == null)
                    return null;

                Serialize(writer, obj, objType, serializer, settings);
                return writer.ToArray();
            }
        }

        public static void Serialize(FastMemoryWriter writer, object obj, Type objType, TypeSerializer serializer, SerializerSettings settings = null)
        {
            if (serializer.IsGeneric) {
                ((GenericTypeSerializer) serializer).SerializeGeneric(obj, GetGenericInnerTypes(objType), writer, settings);
            } else {
                serializer.Serialize(obj, writer, settings);
            }
        }

        public static T Deserialize<T>(byte[] data, SerializerSettings settings = null)
        {
            ResetIfNeeded();
            if(data == null)
                return default;
            if(settings == null)
                settings = defaultSettings;

            Type objType = typeof(T);
            TypeSerializer serializer = GetSerializer(objType);
            MemoryStream stream = new MemoryStream(data);
            using (FastReader reader = new FastReader(stream)) {
                return (T) Deserialize(objType, serializer, reader, settings);
            }
        }

        public static object Deserialize(Type type, TypeSerializer serializer, FastReader reader, SerializerSettings settings = null)
        {
            ResetIfNeeded();
            if(settings == null)
                settings = defaultSettings;

            if (serializer.IsGeneric) {
                return ((GenericTypeSerializer) serializer).DeserializeGeneric(GetGenericInnerTypes(type), reader, settings);
            }
            return serializer.Deserialize(reader, settings);
        }

        public static TypeSerializer GetSerializer(Type objType)
        {
            // Step 1: Try and retrieve serializer from the global cache.
            if (serializerCache.TryGetValue(objType, out TypeSerializer serializer)) {
                return serializer;
            }

            // Step 2: Test to see if it's a specially recognized type.
            // A) Flat array.
            if (objType.IsArray) {
                serializerCache.Add(objType, arraySerializer);
                genericInnerTypeCache.Add(objType, new [] { objType.GetElementType() });
                return arraySerializer;
            }

            // B) TODO: Jagged array.
            // C) TODO: Multidimensional array.

            // Step 3: Check if the concrete type is in the ValueSerializersType dictionary.
            if(typeSerializers.TryGetValue(objType, out serializer)) {
                serializerCache.Add(objType, serializer);
                return serializer;
            } 
                
            // Step 4: Check if the type implements an interface within the ValueSerializersType dictionary.
            foreach (Type intType in objType.GetInterfaces()) {
                if (typeSerializers.TryGetValue(intType, out serializer)) {
                    serializerCache.Add(objType, serializer);
                    return serializer;
                }
            }

            // Step 5: Check if the type is generic.
            if (objType.IsGenericType) {
                if (genericTypeSerializers.TryGetValue(objType.GetGenericTypeDefinition(), out GenericTypeSerializer genericSerializer)) {
                    serializerCache.Add(objType, genericSerializer);
                    genericInnerTypeCache.Add(objType, objType.GetGenericArguments());
                    return genericSerializer;
                }
            }

            // Step 6: Try and retrieve or generate a serializer for the given type.
            GeneratedSerializer generatedSerializer = GetGeneratedSerializer(objType);
            serializerCache.Add(objType, generatedSerializer);
            return generatedSerializer;
        }

        internal static Type[] GetGenericInnerTypes(Type objType)
        {
            if (genericInnerTypeCache.TryGetValue(objType, out Type[] inner)) {
                return inner;
            }
            inner = objType.GetGenericArguments();
            genericInnerTypeCache.Add(objType, inner);
            return inner;
        }

        private static GeneratedSerializer GetGeneratedSerializer(Type type)
        {
            if (!generatedSerializers.TryGetValue(type, out GeneratedSerializer serializer)) {
                serializer = GeneratedSerializer.Generate(type);
                generatedSerializers.Add(type, serializer);
            }
            return serializer;
        }

        private static void Reset()
        {
            typeSerializers.Clear();
            genericTypeSerializers.Clear();

            IEnumerable<TypeSerializer> enumerable = GetTypeInstances<TypeSerializer>(serializerPredicate);
            foreach (TypeSerializer valSerializer in enumerable) {
                if(!valSerializer.DoNotStore)
                    typeSerializers.Add(valSerializer.Type, valSerializer);
            }

            IEnumerable<GenericTypeSerializer> genericEnumerable = GetTypeInstances<GenericTypeSerializer>(genericSerializerPredicate);
            foreach (GenericTypeSerializer valSerializer in genericEnumerable) {
                if(!valSerializer.DoNotStore)
                    genericTypeSerializers.Add(valSerializer.Type, valSerializer);
            }

            isDirty = false;
        }

        private static void ResetIfNeeded()
        {
            if (isDirty)
                Reset();
        }
    }
}
