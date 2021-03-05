using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Buffers.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using FastStream;
using SE.Core;
using static SE.Serialization.Constants;

namespace SE.Serialization
{
    public sealed class Utf8Reader : BinaryReader
    {
        private Memory<byte> memory;
        private byte[] memoryBuffer;
        private int memoryPosition;
        private int currentMemoryLength;
        private Stream stream;

        public Utf8Reader(Stream input) : this(input, new UTF8Encoding(), false) { }
        public Utf8Reader(Stream input, Encoding encoding) : this(input, encoding, false) { }

        public Utf8Reader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
            memoryBuffer = ArrayPool<byte>.Shared.Rent(256);
            memory = new Memory<byte>(memoryBuffer);
            memoryPosition = 0;
            currentMemoryLength = memory.Length;
            stream = input;
        }

        private ReadOnlySpan<byte> ReadNumber()
        {
            memoryPosition = 0;
            bool shouldBreak = false;
            while (!shouldBreak) {
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
                        memoryBuffer[memoryPosition++] = b;
                        break;

                    default:
                        shouldBreak = true;
                        break;
                }
            }
            return memory.Span.Slice(0, memoryPosition);
        }

        private ReadOnlySpan<byte> ReadQuotedUtf8StringInternal()
        {
            memoryPosition = 0;
            if (ReadByte() != _STRING_IDENTIFIER)
                throw new Exception("Not a quoted string!");

            return ReadUntil(_STRING_IDENTIFIER);
        }

        private ReadOnlySpan<byte> ReadUntil(byte identifier)
        {
            memoryPosition = 0;
            while (true) {
                byte b = ReadByte();
                if (b == identifier)
                    break;

                EnsureCapacity();
                memoryBuffer[memoryPosition++] = b;
            }
            return memory.Span.Slice(0, memoryPosition);
        }

        public string ReadUntil(byte symbol, bool skipPast = true)
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
        private void EnsureCapacity(int capacity = 1)
        {
            if(memoryPosition + capacity < memory.Length)
                return;

            Grow(capacity);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Grow(int capacity)
        {
            byte[] newMemoryBuffer = ArrayPool<byte>.Shared.Rent((memory.Length + capacity) * 2);
            Buffer.BlockCopy(memoryBuffer, 0, newMemoryBuffer, 0, currentMemoryLength);
            ArrayPool<byte>.Shared.Return(memoryBuffer);
            memoryBuffer = newMemoryBuffer;
            memory = new Memory<byte>(memoryBuffer);
            currentMemoryLength = memory.Length;
        }

        public byte[] ReadByteArray()
        {
            int length = ReadInt32();
            byte[] buffer = new byte[length];
            int index = 0;
            do {
                int num = Read(buffer, index, length - index);
                if (num == 0)
                    throw new EndOfStreamException();
                index += num;
            } while (index != length);
            return buffer;
        }

        public override string ReadString()
        {
            byte[] numArray = new byte[ReadInt32()];
            Read(numArray, 0, numArray.Length);
            return Serializer.UTF8.GetString(numArray);
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
            // TODO: Replace string buffer with byte buffer. Then use Encoding.GetString().

            private static string buffer;
            private static int currentBufferLength;
            private static int position;

            static MemoryCache()
            {
                SetLengthInternal(256);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static unsafe void SetLengthInternal(int newSize)
            {
                if (buffer == null) {
                    buffer = new string(' ', newSize);
                    currentBufferLength = buffer.Length;
                    return;
                }

                string newBuffer = new string(' ', newSize);
                fixed (char* target = newBuffer) {
                    Unsafe.Copy(target, ref buffer);
                }
                currentBufferLength = buffer.Length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void EnsureCapacity(int toRead = 1)
            {
                if (position + toRead < currentBufferLength)
                    return;

                SetLengthInternal((currentBufferLength + toRead) * 2);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe void Append(byte b)
            {
                EnsureCapacity();
                fixed (char* target = buffer) {
                    target[position++] = (char)b;
                }
            }

            public static string RetrieveAndReset()
            {
                string str = buffer.Substring(0, position);
                position = 0;
                return str;
            }
        }
    }
}
