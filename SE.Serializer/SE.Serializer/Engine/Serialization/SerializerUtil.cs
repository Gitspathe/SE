using System;
using System.Buffers;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using FastStream;
using SE.Core;
using static SE.Serialization.Constants;

namespace SE.Serialization
{
    public static class SerializerUtil
    {
        private static StringBuilder strBuilder = new StringBuilder(255);
        private static Dictionary<Type, string> minimalAssemblyQualifiedTypeNames = new Dictionary<Type, string>();

        public static string GetQualifiedTypeName(Type type, SerializerSettings settings)
        {
            if (settings.TypeNaming == TypeNaming.Full) {
                return type.AssemblyQualifiedName;
            }
            if (minimalAssemblyQualifiedTypeNames.TryGetValue(type, out string str)) {
                return str;
            }

            strBuilder.Clear();
            AssemblyName assemblyName = type.Assembly.GetName();
            string publicKeyTokenStr = GetPublicKeyTokenHexString(assemblyName.GetPublicKeyToken());
            if (publicKeyTokenStr == null) {
                strBuilder
                   .Append(type.FullName)
                   .Append(", ")
                   .Append(assemblyName.Name);
            } else {
                strBuilder
                   .Append(type.FullName)
                   .Append(", ")
                   .Append(assemblyName.Name)
                   .Append(", ")
                   .Append("PublicKeyToken=")
                   .Append(publicKeyTokenStr);
            }

            str = strBuilder.ToString();
            minimalAssemblyQualifiedTypeNames.Add(type, str);
            return str;
        }

        public static void WriteTextUtf8(this Utf8Writer writer, string str)
        {
            byte[] arr = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetByteCount(str));
            int bufferSize = GetUtf8Bytes(arr, str);
            writer.Write(arr, bufferSize);
            ArrayPool<byte>.Shared.Return(arr);
        }

        public static void WriteQuotedTextUtf8(this Utf8Writer writer, string str)
        {
            byte[] arr = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetByteCount(str));
            int bufferLength = GetUtf8Bytes(arr, str);

            writer.Write(_STRING_IDENTIFIER);
            writer.Write(arr, bufferLength);
            writer.Write(_STRING_IDENTIFIER);
            ArrayPool<byte>.Shared.Return(arr);
        }

        internal static int GetUtf8Bytes(byte[] arr, string str) 
            => Serializer.UTF8.GetBytes(str, 0, str.Length, arr, 0);

        private static string GetPublicKeyTokenHexString(byte[] token)
            => (token == null || token.Length < 1)
                ? null : BitConverter.ToString(token).Replace("-", string.Empty).ToLower();

        // TODO: ReadArray<T>, to read array entries separated with ','.

    }
}
