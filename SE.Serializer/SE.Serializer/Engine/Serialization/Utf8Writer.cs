using FastStream;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using static SE.Serialization.Constants;

namespace SE.Serialization
{
    public sealed class Utf8Writer : Stream
    {
        private Memory<byte> memory;
        private byte[] buffer;
        private int position;
        private int currentMemoryLength;
        private bool disposedValue;

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        public override long Length => buffer.Length;

        public override long Position {
            get => position;
            set => position = (int)value;
        }

        public Utf8Writer(int capacity = 512)
        {
            buffer = ArrayPool<byte>.Shared.Rent(capacity);
            memory = new Memory<byte>(buffer);
            currentMemoryLength = capacity;
            position = 0;
        }

        public byte[] ToArray()
        {
            byte[] numArray = new byte[position];
            Buffer.BlockCopy(buffer, 0, numArray, 0, position);
            return numArray;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SetLengthInternal(int newSize)
        {
            int nextSize = newSize;
            byte[] newBuffer = ArrayPool<byte>.Shared.Rent(nextSize);
            Buffer.BlockCopy(buffer, 0, newBuffer, 0, currentMemoryLength);
            ArrayPool<byte>.Shared.Return(buffer);
            buffer = newBuffer;
            memory = new Memory<byte>(buffer);
            currentMemoryLength = buffer.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void IncrementSize(int count)
        {
            SetLengthInternal((currentMemoryLength + count) * 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int count = 1)
        {
            if(position + count < currentMemoryLength)
                return;

            IncrementSize(count);
        }

        public void WriteUtf8(bool value)
        {
            EnsureCapacity(WriterConstants._MAX_BOOL_UTF8_SIZE);
            Write(value ? TrueValue : FalseValue);
        }

        public void WriteUtf8(byte value)
        {
            EnsureCapacity(WriterConstants._MAX_BYTE_UTF8_SIZE);
            Utf8Formatter.TryFormat(value, memory.Span.Slice(position), out int bytesWritten);
            position += bytesWritten;
        }

        public void WriteUtf8(sbyte value)
        {
            EnsureCapacity(WriterConstants._MAX_BYTE_UTF8_SIZE);
            Utf8Formatter.TryFormat(value, memory.Span.Slice(position), out int bytesWritten);
            position += bytesWritten;
        }

        public void WriteUtf8(short value)
        {
            EnsureCapacity(WriterConstants._MAX_SHORT_UTF8_SIZE);
            Utf8Formatter.TryFormat(value, memory.Span.Slice(position), out int bytesWritten);
            position += bytesWritten;
        }

        public void WriteUtf8(ushort value)
        {
            EnsureCapacity(WriterConstants._MAX_SHORT_UTF8_SIZE);
            Utf8Formatter.TryFormat(value, memory.Span.Slice(position), out int bytesWritten);
            position += bytesWritten;
        }

        public void WriteUtf8(int value)
        {
            EnsureCapacity(WriterConstants._MAX_INT_UTF8_SIZE);
            Utf8Formatter.TryFormat(value, memory.Span.Slice(position), out int bytesWritten);
            position += bytesWritten;
        }

        public void WriteUtf8(uint value)
        {
            EnsureCapacity(WriterConstants._MAX_INT_UTF8_SIZE);
            Utf8Formatter.TryFormat(value, memory.Span.Slice(position), out int bytesWritten);
            position += bytesWritten;
        }

        public void WriteUtf8(long value)
        {
            EnsureCapacity(WriterConstants._MAX_LONG_UTF8_SIZE);
            Utf8Formatter.TryFormat(value, memory.Span.Slice(position), out int bytesWritten);
            position += bytesWritten;
        }

        public void WriteUtf8(ulong value)
        {
            EnsureCapacity(WriterConstants._MAX_LONG_UTF8_SIZE);
            Utf8Formatter.TryFormat(value, memory.Span.Slice(position), out int bytesWritten);
            position += bytesWritten;
        }

        public void WriteUtf8(float value)
        {
            EnsureCapacity(WriterConstants._MAX_FLOAT_UTF8_SIZE);
            Utf8Formatter.TryFormat(value, memory.Span.Slice(position), out int bytesWritten);
            position += bytesWritten;
        }

        public void WriteUtf8(double value)
        {
            EnsureCapacity(WriterConstants._MAX_DOUBLE_UTF8_SIZE);
            Utf8Formatter.TryFormat(value, memory.Span.Slice(position), out int bytesWritten);
            position += bytesWritten;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            Write(bytes.Length);
            Write(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(bool value)
        {
            Write(value ? (byte)1 : (byte)0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(short value)
        {
            EnsureCapacity(2);
            buffer[position] = (byte)value;
            buffer[position + 1] = (byte)((uint)value >> 8);
            position += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ushort value)
        {
            EnsureCapacity(2);
            buffer[position] = (byte)value;
            buffer[position + 1] = (byte)((uint)value >> 8);
            position += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(char value)
        {
            EnsureCapacity(2);
            buffer[position] = (byte)value;
            buffer[position + 1] = (byte)((uint)value >> 8);
            position += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int value)
        {
            EnsureCapacity(4);
            buffer[position] = (byte)value;
            buffer[position + 1] = (byte)(value >> 8);
            buffer[position + 2] = (byte)(value >> 16);
            buffer[position + 3] = (byte)(value >> 24);
            position += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(uint value)
        {
            EnsureCapacity(4);
            buffer[position] = (byte)value;
            buffer[position + 1] = (byte)(value >> 8);
            buffer[position + 2] = (byte)(value >> 16);
            buffer[position + 3] = (byte)(value >> 24);
            position += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(long value)
        {
            Write8Bytes((byte*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ulong value)
        {
            Write8Bytes((byte*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(float value)
        {
            Write4BytesFromPointer((byte*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(double value)
        {
            Write8Bytes((byte*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] value)
        {
            Write(value, 0, value.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByteArray(byte[] value)
        {
            Write(value.Length);
            Write(value, 0, value.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte value)
        {
            EnsureCapacity(1);
            buffer[position] = value;
            ++position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteIndent(int depth)
        {
            int toWrite = (depth-1) * 2;
            EnsureCapacity(toWrite);
            for (int i = 0; i < toWrite; i++) {
                buffer[position++] = _TAB;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void Write4BytesFromPointer(byte* p)
        {
            EnsureCapacity(4);
            buffer[position] = *p;
            buffer[position + 1] = p[1];
            buffer[position + 2] = p[2];
            buffer[position + 3] = p[3];
            position += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void Write8Bytes(byte* p)
        {
            EnsureCapacity(8);
            buffer[position] = *p;
            buffer[position + 1] = p[1];
            buffer[position + 2] = p[2];
            buffer[position + 3] = p[3];
            buffer[position + 4] = p[4];
            buffer[position + 5] = p[5];
            buffer[position + 6] = p[6];
            buffer[position + 7] = p[7];
            position += 8;
        }

        public override void Flush() { }

        public override int Read(byte[] inputBuffer, int offset, int count)
        {
            int count1 = (int)Math.Min(Length - Position, count);
            Buffer.BlockCopy(buffer, (int)Position, inputBuffer, offset, count1);
            Position += count1;
            return count1;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length + offset;
                    break;
            }
            return Position;
        }

        public override void SetLength(long value)
        {
            SetLengthInternal((int)value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureCapacity(count);
            Buffer.BlockCopy(buffer, offset, this.buffer, position, count);
            position += count;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposedValue)
                return;

            if (disposing) {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            buffer = null;
            disposedValue = true;
        }

        internal static class WriterConstants
        {
            public const int _MAX_BOOL_UTF8_SIZE = 5;
            public const int _MAX_BYTE_UTF8_SIZE = 4;
            public const int _MAX_SHORT_UTF8_SIZE = 6;
            public const int _MAX_INT_UTF8_SIZE = 11;
            public const int _MAX_LONG_UTF8_SIZE = 21;
            public const int _MAX_FLOAT_UTF8_SIZE = 15;
            public const int _MAX_DOUBLE_UTF8_SIZE = 150;
        }
    }
}
