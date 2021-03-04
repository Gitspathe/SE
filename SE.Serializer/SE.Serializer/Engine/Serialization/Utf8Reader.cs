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
            if (ReadByte() != Serializer._STRING_IDENTIFIER)
                throw new Exception("Not a quoted string!");

            return ReadUntil(Serializer._STRING_IDENTIFIER);
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
            return Encoding.UTF8.GetString(numArray);
        }

        public string ReadQuotedStringUtf8()
        {
            ReadOnlySpan<byte> stringBytes = ReadQuotedUtf8StringInternal();
            return Encoding.UTF8.GetString(stringBytes);
        }

        public void SkipWhiteSpace()
        {
            while (true) {
                if (ReadByte() == Serializer._TAB) 
                    continue;

                stream.Position -= 1;
                break;
            }
        }

        public void SkipDelimitersAndWhitespace()
        {
            bool shouldBreak = false;
            while (!shouldBreak) {
                byte b = ReadByte();
                switch (b) {
                    case Serializer._ARRAY_SEPARATOR:
                    case Serializer._BEGIN_ARRAY:
                    case Serializer._BEGIN_META:
                    case Serializer._BEGIN_VALUE:
                    case Serializer._END_ARRAY:
                    case Serializer._END_CLASS:
                    case Serializer._END_META:
                    case Serializer._TAB:
                    case Serializer._STRING_IDENTIFIER:
                        break;

                    default:
                        shouldBreak = true;
                        stream.Position -= 1;
                        break;
                }
            }
        }

        public void SkipTo(byte utf8Char)
        {
            while (true) {
                if (ReadByte() != utf8Char)
                    continue;

                stream.Position -= 1;
                break;
            }
        }

        public void SkipToAfter(byte utf8Char)
        {
            while (true) {
                if (ReadByte() != utf8Char)
                    continue;
                break;
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
    }
}
