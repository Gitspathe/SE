using System;
using System.Collections.Generic;
using System.Text;
using LiteNetLib.Utils;
using SE.Core;
using SE.Pooling;

namespace SE.Engine.Networking
{
    public static class NetworkPool
    {
        private static ObjectPool<NetDataReader> readerPool = new ObjectPool<NetDataReader>();
        private static ObjectPool<NetDataWriter> writerPool = new ObjectPool<NetDataWriter>();

        internal static NetDataReader GetReader()
        {
            lock (Network.NetworkLock) 
                return readerPool.Take();
        }

        internal static NetDataReader GetReader(byte[] data)
        {
            lock (Network.NetworkLock) {
                NetDataReader reader = readerPool.Take();
                reader.SetSource(data);
                return reader;
            }
        }

        internal static void ReturnReader(NetDataReader reader)
        {
            lock (Network.NetworkLock)
                readerPool.Return(reader);
        }

        internal static NetDataWriter GetWriter()
        {
            lock (Network.NetworkLock) {
                NetDataWriter writer = writerPool.Take();
                writer.Reset();
                return writer;
            }
        }

        internal static void ReturnWriter(NetDataWriter writer)
        {
            lock (Network.NetworkLock)
                writerPool.Return(writer);
        }
    }
}
