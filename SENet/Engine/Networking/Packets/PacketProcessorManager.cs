using System;
using System.Collections.Generic;

namespace SE.Engine.Networking.Packets
{
    internal static class PacketProcessorManager
    {
        private static Dictionary<Type, IPacketProcessor> packetProcessorsType = new Dictionary<Type, IPacketProcessor>();
        private static Dictionary<byte, IPacketProcessor> packetProcessorsByte = new Dictionary<byte, IPacketProcessor>();
        private static Dictionary<Type, byte> byteConversionTable = new Dictionary<Type, byte>();

        private static byte curIndex = 0;

        public static void RegisterProcessor(IPacketProcessor processor)
        {
            packetProcessorsType.Add(processor.GetType(), processor);
            packetProcessorsByte.Add(curIndex, processor);
            byteConversionTable.Add(processor.GetType(), curIndex);
            curIndex++;
        }

        public static IPacketProcessor GetProcessor(Type type) 
            => packetProcessorsType.TryGetValue(type, out IPacketProcessor processor) ? processor : null;

        public static IPacketProcessor GetProcessor(byte val) 
            => packetProcessorsByte.TryGetValue(val, out IPacketProcessor processor) ? processor : null;

        public static byte? GetByte(Type type)
        {
            if (byteConversionTable.TryGetValue(type, out byte b)) {
                return b;
            }
            return null;
        }
    }
}
