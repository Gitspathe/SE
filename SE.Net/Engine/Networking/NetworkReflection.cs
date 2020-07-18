using SE.Engine.Networking.Packets;
using SE.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SE.Core.ReflectionUtil;

namespace SE.Engine.Networking
{
    internal static class NetworkReflection
    {
        private static Func<Type, bool> packetProcessorPredicate = myType 
            => myType.IsClass
               && !myType.IsAbstract
               && myType.IsSubclassOf(typeof(PacketProcessor));

        public static void Initialize()
        {
            IEnumerable<PacketProcessor> enumerable = GetTypeInstances<PacketProcessor>(packetProcessorPredicate)
               .OrderBy(x => string.Compare(x.GetType().FullName, x.GetType().FullName, StringComparison.Ordinal));

            foreach (PacketProcessor packetProcessor in enumerable) {
                NetData.RegisterPacketType(packetProcessor);
            }
        }
    }
}
