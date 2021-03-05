using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using FastStream;
using SE.Core;
using static SE.Serialization.Constants;

namespace SE.Serialization
{
    public static class SerializerUtil
    {
        internal static StringBuilder StrBuilder = new StringBuilder(255);

        internal static byte[] GetUtf8Bytes(byte[] arr, string str, out int arrLength)
        {
            arrLength = Serializer.UTF8.GetBytes(str, 0, str.Length, arr, 0);
            return arr;
        }

        public static void WriteText(this Utf8Writer writer, string str)
        {
            byte[] arr = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetByteCount(str));
            writer.Write(GetUtf8Bytes(arr, str, out int arrLength), arrLength);
            ArrayPool<byte>.Shared.Return(arr);
        }

        public static void WriteQuotedText(this Utf8Writer writer, string str)
        {
            byte[] arr = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetByteCount(str));
            writer.Write(_STRING_IDENTIFIER);
            writer.Write(GetUtf8Bytes(arr, str, out int arrLength), arrLength);
            writer.Write(_STRING_IDENTIFIER);
            ArrayPool<byte>.Shared.Return(arr);

        }

        // TODO: ReadArray<T>, to read array entries separated with ','.

    }
}
