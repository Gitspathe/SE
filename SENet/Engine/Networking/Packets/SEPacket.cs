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
        public byte PacketType;
        public byte[] Buffer;

        public SEPacket(byte packetType, byte[] buffer)
        {
            Reset(packetType, buffer);
        }

        public void Reset(byte packetType, byte[] buffer)
        {
            PacketType = packetType;
            Buffer = buffer;
        }

        /// <summary>
        /// Reads a NetIncomingMessage stream into the packet.
        /// </summary>
        /// <param name="message">Stream to read.</param>
        /// <returns>Bytes read.</returns>
        public void Read(NetPacketReader message)
        {
            PacketType = message.GetByte();
            Buffer = message.GetBytesWithLength();
        }

        /// <summary>
        /// Writes the packet into a NetOutgoingMessage stream for transmission over a network.
        /// </summary>
        /// <param name="message">Stream to write into.</param>
        public void WriteTo(NetDataWriter message)
        {
            message.Put(PacketType);
            message.PutBytesWithLength(Buffer, 0, Buffer.Length);
        }

        /// <summary>
        /// Gets the size of the packet.
        /// </summary>
        /// <returns>Packet size.</returns>
        public virtual ushort Size => 1;
    }
}
