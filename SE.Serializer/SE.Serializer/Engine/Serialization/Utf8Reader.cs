using SE.Core;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using static SE.Serialization.Constants;

namespace SE.Serialization
{
    public sealed class Utf8Reader : BinaryReader
    {
        private Memory<byte> memory;
        private byte[] buffer;
        private int position;
        private int bufferLength;
        private int bufferLengthMinusOne;
        private Stream stream;

        public Utf8Reader(Stream input) : this(input, new UTF8Encoding(), false) { }
        public Utf8Reader(Stream input, Encoding encoding) : this(input, encoding, false) { }

        public Utf8Reader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
            buffer = ArrayPool<byte>.Shared.Rent(256);
            memory = new Memory<byte>(buffer);
            position = 0;
            bufferLength = memory.Length;
            bufferLengthMinusOne = bufferLength - 1;
            stream = input;
        }

        /// <summary>
        /// Move buffer back one byte.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BackOne()
        {
            BaseStream.Position -= 1;
        }

        private ReadOnlySpan<byte> ReadNumber()
        {
            position = 0;
            while (true) {
                byte b = ReadByte();
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
                    case (byte)'-':
                    case (byte)'+':
                    case (byte)'.':
                        EnsureCapacity();
                        buffer[position++] = b;
                        break;

                    default:
                        goto end;
                }
            }

