using System;
using System.Collections.Generic;

namespace SE.Engine.Networking.Packets
{
    internal static class PacketProcessorManager
    {
        private static Dictionary<Type, PacketProcessor> packetProcessorsType = new Dictionary<Type, PacketProcessor>();
        private static Dictionary<ushort, PacketProcessor> packetProcessorsVal = new Dictionary<ushort, PacketProcessor>();
        private static Dictionary<Type, ushort> valConversionTable = new Dictionary<Type, ushort>();

        private static ushort curIndex = 0;

        public static void RegisterProcessor(PacketProcessor processor)
        {
            packetProcessorsType.Add(processor.GetType(), processor);
            packetProcessorsVal.Add(curIndex, processor);
            valConversionTable.Add(processor.GetType(), curIndex);
            curIndex++;
        }

        public static PacketProcessor GetProcessor(Type type) 
            => packetProcessorsType.TryGetValue(type, out PacketProcessor processor) ? processor : null;

        public static PacketProcessor GetProcessor(ushort val) 
            => packetProcessorsVal.TryGetValue(val, out PacketProcessor processor) ? processor : null;

        public static ushort? GetVal(Type type)
        {
            if (valConversionTable.TryGetValue(type, out ushort s)) {
                return s;
            }
            return null;
        }
    }
}
