using System;
using System.Collections.Generic;
using DeeZ.Core.Exceptions;
using DeeZ.Engine.Utility;
using LiteNetLib;
using LiteNetLib.Utils;
using Vector2 = System.Numerics.Vector2;

namespace DeeZ.Engine.Networking
{
    public static class NetData
    {
        // TODO: REEEEEEEEEEEEEEEEEEEEEEE BOXING & UNBOXING REEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE!!!!!

        private static byte curByte = 23;

        /// <summary>
        /// Dictionary used to convert types to bytes. The byte returned from a conversion is used as an ID
        /// by the networking system to identify Types.
        /// </summary>
        internal static Dictionary<Type, byte> NetDataConversionTable = new Dictionary<Type, byte> {
            {typeof(long), 1},
            {typeof(ulong), 2},
            {typeof(int), 3},
            {typeof(uint), 4},
            {typeof(short), 5},
            {typeof(ushort), 6},
            {typeof(bool), 7},
            {typeof(byte), 8},
            {typeof(float), 9},
            {typeof(double), 10},
            {typeof(string), 11},
            {typeof(long[]), 12},
            {typeof(ulong[]), 13},
            {typeof(int[]), 14},
            {typeof(uint[]), 15},
            {typeof(short[]), 16},
            {typeof(ushort[]), 17},
            {typeof(bool[]), 18},
            {typeof(byte[]), 19},
            {typeof(float[]), 20},
            {typeof(double[]), 21},
            {typeof(string[]), 22},
            {typeof(Vector2), 23}
        };

        /// <summary>
        /// Dictionary used to read an object from a NetIncomingMessage. The byte key identifies which Type the object is.
        /// </summary>
        //TODO: Could eliminate new() calls with a non-alloc version of this function.
        internal static QuickList<Func<NetPacketReader, object>> DataReaders = new QuickList<Func<NetPacketReader, object>> {
            message => message.GetLong(),
            message => message.GetULong(),
            message => message.GetInt(),
            message => message.GetUInt(),
            message => message.GetShort(),
            message => message.GetUShort(),
            message => message.GetBool(),
            message => message.GetByte(),
            message => message.GetFloat(),
            message => message.GetDouble(),
            message => message.GetString(),
            message => {
                long[] array = new long[message.GetUShort()];
                for (int ii = 0; ii < array.Length; ii++) {
                    array[ii] = message.GetLong();
                }
                return array;
            },
            message => {
                ulong[] array = new ulong[message.GetUShort()];
                for (int ii = 0; ii < array.Length; ii++) {
                    array[ii] = message.GetULong();
                }
                return array;
            },
            message => {
                int[] array = new int[message.GetUShort()];
                for (int ii = 0; ii < array.Length; ii++) {
                    array[ii] = message.GetInt();
                }
                return array;
            },
            message => {
                uint[] array = new uint[message.GetUShort()];
                for (int ii = 0; ii < array.Length; ii++) {
                    array[ii] = message.GetUInt();
                }
                return array;
            },
            message => {
                short[] array = new short[message.GetUShort()];
                for (int ii = 0; ii < array.Length; ii++) {
                    array[ii] = message.GetShort();
                }
                return array;
            },
            message => {
                ushort[] array = new ushort[message.GetUShort()];
                for (int ii = 0; ii < array.Length; ii++) {
                    array[ii] = message.GetUShort();
                }
                return array;
            },
            message => {
                bool[] array = new bool[message.GetUShort()];
                for (int ii = 0; ii < array.Length; ii++) {
                    array[ii] = message.GetBool();
                }
                return array;
            },
            message => {
                byte[] array = new byte[message.GetUShort()];
                for (int ii = 0; ii < array.Length; ii++) {
                    array[ii] = message.GetByte();
                }
                return array;
            },
            message => {
                float[] array = new float[message.GetUShort()];
                for (int ii = 0; ii < array.Length; ii++) {
                    array[ii] = message.GetFloat();
                }
                return array;
            },
            message => {
                double[] array = new double[message.GetUShort()];
                for (int ii = 0; ii < array.Length; ii++) {
                    array[ii] = message.GetDouble();
                }
                return array;
            },
            message => {
                string[] array = new string[message.GetUShort()];
                for (int ii = 0; ii < array.Length; ii++) {
                    array[ii] = message.GetString();
                }
                return array;
            },
            message => new Vector2(message.GetFloat(), message.GetFloat())
        };

