using LiteNetLib;
using LiteNetLib.Utils;

namespace SE.Engine.Networking.Packets
{
    /// <summary>
    /// Container for engine-level data sent over the network.
    /// Higher level than a regular network packet.
    /// </summary>
    public abstract class SEPacket
    {
        public byte PacketType;

        protected SEPacket(NetDataReader message = null)
        {
            if (message == null) {
                if (NetData.PacketConverters.TryGetValue(GetType(), out byte val)) {
                    PacketType = val;
                }
                return;
            }

            PacketType = message.GetByte();
        }

        public virtual void Reset(NetPacketReader message = null)
        {
            if (message == null) {
                if (NetData.PacketConverters.TryGetValue(GetType(), out byte val)) {
                    PacketType = val;
                }
                return;
            }

            PacketType = message.GetByte();
        }

        /// <summary>
        /// Reads a NetIncomingMessage stream into the packet.
        /// </summary>
        /// <param name="message">Stream to read.</param>
        /// <returns>Bytes read.</returns>
        public virtual void Read(NetPacketReader message) 
            => PacketType = message.GetByte();

        /// <summary>
        /// Writes the packet into a NetOutgoingMessage stream for transmission over a network.
        /// </summary>
        /// <param name="message">Stream to write into.</param>
        public virtual void WriteTo(NetDataWriter message)
            => message.Put(PacketType);

        /// <summary>
        /// Gets the size of the packet.
        /// </summary>
        /// <returns>Packet size.</returns>
        public virtual ushort Size => 1;
    }
}