        end:
            return memory.Span.Slice(0, position);
        }

        private ReadOnlySpan<byte> ReadQuotedUtf8StringInternal()
        {
            position = 0;
            if (ReadByte() != _STRING_IDENTIFIER)
                throw new Exception("Not a quoted string!");

            while (true) {
                byte b = ReadByte();

                // Handle escaped character if '\' is encountered.
                // And break if '"' is encountered.
                if (b == _ESCAPE) {
                    byte escapeChar = ReadByte();
                    if (SerializerUtil.IsEscapableChar(escapeChar)) {
                        EnsureCapacity();
                        buffer[position++] = ConvertEscapeToControl(escapeChar);
                        continue;
                    }
                } else if (b == _STRING_IDENTIFIER) {
                    break;
                }

                EnsureCapacity();
                buffer[position++] = b;
            }
            return memory.Span.Slice(0, position);
        }

        private byte ConvertEscapeToControl(byte escapeChar)
        {
            switch (escapeChar) {
                case (byte)'n':
                    return _NEW_LINE;
                case (byte)'r':
                    return _CARRIDGE_RETURN;
                case (byte)'"':
                    return _STRING_IDENTIFIER;
            }
            return 0;
        }

        private ReadOnlySpan<byte> ReadUntil(byte identifier)
        {
            position = 0;
            while (true) {
                byte b = ReadByte();
                if (b == identifier)
                    break;

                EnsureCapacity();
                buffer[position++] = b;
            }
            return memory.Span.Slice(0, position);
        }

        public string ReadTo(byte symbol, bool skipPast = true)
        {
            while (true) {
                try {
                    byte c = ReadByte();
                    if (c != symbol) {
                        MemoryCache.Append(c);
                        continue;
                    }
                    if (!skipPast) {
                        BaseStream.Position -= 1;
                    }
                    break;
                } catch (Exception) {
                    break;
                }
            }
            return MemoryCache.RetrieveAndReset();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity()
        {
            if (position < bufferLengthMinusOne)
                return;

            Grow(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int capacity)
        {
            if (position + capacity < bufferLength)
                return;

            Grow(capacity);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Grow(int capacity)
        {
            byte[] newMemoryBuffer = ArrayPool<byte>.Shared.Rent((bufferLength + capacity) * 2);
            Buffer.BlockCopy(buffer, 0, newMemoryBuffer, 0, bufferLength);
            ArrayPool<byte>.Shared.Return(buffer);
            buffer = newMemoryBuffer;
            memory = new Memory<byte>(buffer);
            bufferLength = memory.Length;
            bufferLengthMinusOne = bufferLength - 1;
        }

        public byte[] ReadByteArray()
        {
            int length = ReadInt32();
            byte[] tmpBuffer = new byte[length];
            int index = 0;
            do {
                int num = Read(tmpBuffer, index, length - index);
                if (num == 0)
                    throw new EndOfStreamException();
                index += num;
            } while (index != length);
            return tmpBuffer;
        }

        public override string ReadString()
        {
            byte[] numArray = new byte[ReadInt32()];
            Read(numArray, 0, numArray.Length);
            return Serializer.UTF8.GetString(numArray);
        }

        public bool ReadBooleanString()
        {
            char[] bytes = ArrayPool<char>.Shared.Rent(4);
            ReadOnlySpan<char> span = new ReadOnlySpan<char>(bytes, 0, 4);

            int index = 0;
            while (true) {
                byte b = ReadByte();
                if (SerializerUtil.IsControlCharOrWhitespace(b)) {
                    stream.Position -= 1;
                    break;
                }

                bytes[index++] = (char)b;

                if (index > 3)
                    break;
            }

            bool val;
            bool.TryParse(span, out val);
            ArrayPool<char>.Shared.Return(bytes);
            return val;
        }

        public string ReadQuotedString()
        {
            ReadOnlySpan<byte> stringBytes = ReadQuotedUtf8StringInternal();
            return Serializer.UTF8.GetString(stringBytes);
        }

        public void SkipWhiteSpace()
        {
            while (true) {
                byte b = ReadByte();
                switch (b) {
                    case _TAB:
                    case _NEW_LINE:
                        continue;
                    default:
                        stream.Position -= 1;
                        return;
                }
            }
        }

        public void SkipDelimitersAndWhitespace()
        {
            while (true) {
                byte b = ReadByte();
                switch (b) {
                    case _ARRAY_SEPARATOR:
                    case _BEGIN_ARRAY:
                    case _BEGIN_META:
                    case _BEGIN_VALUE:
                    case _END_ARRAY:
                    case _END_CLASS:
                    case _END_META:
                    case _TAB:
                    case _STRING_IDENTIFIER:
                        break;
                    default:
                        stream.Position -= 1;
                        return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByteUtf8()
        {
            ReadOnlySpan<byte> numberBytes = ReadNumber();
            Utf8Parser.TryParse(numberBytes, out byte val, out int bytesConsumed);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadSByteUtf8()
        {
            ReadOnlySpan<byte> numberBytes = ReadNumber();
            Utf8Parser.TryParse(numberBytes, out sbyte val, out int bytesConsumed);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadShortUtf8()
        {
            ReadOnlySpan<byte> numberBytes = ReadNumber();
            Utf8Parser.TryParse(numberBytes, out short val, out int bytesConsumed);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUShortUtf8()
        {
            ReadOnlySpan<byte> numberBytes = ReadNumber();
            Utf8Parser.TryParse(numberBytes, out ushort val, out int bytesConsumed);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadIntUtf8()
        {
            ReadOnlySpan<byte> numberBytes = ReadNumber();
            Utf8Parser.TryParse(numberBytes, out int val, out int bytesConsumed);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUIntUtf8()
        {
            ReadOnlySpan<byte> numberBytes = ReadNumber();
            Utf8Parser.TryParse(numberBytes, out uint val, out int bytesConsumed);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadLongUtf8()
        {
            ReadOnlySpan<byte> numberBytes = ReadNumber();
            Utf8Parser.TryParse(numberBytes, out long val, out int bytesConsumed);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadULongUtf8()
        {
            ReadOnlySpan<byte> numberBytes = ReadNumber();
            Utf8Parser.TryParse(numberBytes, out ulong val, out int bytesConsumed);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloatUtf8()
        {
            ReadOnlySpan<byte> numberBytes = ReadNumber();
            Utf8Parser.TryParse(numberBytes, out float val, out int bytesConsumed);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadDoubleUtf8()
        {
            ReadOnlySpan<byte> numberBytes = ReadNumber();
            Utf8Parser.TryParse(numberBytes, out double val, out int bytesConsumed);
            return val;
        }

        private static class MemoryCache
        {
            private static byte[] buffer;
            private static int bufferLength;
            private static int bufferLengthMinusOne;
            private static int position;

            static MemoryCache()
            {
                SetLengthInternal(256);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void SetLengthInternal(int newSize)
            {
                if (buffer == null) {
                    buffer = new byte[newSize];
                    bufferLength = buffer.Length;
                    bufferLengthMinusOne = bufferLength - 1;
                    return;
                }

                byte[] newBuffer = new byte[newSize];
                Array.Copy(buffer, newBuffer, buffer.Length);
                buffer = newBuffer;
                bufferLength = buffer.Length;
                bufferLengthMinusOne = bufferLength - 1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void EnsureCapacity()
            {
                if (position < bufferLengthMinusOne)
                    return;

                SetLengthInternal((bufferLength + 1) * 2);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Append(byte b)
            {
                EnsureCapacity();
                buffer[position++] = b;
            }

            public static string RetrieveAndReset()
            {
                string str = Serializer.UTF8.GetString(buffer, 0, position);
                position = 0;
                return str;
            }
        }
    }
}
