using SE.Core;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
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

        public static bool SkipQuotedString(Utf8Reader reader)
        {
            bool inQuote = false;
            Stream baseStream = reader.BaseStream;
            while (true) {
                if (baseStream.Position + 1 > baseStream.Length)
                    return false;
                if (reader.ReadByte() != _STRING_IDENTIFIER)
                    continue;

                if (inQuote) {
                    return true;
                }
                inQuote = true;
            }
        }

        public static bool SkipArray(Utf8Reader reader)
        {
            bool inArray = false;
            Stream baseStream = reader.BaseStream;
            while (true) {
                if (baseStream.Position + 1 > baseStream.Length)
                    return false;

                switch (reader.ReadByte()) {
                    case _BEGIN_ARRAY: {
                        if (inArray) {
                            reader.BackOne();
                            if (!SkipArray(reader)) {
                                return false;
                            }
                        } else {
                            inArray = true;
                        }
                    }
                    break;
                    case _END_ARRAY:
                        return true;
                    default:
                        continue;
                }
            }
        }

        public static bool SkipMeta(Utf8Reader reader)
        {
            bool inMeta = false;
            Stream baseStream = reader.BaseStream;
            while (true) {
                if (baseStream.Position + 1 > baseStream.Length)
                    return false;

                switch (reader.ReadByte()) {
                    case _BEGIN_META: {
                        if (inMeta) {
                            reader.BackOne();
                            if (!SkipMeta(reader)) {
                                return false;
                            }
                        }
                        inMeta = true;
                    }
                    break;
                    case _END_META:
                        return true;
                    default:
                        continue;
                }
            }
        }

        public static bool SkipClass(Utf8Reader reader)
        {
            bool inClass = false;
            Stream baseStream = reader.BaseStream;
            while (true) {
                if (baseStream.Position + 1 > baseStream.Length)
                    return false;

                switch (reader.ReadByte()) {
                    case _BEGIN_CLASS: {
                        if (inClass) {
                            reader.BackOne();
                            if (!SkipClass(reader)) {
                                return false;
                            }
                        }
                        inClass = true;
                    }
                    break;
                    case _END_CLASS:
                        return true;
                    default:
                        continue;
                }
            }
        }

        public static bool IsNumber(byte b)
        {
            switch (b) {
                case (byte)'0':
                case (byte)'1':
                case (byte)'2':
                case (byte)'3':
                case (byte)'4':
                case (byte)'5':
                case (byte)'6':
                case (byte)'7':
                case (byte)'8':
                case (byte)'9':
                case (byte)',':
                case (byte)'.':
                case (byte)'+':
                case (byte)'-':
                    return true;
                default:
                    return false;
            }
        }

        public static bool SkipNumber(Utf8Reader reader)
        {
            Stream baseStream = reader.BaseStream;
            while (true) {
                if (baseStream.Position + 1 > baseStream.Length)
                    return false;

                switch (reader.ReadByte()) {
                    case (byte)'0':
                    case (byte)'1':
                    case (byte)'2':
                    case (byte)'3':
                    case (byte)'4':
                    case (byte)'5':
                    case (byte)'6':
                    case (byte)'7':
                    case (byte)'8':
                    case (byte)'9':
                    case (byte)',':
                    case (byte)'.':
                    case (byte)'+':
                    case (byte)'-':
                        continue;
                    default:
                        return true;
                }
            }
        }

        public static bool SkipToNextSymbol(Utf8Reader reader, byte symbol, bool skipPast = true)
        {
            Stream baseStream = reader.BaseStream;
            while (baseStream.Position + 1 < baseStream.Length) {
                byte b = reader.ReadByte();
                if (b != symbol)
                    continue;

                if (!skipPast) {
                    reader.BaseStream.Position -= 1;
                }
                return true;
            }
            return false;
        }


        // TODO: ReadArray<T>, to read array entries separated with ','.

    }
}
