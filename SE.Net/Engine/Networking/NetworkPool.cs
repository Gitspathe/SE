using System;
using System.Collections.Generic;
using System.Text;
using LiteNetLib.Utils;
using SE.Pooling;

namespace SE.Engine.Networking
{
    public static class NetworkPool
    {
        private static ObjectPool<NetDataReader> readerPool = new ObjectPool<NetDataReader>();
        private static ObjectPool<NetDataWriter> writerPool = new ObjectPool<NetDataWriter>();

        internal static NetDataReader GetReader()
        {
            lock (readerPool)
                return readerPool.Take();
        }

        internal static NetDataReader GetReader(byte[] data)
        {
            lock (readerPool) {
                NetDataReader reader = readerPool.Take();
                reader.SetSource(data);
                return reader;
            }
        }

        internal static void ReturnReader(NetDataReader reader)
        {
            lock (readerPool)
                readerPool.Return(reader);
        }

        internal static NetDataWriter GetWriter()
        {
            lock (writerPool) {
                NetDataWriter writer = writerPool.Take();
                writer.Reset();
                return writer;
            }
        }

        internal static void ReturnWriter(NetDataWriter writer)
        {
            lock (writerPool)
                writerPool.Return(writer);
        }
    }
}
