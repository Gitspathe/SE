using LiteNetLib;
using LiteNetLib.Utils;
using SE.Core;
using SE.Core.Exceptions;
using SE.Engine.Networking.Internal;
using System;

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

            switch (Network.InstanceType) {
                case NetInstanceType.Server: {
                    if (!NetworkRPCManager.ClientRPCLookupTable.TryGetRPCInfo(methodID, out RPCClientInfo clientInfo)) {
                        if (Network.Report) throw new Exception(); return;
                    }
                    RPCInfo = clientInfo;
                }
                break;
                case NetInstanceType.Client: {
                    if (!NetworkRPCManager.ServerRPCLookupTable.TryGetRPCInfo(methodID, out RPCServerInfo serverInfo)) {
                        if (Network.Report) throw new Exception(); return;
                    }
                    RPCInfo = serverInfo;
                }
                break;
            }

            ParametersNum = RPCInfo.ParameterTypes.Length;
        }

        public void Reset(uint networkID, NetDataReader message = null)
        {
            NetworkID = networkID;
            Read(message);
        }

        public void Read(NetDataReader message)
        {
            try {
                MethodID = message.GetUShort();

                switch (Network.InstanceType) {
                    case NetInstanceType.Server: {
                        if (!NetworkRPCManager.ServerRPCLookupTable.TryGetRPCInfo(MethodID, out RPCServerInfo serverInfo)) {
                            if (Network.Report) throw new Exception(); return;
                        }
                        RPCInfo = serverInfo;
                    }
                    break;
                    case NetInstanceType.Client: {
                        if (!NetworkRPCManager.ClientRPCLookupTable.TryGetRPCInfo(MethodID, out RPCClientInfo clientInfo)) {
                            if (Network.Report) throw new Exception(); return;
                        }
                        RPCInfo = clientInfo;
                    }
                    break;
                }

                ParametersNum = RPCInfo.ParameterTypes.Length;
                if (Parameters.Length < ParametersNum) {
                    Parameters = new object[ParametersNum];
                }
                for (int i = 0; i < ParametersNum; i++) {
                    Parameters[i] = NetData.Read(RPCInfo.ParameterTypes[i], message);
                }
            } catch (Exception e) {
                if (Network.Report) throw new MalformedPacketException("Failed to read packet.", e);
            }
        }

        public void WriteTo(NetDataWriter message)
        {
            try {
                message.Put(MethodID);
                for (int i = 0; i < ParametersNum; i++) {
                    NetData.Write(RPCInfo.ParameterTypes[i], Parameters[i], message);
                }
            } catch (Exception e) {
                if (Network.Report) throw new MalformedPacketException("Failed to write packet.", e);
            }
        }
    }

    public class RPCFunctionProcessor : PacketProcessor
    {
        public override void OnReceive(INetLogic netLogic, NetDataReader reader, NetPeer peer, DeliveryMethod deliveryMethod)
        {
            // Invoke on server.
            RPCFunction func = NetworkRPCManager.CacheRPCFunc;
            func.Reset(netLogic.ID, reader);
            NetworkRPCManager.InvokeRPC(func);

            if (Network.IsServer) {
                // If the options has CallClientRPC flag, invoke the RPC on all clients.
                NetworkRPCManager.ServerRPCLookupTable.TryGetRPCInfo(NetworkRPCManager.CacheRPCFunc.MethodID, out RPCServerInfo info);
                if (info.Options.CallClientRPC) {
                    NetworkRPCManager.SendRPC(func.NetworkID, deliveryMethod, 0, Scope.Broadcast, null, peer, info.StringID, func.Parameters);
                }
            }
        }
    }
}
