using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SE.Serialization.Converters;
using static SE.Core.ReflectionUtil;

namespace SE.Serialization.Resolvers
{
    public sealed class DefaultResolver : ConverterResolver
    {
        private Dictionary<Type, Converter> converterCache               = new Dictionary<Type, Converter>();
        private Dictionary<Type, Converter> typeConverters               = new Dictionary<Type, Converter>();
        private Dictionary<Type, GeneratedConverter> generatedConverters = new Dictionary<Type, GeneratedConverter>();
        private Dictionary<Type, Type> genericTypeConverterTypes         = new Dictionary<Type, Type>();

        private Func<Type, bool> converterPredicate = myType 
            => myType != typeof(GeneratedConverter) 
               && myType.IsClass 
               && !myType.IsAbstract 
               && typeof(Converter).IsAssignableFrom(myType);

        private Func<Type, bool> genericConverterPredicate = myType
            => typeof(GenericConverter).IsAssignableFrom(myType) 
               && myType != typeof(GeneratedConverter)
               && myType.IsClass
               && !myType.IsAbstract;

        private bool isDirty = true;

        public DefaultResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (o, args) => {
                isDirty = true;
                return null;
            };
        }

        private static class ResolverCache<T>
        {
            public static Converter<T> Converter;
            public static bool Setup = false;
        }

        private void RegisterConverter(Type objType, Converter converter)
        {
            converterCache.Add(objType, converter);
        }

        public override Converter GetConverter(Type objType)
        {
            ResetIfNeeded();

            // Step 1: Try and retrieve serializer from the global cache.
            if (converterCache.TryGetValue(objType, out Converter converter)) {
                return converter;
            }

            // Step 2: Check if the concrete type is in the ValueSerializersType dictionary.
            converter = GetTypeConverter(objType);
            if(converter != null) {
                RegisterConverter(objType, converter);
                return converter;
            } 

            // Step 3: Test to see if it's a specially recognized type.
            // A) Flat array.
            if (objType.IsArray) {
                Type[] innerTypes = { objType.GetElementType() };
                return CreateGenericTypeConverter(objType, typeof(Array), innerTypes);
            }

            // B) TODO: Multidimensional array. NOTE: Jagged array seems to already work!!
                
            // Step 4: Check if the type implements an interface within the ValueSerializersType dictionary.
            foreach (Type intType in objType.GetInterfaces()) {
                converter = GetTypeConverter(intType);
                if (converter != null) {
                    RegisterConverter(objType, converter);
                    return converter;
                }
            }

            // Step 5: Check if the type is generic.
            if (objType.IsGenericType) {
                return CreateGenericTypeConverter(objType, objType.GetGenericTypeDefinition(), objType.GenericTypeArguments);
            }

            // TODO: Generic interfaces?

            // Step 6: Try and retrieve or generate a serializer for the given type.
            GeneratedConverter generatedConverter = GetGeneratedConverter(objType);
            RegisterConverter(objType, generatedConverter);
            return generatedConverter;
        }

        public override Converter<T> GetConverter<T>()
        {
            if (ResolverCache<T>.Setup) {
                return ResolverCache<T>.Converter;
            }

            Converter converter = GetConverter(typeof(T));
            if (converter is Converter<T> converterT) {
                ResolverCache<T>.Converter = converterT;
            }
            ResolverCache<T>.Setup = true;
            return ResolverCache<T>.Converter;
        }

        public Converter GetTypeConverter(Type type)
        {
            if (typeConverters.TryGetValue(type, out Converter serializer)) {
                return serializer;
            }
            return null;
        }

        public GenericConverter CreateGenericTypeConverter(Type concreteType, Type genericType, Type[] innerTypes)
        {
            if (genericTypeConverterTypes.TryGetValue(genericType, out Type serializerType)) {
                GenericConverter newSerializer = (GenericConverter) Activator.CreateInstance(serializerType);
                newSerializer.InnerTypes = innerTypes;
                RegisterConverter(concreteType, newSerializer);
                return newSerializer;
            }
            return null;
        }

        public GeneratedConverter GetGeneratedConverter(Type type)
        {
            if (!generatedConverters.TryGetValue(type, out GeneratedConverter serializer)) {
                serializer = GeneratedConverter.Create(type, this);
                generatedConverters.Add(type, serializer);
            }
            return serializer;
        }

        private void Reset()
        {
            typeConverters.Clear();

            IEnumerable<Converter> enumerable = GetTypeInstances<Converter>(converterPredicate);
            foreach (Converter valSerializer in enumerable) {
                if(valSerializer.StoreAtRuntime)
                    typeConverters.Add(valSerializer.Type, valSerializer);
            }

            IEnumerable<GenericConverter> genericEnumerable = GetTypeInstances<GenericConverter>(genericConverterPredicate);
            foreach (GenericConverter genericTypeSerializer in genericEnumerable) {
                if(genericTypeSerializer.StoreAtRuntime)
                    genericTypeConverterTypes.Add(genericTypeSerializer.Type, genericTypeSerializer.GetType());
            }

            isDirty = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetIfNeeded()
        {
            if (isDirty)
                Reset();
        }
    }
}
