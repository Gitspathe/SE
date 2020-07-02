using LiteNetLib;
using LiteNetLib.Utils;

namespace SE.Engine.Networking.Packets
{
    /// <summary>
    /// Represents an object which processes packet data of a specific type.
    /// </summary>
    public interface IPacketProcessor
    {
        void OnReceive(NetDataReader reader, NetPeer peer, DeliveryMethod deliveryMethod);
    }
}
