using System;
using LiteNetLib;
using LiteNetLib.Utils;
using SE.Core;
using SE.Core.Exceptions;
using SE.Engine.Networking.Internal;

namespace SE.Engine.Networking.Packets
{
    internal class RPCFunction
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

        public void Reset(NetDataReader message = null) => Read(message);

        public void Read(NetDataReader message)
        {
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

        public void WriteTo(NetDataWriter message)
        {
            try {
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

    public class RPCFunctionProcessor : PacketProcessor
    {
        public override void OnReceive(NetDataReader reader, NetPeer peer, DeliveryMethod deliveryMethod)
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
