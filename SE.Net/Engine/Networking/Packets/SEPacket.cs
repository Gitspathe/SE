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
        public const int _HEADER_OFFSET = sizeof(ushort) + sizeof(uint);

        // Serialized.
        public ushort PacketType;
        public uint NetworkID;
        public byte[] Buffer;

        // NOT serialized.
        public int BufferLength;

        public SEPacket(ushort packetType, uint networkID, byte[] buffer, int bufferLength = -1)
        {
            Reset(packetType, networkID, buffer, bufferLength);
        }

        public SEPacket(NetPacketReader reader)
        {
            Read(reader);
        }

        public void Reset(ushort packetType, uint networkID, byte[] buffer, int bufferLength = -1)
        {
            if (bufferLength == -1)
                bufferLength = buffer.Length;

            PacketType = packetType;
            NetworkID = networkID;
            BufferLength = bufferLength;
            Buffer = buffer;
        }

        public void Reset(NetPacketReader reader)
        {
            Read(reader);
        }

        /// <summary>
        /// Reads a NetIncomingMessage stream into the packet.
        /// </summary>
        /// <param name="message">Stream to read.</param>
        /// <returns>Bytes read.</returns>
        public void Read(NetPacketReader message)
        {
            PacketType = message.GetUShort();
            NetworkID = message.GetUInt();

            // To get the buffer, we simply grab the rest of the data from the NetPacketReader.
            // By doing this, 4 less bytes are used, because the byte array length isn't serialized.
            // TODO: When SEPacket pooling is added, the buffer can be resized as needed.
            BufferLength = message.UserDataSize - _HEADER_OFFSET;
            Buffer = new byte[BufferLength];
            message.GetBytes(Buffer, BufferLength);
        }

        /// <summary>
        /// Writes the packet into a NetOutgoingMessage stream for transmission over a network.
        /// </summary>
        /// <param name="message">Stream to write into.</param>
        public void WriteTo(NetDataWriter message)
        {
            message.Put(PacketType);
            message.Put(NetworkID);
            message.Put(Buffer, 0, BufferLength);
        }
    }
}