        /// <summary>
        /// Dictionary used to write an object to a NetIncomingMessage. The byte key identifies which Type the object is.
        /// </summary>
        internal static QuickList<Action<object, NetDataWriter>> DataWriters = new QuickList<Action<object, NetDataWriter>> {
            (obj, message) => message.Put((long)obj),
            (obj, message) => message.Put((ulong)obj),
            (obj, message) => message.Put((int)obj),
            (obj, message) => message.Put((uint)obj),
            (obj, message) => message.Put((short)obj),
            (obj, message) => message.Put((ushort)obj),
            (obj, message) => message.Put((bool)obj),
            (obj, message) => message.Put((byte)obj),
            (obj, message) => message.Put((float)obj),
            (obj, message) => message.Put((double)obj),
            (obj, message) => message.Put(obj as string),
            (obj, message) => {
                long[] array = obj as long[];
                message.Put((ushort)array.Length);
                for (int ii = 0; ii < array.Length; ii++) {
                    message.Put(array[ii]);
                }
            },
            (obj, message) => {
                ulong[] array = obj as ulong[];
                message.Put((ushort)array.Length);
                for (int ii = 0; ii < array.Length; ii++) {
                    message.Put(array[ii]);
                }
            },
            (obj, message) => {
                int[] array = obj as int[];
                message.Put((ushort)array.Length);
                for (int ii = 0; ii < array.Length; ii++) {
                    message.Put(array[ii]);
                }
            },
            (obj, message) => {
                uint[] array = obj as uint[];
                message.Put((ushort)array.Length);
                for (int ii = 0; ii < array.Length; ii++) {
                    message.Put(array[ii]);
                }
            },
            (obj, message) => {
                short[] array = obj as short[];
                message.Put((ushort)array.Length);
                for (int ii = 0; ii < array.Length; ii++) {
                    message.Put(array[ii]);
                }
            },
            (obj, message) => {
                ushort[] array = obj as ushort[];
                message.Put((ushort)array.Length);
                for (int ii = 0; ii < array.Length; ii++) {
                    message.Put(array[ii]);
                }
            },
            (obj, message) => {
                bool[] array = obj as bool[];
                message.Put((ushort)array.Length);
                for (int ii = 0; ii < array.Length; ii++) {
                    message.Put(array[ii]);
                }
            },
            (obj, message) => {
                byte[] array = obj as byte[];
                message.Put((ushort)array.Length);
                for (int ii = 0; ii < array.Length; ii++) {
                    message.Put(array[ii]);
                }
            },
            (obj, message) => {
                float[] array = obj as float[];
                message.Put((ushort)array.Length);
                for (int ii = 0; ii < array.Length; ii++) {
                    message.Put(array[ii]);
                }
            },
            (obj, message) => {
                double[] array = obj as double[];
                message.Put((ushort)array.Length);
                for (int ii = 0; ii < array.Length; ii++) {
                    message.Put(array[ii]);
                }
            },
            (obj, message) => {
                string[] array = obj as string[];
                message.Put((ushort)array.Length);
                for (int ii = 0; ii < array.Length; ii++) {
                    message.Put(array[ii]);
                }
            },
            (obj, message) => {
                Vector2 vector = (Vector2) obj;
                message.Put(vector.X);
                message.Put(vector.Y);
            }
        };

        public static void Convert(object[] parameters, byte[] existing)
        {
            try {
                for (int i = 0; i < parameters.Length; i++) {
                    NetDataConversionTable.TryGetValue(parameters[i].GetType(), out existing[i]);
                }
            } catch (Exception e) {
                throw new RPCConversionException("Failed to convert object[] to RPCDataType[].", e);
            }
        }

        public static void AddDataType(Type type, Func<NetPacketReader, object> readFunction, Action<object, NetDataWriter> writeFunction)
        {
            if (curByte + 1 > byte.MaxValue)
                throw new Exception("Cannot add new data type. Not enough space.");

            curByte++;
            NetDataConversionTable.Add(type, curByte);
            DataReaders.Add(readFunction);
            DataWriters.Add(writeFunction);
        }

        public static object Read(byte byteVal, NetPacketReader reader)
        {
            byteVal--; // Decrement to get correct array index.
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            if (byteVal > DataWriters.Count)
                throw new ArgumentOutOfRangeException(nameof(byteVal), "No data reader found.");

            return DataReaders.Array[byteVal].Invoke(reader);
        }

        public static void Write(byte byteVal, object obj, NetDataWriter writer)
        {
            byteVal--; // Decrement to get correct array index.
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if(writer == null)
                throw new ArgumentNullException(nameof(writer));
            if(byteVal > DataWriters.Count)
                throw new ArgumentOutOfRangeException(nameof(byteVal), "No data writer found.");

            DataWriters.Array[byteVal].Invoke(obj, writer);
        }
    }
}
