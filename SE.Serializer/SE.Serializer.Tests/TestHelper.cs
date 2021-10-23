using System;
using System.Collections.Generic;
using System.Text;
using SE.Serialization;

namespace SE.Serializer.Tests
{
    public static class TestHelper
    {
        public static string GetFailMessage(object requiredValue, object value, string additionalInfo = null)
        {
            return GetFailMessage(requiredValue.ToString(), value.ToString(), additionalInfo);
        }

        public static string GetFailMessage(string requiredValue, object value, string additionalInfo = null)
        {
            return GetFailMessage(requiredValue, value.ToString(), additionalInfo);
        }

        public static string GetFailMessage(string requiredValue, string value, string additionalInfo = null)
        {
            return additionalInfo == null 
                ? $"Required '{requiredValue}', but got '{value}'."
                : $"Required '{requiredValue}', but got '{value}'. [{additionalInfo}]";
        }
    }

    public static class Configs
    {
        public static SerializerSettings BinaryOrder = new SerializerSettings() {
            Formatting = Formatting.Binary,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Serialize,
            ConvertBehaviour = ConvertBehaviour.Order,
            TypeHandling = TypeHandling.Auto,
            TypeNaming = TypeNaming.Minimal,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ReferenceHandling = ReferenceHandling.Preserve,
            MaxDepth = 10
        };

        public static SerializerSettings BinaryNameAndOrder = new SerializerSettings() {
            Formatting = Formatting.Binary,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Serialize,
            ConvertBehaviour = ConvertBehaviour.NameAndOrder,
            TypeHandling = TypeHandling.Auto,
            TypeNaming = TypeNaming.Minimal,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ReferenceHandling = ReferenceHandling.Preserve,
            MaxDepth = 10
        };

        public static SerializerSettings TextOrder = new SerializerSettings() {
            Formatting = Formatting.Text,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Serialize,
            ConvertBehaviour = ConvertBehaviour.Order,
            TypeHandling = TypeHandling.Auto,
            TypeNaming = TypeNaming.Minimal,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ReferenceHandling = ReferenceHandling.Preserve,
            MaxDepth = 10
        };

        public static SerializerSettings TextName = new SerializerSettings() {
            Formatting = Formatting.Text,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Serialize,
            ConvertBehaviour = ConvertBehaviour.Configuration,
            TypeHandling = TypeHandling.Auto,
            TypeNaming = TypeNaming.Minimal,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ReferenceHandling = ReferenceHandling.Preserve,
            MaxDepth = 10
        };

        public static SerializerSettings TextNameAndOrder = new SerializerSettings() {
            Formatting = Formatting.Text,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Serialize,
            ConvertBehaviour = ConvertBehaviour.NameAndOrder,
            TypeHandling = TypeHandling.Auto,
            TypeNaming = TypeNaming.Minimal,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ReferenceHandling = ReferenceHandling.Preserve,
            MaxDepth = 10
        };
    }
}
