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
        public ushort PacketType;
        public int BufferLength;
        public byte[] Buffer;

        public SEPacket(ushort packetType, byte[] buffer, int bufferLength = -1)
        {
            if(bufferLength == -1)
                bufferLength = buffer.Length;

            Reset(packetType, buffer, bufferLength);
        }

        public void Reset(ushort packetType, byte[] buffer, int bufferLength = -1)
        {
            if(bufferLength == -1)
                bufferLength = buffer.Length;

            PacketType = packetType;
            BufferLength = bufferLength;
            Buffer = buffer;
        }

        /// <summary>
        /// Reads a NetIncomingMessage stream into the packet.
        /// </summary>
        /// <param name="message">Stream to read.</param>
        /// <returns>Bytes read.</returns>
        public void Read(NetPacketReader message)
        {
            PacketType = message.GetUShort();
            Buffer = message.GetBytesWithLength();
            BufferLength = Buffer.Length;
        }

        /// <summary>
        /// Writes the packet into a NetOutgoingMessage stream for transmission over a network.
        /// </summary>
        /// <param name="message">Stream to write into.</param>
        public void WriteTo(NetDataWriter message)
        {
            message.Put(PacketType);
            message.PutBytesWithLength(Buffer, 0, BufferLength);
        }
    }
}
