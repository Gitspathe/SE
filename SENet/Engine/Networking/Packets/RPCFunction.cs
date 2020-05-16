using System;
using LiteNetLib;
using LiteNetLib.Utils;
using SE.Core;
using SE.Core.Exceptions;
using SE.Engine.Networking.Internal;

namespace SE.Engine.Networking.Packets
{
    /// <inheritdoc/>
    internal class RPCFunction : SEPacket
    {
        public uint NetworkID;
        public ushort MethodID;
        public object[] Parameters = new object[0];
        public RPCInfo RPCInfo;

        public int ParametersNum;

        public void Reset(uint networkID, ushort methodID, object[] parameters)
        {
            NetworkID = networkID;
            MethodID = methodID;
            Parameters = parameters;
            RPCInfo = Network.InstanceType == NetInstanceType.Server
                ? (RPCInfo) Network.clientRPCLookupTable.GetRPCInfo(methodID)
                : Network.serverRPCLookupTable.GetRPCInfo(methodID);
            ParametersNum = RPCInfo.ParameterTypes.Length;

            NetData.Convert(parameters, RPCInfo.ParameterTypes);
        }

        public override void Reset(NetPacketReader message = null) => Read(message);

        /// <summary>
        /// Creates a new RPCFunction packet.
        /// </summary>
        /// <param name="message">NetIncomingMessage.</param>
        public RPCFunction(NetPacketReader message = null) : base(message) 
            => PacketType = SEPacketType.RPC;

        /// <inheritdoc/>
        public override void Read(NetPacketReader message)
        {
            base.Read(message);
            try {
                NetworkID = message.GetUInt();
                MethodID = message.GetUShort();

                RPCInfo = Network.InstanceType == NetInstanceType.Server
                    ? (RPCInfo)Network.serverRPCLookupTable.GetRPCInfo(MethodID)
                    : Network.clientRPCLookupTable.GetRPCInfo(MethodID);
                ParametersNum = RPCInfo.ParameterTypes.Length;

                if (Parameters.Length < ParametersNum) {
                    Parameters = new object[ParametersNum];
                }

                for (int i = 0; i < ParametersNum; i++) {
                    Parameters[i] = NetData.Read(RPCInfo.ParameterTypes[i], message);
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
                for (int i = 0; i < ParametersNum; i++) {
                    NetData.Write(RPCInfo.ParameterTypes[i], Parameters[i], message);
                }
            } catch (Exception e) {
                throw new MalformedPacketException("Failed to write packet.", e);
            }
        }
    }
}
