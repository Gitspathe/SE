using LiteNetLib;
using LiteNetLib.Utils;

namespace SE.Engine.Networking.Packets
{
    /// <summary>
    /// Represents an object which processes packet data of a specific type.
    /// Automatically registered to the networking system via reflection at startup.
    /// </summary>
    public abstract class PacketProcessor
    {
        public abstract void OnReceive(INetLogic netLogic, NetDataReader reader, NetPeer peer, DeliveryMethod deliveryMethod);
        public PacketProcessor() { }
    }
}
