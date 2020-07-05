using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using SE.Attributes;
using SE.Core.Extensions;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;
using System.Collections;
using SE.Core.Internal;
using static SE.Core.ReflectionUtil;

namespace SE.Serialization
{
    public static class SerializerReflection
    {
        public static Dictionary<Type, Action<SerializedValueData, SerializedValue>> RestoreTable = new Dictionary<Type, Action<SerializedValueData, SerializedValue>> {
            {
                typeof(byte),
                (serialized, existing) => existing.Value = (byte) serialized.Value
            },
            {
                typeof(sbyte),
                (serialized, existing) => existing.Value = (sbyte) serialized.Value
            },
            {
                typeof(float),
                (serialized, existing) => existing.Value = (float) serialized.Value
            },
            {
                typeof(double),
                (serialized, existing) => existing.Value = (double) serialized.Value
            },
            {
                typeof(uint),
                (serialized, existing) => existing.Value = (uint) serialized.Value
            },
            {
                typeof(short),
                (serialized, existing) => existing.Value = (short) serialized.Value
            },
            {
                typeof(ushort),
                (serialized, existing) => existing.Value = (ushort) serialized.Value
            },
            {
                typeof(long),
                (serialized, existing) => existing.Value = (long) serialized.Value
            },
            {
                typeof(ulong),
                (serialized, existing) => existing.Value = (ulong) serialized.Value
            },
        };

        public static SelfReferenceDictionary<int, Type> TypeTable = new SelfReferenceDictionary<int, Type>();
        internal static Dictionary<Type, SerializerInfo> ObjectSerializers = new Dictionary<Type, SerializerInfo>();
        internal static Dictionary<Type, Type> EngineSerializers = new Dictionary<Type, Type>();

        public static HashSet<Type> SerializableTypes = new HashSet<Type>
        {
            typeof(bool), typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
            typeof(float), typeof(double),
            typeof(string), typeof(Color), typeof(Vector2), typeof(Rectangle), typeof(RectangleF),
            typeof(IList), typeof(Array)
        };

        public static void RegenerateEngineSerializers()
        {
            EngineSerializers.Clear();
            IEnumerable<Type> enumerable = GetTypes(type => type.IsClass && type.InheritsFromGeneric(typeof(EngineSerializer<>)));
            foreach (Type type in enumerable) {
                if (type.BaseType != null) {
                    EngineSerializers.Add(type.BaseType.GetGenericArguments()[0], type);
                }
            }
        }

        internal static EngineSerializerBase GetEngineSerializer<T>(T obj)
        {
            if (EngineSerializers.TryGetValue(typeof(T), out Type result)) {
                return (EngineSerializerBase)Activator.CreateInstance(result, obj);
            }
            return null;
        }

        internal static EngineSerializerBase GetEngineSerializer(Type type, dynamic obj)
        {
            if (EngineSerializers.TryGetValue(type, out Type result)) {
                return (EngineSerializerBase)Activator.CreateInstance(result, obj);
            }
            return null;
        }

        public static void RegenerateSerializers()
        {
            ObjectSerializers.Clear();
            IEnumerable<Type> enumerable = GetTypes(type => type.IsClass && type.GetInterfaces().Contains(typeof(ISerializedObject)));
            foreach (Type type in enumerable) {
                if (type.GetCustomAttribute(typeof(NoSerializeAttribute)) != null)
                    continue;

                QuickList<string> strList = new QuickList<string>();

                IEnumerable<PropertyInfo> properties = type
                   .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                   .Where(prop => prop.CanRead && prop.CanWrite);
                IEnumerable<FieldInfo> fields = type
                   .GetFields(BindingFlags.Instance | BindingFlags.Public);

                foreach (PropertyInfo pInfo in properties) {
                    if (pInfo.GetCustomAttribute(typeof(NoSerializeAttribute)) != null)
                        continue;

                    Type t = pInfo.PropertyType;
                    bool isGenericField = t.IsGenericType;
                    if (SerializableTypes.Contains(pInfo.PropertyType)) {
                        strList.Add(pInfo.Name);
                    } else if (isGenericField && SerializableTypes.Contains(t.GetGenericTypeDefinition())) {
                        strList.Add(pInfo.Name);
                    } else if(Reflection.TypeHasAnyOfInterface(t, SerializableTypes, out Type intT)) {
                        strList.Add(pInfo.Name);
                    }
                }
                foreach (FieldInfo fInfo in fields) {
                    if (fInfo.GetCustomAttribute(typeof(NoSerializeAttribute)) != null
                        || fInfo.GetCustomAttribute(typeof(NonSerializedAttribute)) != null)
                        continue;

                    Type t = fInfo.FieldType;
                    bool isGenericField = t.IsGenericType;
                    if (SerializableTypes.Contains(t)) {
                        strList.Add(fInfo.Name);
                    } else if (isGenericField && SerializableTypes.Contains(t.GetGenericTypeDefinition())) {
                        strList.Add(fInfo.Name);
                    } else if(Reflection.TypeHasAnyOfInterface(t, SerializableTypes, out Type intT)) {
                        strList.Add(fInfo.Name);
                    }
                }

                ObjectSerializers.Add(type, new SerializerInfo(type, strList));
            }
        }

        internal static SerializerInfo GetObjectSerializer(Type type) 
            => ObjectSerializers.TryGetValue(type, out SerializerInfo result) ? result : null;

        static SerializerReflection()
        {
            TypeTable.Add(0, typeof(byte));
            TypeTable.Add(1, typeof(sbyte));
            TypeTable.Add(2, typeof(float));
            TypeTable.Add(3, typeof(double));
            TypeTable.Add(4, typeof(uint));
            TypeTable.Add(5, typeof(short));
            TypeTable.Add(6, typeof(ushort));
            TypeTable.Add(7, typeof(long));
            TypeTable.Add(8, typeof(ulong));
        }
    }

    public class SerializerInfo
    {
        public Type Type;
        public QuickList<string> SerializedVariables;

        public SerializerInfo(Type type, QuickList<string> serializedVariables) 
        {
            Type = type;
            SerializedVariables = serializedVariables;
        }
    }
}
