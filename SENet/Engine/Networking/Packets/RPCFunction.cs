using System;
using DeeZ.Core.Exceptions;
using LiteNetLib;
using LiteNetLib.Utils;

namespace DeeZ.Engine.Networking.Packets
{
    /// <inheritdoc/>
    internal class RPCFunction : DeeZPacket
    {
        public uint NetworkID;
        public ushort MethodID;
        public object[] Parameters = new object[0];
        public byte[] ParameterTypes = new byte[0];

        public int ParametersNum { get; protected set; }

        public void Reset(uint networkID, ushort methodID, object[] parameters)
        {
            NetworkID = networkID;
            MethodID = methodID;
            ParametersNum = parameters.Length;
            Parameters = parameters;
            if (ParameterTypes.Length < ParametersNum) {
                ParameterTypes = new byte[ParametersNum];
            }
            NetData.Convert(parameters, ParameterTypes);
        }

        public override void Reset(NetPacketReader message = null) => Read(message);

        /// <summary>
        /// Creates a new RPCFunction packet.
        /// </summary>
        /// <param name="networkID">Network ID to invoke RPC on.</param>
        /// <param name="methodID">Method ID.</param>
        /// <param name="parameters">Parameter values passed to the RPC method.</param>
        /// <param name="types">Parameter types sent to the RPC method.</param>
        public RPCFunction(ushort networkID, ushort methodID, object[] parameters, byte[] types)
        {
            MethodID = methodID;
            NetworkID = networkID;
            Parameters = parameters;
            ParametersNum = parameters.Length;
            ParameterTypes = types;
            PacketType = DeeZPacketType.RPC;
        }

        /// <summary>
        /// Creates a new RPCFunction packet.
        /// </summary>
        /// <param name="message">NetIncomingMessage.</param>
        public RPCFunction(NetPacketReader message = null) : base(message) 
            => PacketType = DeeZPacketType.RPC;

        /// <inheritdoc/>
        public override void Read(NetPacketReader message)
        {
            base.Read(message);
            try {
                NetworkID = message.GetUInt();
                MethodID = message.GetUShort();
                ParametersNum = message.GetByte();
                if (Parameters.Length < ParametersNum) {
                    Parameters = new object[ParametersNum];
                    ParameterTypes = new byte[ParametersNum];
                }
                for (int i = 0; i < ParametersNum; i++) {
                    ParameterTypes[i] = message.GetByte();
                    Parameters[i] = NetData.Read(ParameterTypes[i], message);
                }
            } catch (Exception e) {
                throw new MalformedPacketException("Failed to read packet.", e);
            }
        }

        /// <inheritdoc/>
        public override void WriteTo(NetDataWriter message)
        {
            try {
                base.WriteTo(message);
                message.Put(NetworkID);
                message.Put(MethodID);
                message.Put((byte)ParametersNum);
                for (int i = 0; i < ParametersNum; i++) {
                    message.Put(ParameterTypes[i]);
                    NetData.Write(ParameterTypes[i], Parameters[i], message);
                }
            } catch (Exception e) {
                throw new MalformedPacketException("Failed to write packet.", e);
            }
        }
    }
}
