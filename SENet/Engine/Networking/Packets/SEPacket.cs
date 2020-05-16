using LiteNetLib;
using LiteNetLib.Utils;

namespace SE.Engine.Networking.Packets
{
    /// <summary>
    /// Container for engine-level data sent over the network.
    /// Higher level than a regular network packet.
    /// </summary>
    public class SEPacket
    {
        public SEPacketType PacketType;

        public SEPacket(NetPacketReader message = null)
        {
            PacketType = SEPacketType.None;
            if (message == null)
                return;

            PacketType = (SEPacketType)message.GetByte();
        }

        public virtual void Reset(NetPacketReader message = null)
        {
            PacketType = SEPacketType.None;
            if (message == null)
                return;

            PacketType = (SEPacketType)message.GetByte();
        }

        /// <summary>
        /// Reads a NetIncomingMessage stream into the packet.
        /// </summary>
        /// <param name="message">Stream to read.</param>
        /// <returns>Bytes read.</returns>
        public virtual void Read(NetPacketReader message) 
            => PacketType = (SEPacketType)message.GetByte();

        public void PeekType(NetPacketReader message) 
            => PacketType = (SEPacketType)message.PeekByte();

        /// <summary>
        /// Writes the packet into a NetOutgoingMessage stream for transmission over a network.
        /// </summary>
        /// <param name="message">Stream to write into.</param>
        public virtual void WriteTo(NetDataWriter message)
            => message.Put((byte) PacketType);

        /// <summary>
        /// Gets the size of the packet.
        /// </summary>
        /// <returns>Packet size.</returns>
        public virtual ushort Size => 1;
    }
}
