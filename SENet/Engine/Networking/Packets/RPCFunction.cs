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
                ? (RPCInfo) Network.ClientRPCLookupTable.GetRPCInfo(methodID)
                : Network.ServerRPCLookupTable.GetRPCInfo(methodID);
            ParametersNum = RPCInfo.ParameterTypes.Length;
        }

        public override void Reset(NetPacketReader message = null) => Read(message);

        /// <summary>
        /// Creates a new RPCFunction packet.
        /// </summary>
        /// <param name="message">NetIncomingMessage.</param>
        public RPCFunction(NetPacketReader message = null) : base(message) { }

        /// <inheritdoc/>
        public override void Read(NetPacketReader message)
        {
            base.Read(message);
            try {
                NetworkID = message.GetUInt();
                MethodID = message.GetUShort();

                RPCInfo = Network.InstanceType == NetInstanceType.Server
                    ? (RPCInfo)Network.ServerRPCLookupTable.GetRPCInfo(MethodID)
                    : Network.ClientRPCLookupTable.GetRPCInfo(MethodID);
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

    public class RPCFunctionProcessor : IPacketProcessor
    {
        public void OnReceive(NetPacketReader reader, NetPeer peer, DeliveryMethod deliveryMethod)
        {
            // Invoke on server.
            RPCFunction func = Network.CacheRPCFunc;
            func.Reset(reader);
            Network.InvokeRPC(func);

            if (Network.IsServer) {
                // If the options has CallClientRPC flag, invoke the RPC on all clients.
                Network.ServerRPCLookupTable.TryGetRPCInfo(Network.CacheRPCFunc.MethodID, out RPCServerInfo info);
                if (info.Options.CallClientRPC) {
                    Network.SendRPC(func.NetworkID, deliveryMethod, 0, Scope.Broadcast, null, peer, info.StringID, func.Parameters);
                }
            }
        }
    }
}
