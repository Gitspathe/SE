using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using LiteNetLib;
using LiteNetLib.Utils;
using SE.Core.Exceptions;
using SE.Core.Extensions;
using SE.Engine.Networking;
using SE.Engine.Networking.Attributes;
using SE.Engine.Networking.Internal;
using SE.Engine.Networking.Packets;
using static SE.Core.Network;

namespace SE.Core
{
    internal static class NetworkRPC
    {
        internal static RPCLookupTable<RPCServerInfo> ServerRPCLookupTable;
        internal static RPCLookupTable<RPCClientInfo> ClientRPCLookupTable;
        internal static RPCFunction CacheRPCFunc = new RPCFunction();

        private static StringBuilder signatureBuilder = new StringBuilder(256);
        private static StringBuilder methodNameBuilder = new StringBuilder(256);

        private static bool initialized;

        internal static void Initialize()
        {
            // Use reflection to determine which functions in the assemblies are RPCs.
            LogInfo("  Loading RPC methods...", true);
            List<MethodData> methodBundlesServerRPC = new List<MethodData>();
            List<MethodData> methodBundlesClientRPC = new List<MethodData>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                MethodInfo[] methods = assembly.GetTypes()
                    .SelectMany(t => t.GetMethods())
                    .Where(m => m.GetCustomAttributes(typeof(ServerRPCAttribute), false).Length > 0)
                    .ToArray();
                foreach (MethodInfo info in methods) {
                    methodBundlesServerRPC.Add(new MethodData(GetName(info), info));
                    LogInfo("  S-RPC: " + GetName(info));
                }

                methods = assembly.GetTypes()
                    .SelectMany(t => t.GetMethods())
                    .Where(m => m.GetCustomAttributes(typeof(ClientRPCAttribute), false).Length > 0)
                    .ToArray();
                foreach (MethodInfo info in methods) {
                    string methodName = GetName(info).Replace("_CLIENT", "");
                    methodBundlesClientRPC.Add(new MethodData(methodName, info));
                    LogInfo("  C-RPC: " + methodName);
                }
            }

            // Construct server RPC tables.
            ServerRPCLookupTable = new RPCLookupTable<RPCServerInfo>();
            MethodData[] methodData = methodBundlesServerRPC.OrderBy(a => a.ID).ToArray();
            for (ushort i = 0; i < methodData.Length; i++) {
                MethodData data = methodData[i];
                ServerRPCAttribute attribute = data.Info.GetCustomAttribute<ServerRPCAttribute>();
                ServerRPCLookupTable.Add(new RPCServerInfo(
                    attribute, 
                    new RPCCache(data.Info), 
                    i, 
                    data.ID, 
                    data.Info.GetParameterTypes()));
            }

            // Construct client RPC tables.
            ClientRPCLookupTable = new RPCLookupTable<RPCClientInfo>();
            methodData = methodBundlesClientRPC.OrderBy(a => a.ID).ToArray();
            for (ushort i = 0; i < methodData.Length; i++) {
                MethodData data = methodData[i];
                ClientRPCAttribute attribute = data.Info.GetCustomAttribute<ClientRPCAttribute>();
                ClientRPCLookupTable.Add(new RPCClientInfo(
                    attribute,
                    new RPCCache(data.Info), 
                    i, 
                    data.ID, 
                    data.Info.GetParameterTypes()));
            }
            initialized = true;
        }

        internal static void SendRPCClient(NetDataWriter writer, uint networkIdentity, DeliveryMethod deliveryMethod, byte channel, string method, params object[] parameters)
        {
            // TODO: Allow clients to specify scope in some way. Needs to be secure. Maybe use attributes??
            // Get the unique method signature string for the RPC.
            string methodSignature = method;
            if (!method.Contains("("))
                methodSignature = GetMethodSignature(networkIdentity, method, parameters);

            // Convert the method signature into an ID which can then be sent over the network.
            if(!ServerRPCLookupTable.TryGetUshortID(methodSignature, out ushort methodID)) {
                if(Report) LogError(exception: new InvalidRPCException($"Invalid RPC send: client RPC method '{method}' not found.")); return;
            }

            // Find INetLogic object for network ID.
            lock (NetworkLock) {
                if (!NetworkObjects.TryGetValue(networkIdentity, out INetLogic netLogic)) {
                    if(Report) LogWarning(exception: new InvalidRPCException($"No object for network ID {networkIdentity} found.")); return;
                }

                // Construct a data writer.
                CacheRPCFunc.Reset(networkIdentity, methodID, parameters);
                CacheRPCFunc.WriteTo(writer);
                SendPacketClient<RPCFunctionProcessor>(netLogic, writer, deliveryMethod, channel);
            }
        }

        internal static void SendRPCServer(NetDataWriter writer, uint networkIdentity, DeliveryMethod deliveryMethod, byte channel, Scope targets, NetPeer[] connections, NetPeer sender, string method, params object[] parameters)
        {
            if (targets == Scope.None) {
                if(Report) LogError(exception: new Exception($"Invalid RPC send: server RPC target scope not specified during method {method}.")); return;
            }

            // Get the unique method signature string for the RPC.
            string methodSignature = method;
            if (!method.Contains("("))
                methodSignature = GetMethodSignature(networkIdentity, method, parameters);

            // Convert the method signature into an ID which can then be sent over the network.
            if (!ClientRPCLookupTable.TryGetUshortID(methodSignature, out ushort methodID)) {
                if(Report) LogError(exception: new InvalidRPCException($"Invalid RPC send: server RPC method '{method}' not found.")); return;
            }

            // Find INetLogic object for network ID.
            lock (NetworkLock) {
                if (!NetworkObjects.TryGetValue(networkIdentity, out INetLogic netLogic)) {
                    if(Report) LogWarning(exception: new InvalidRPCException($"No object for network ID {networkIdentity} found.")); return;
                }

                // Construct NetOutgoingMessage.
                CacheRPCFunc.Reset(networkIdentity, methodID, parameters);
                CacheRPCFunc.WriteTo(writer);
                SendPacketServer<RPCFunctionProcessor>(netLogic, writer, deliveryMethod, channel, targets, connections, sender);
            }
        }

        internal static void SendRPC(uint networkIdentity, DeliveryMethod deliveryMethod, byte channel, Scope targets, NetPeer[] connections, NetPeer sender, string method, params object[] parameters)
        {
            NetDataWriter writer = NetworkPool.GetWriter();
            try {
                if (!initialized)
                    LogError(exception: new InvalidOperationException("Network manager is not initialized."));

                switch (InstanceType) {
                    // Send server -> client(s) RPC.
                    case NetInstanceType.Server:
                        SendRPCServer(writer, networkIdentity, deliveryMethod, channel, targets, connections, sender, method, parameters);
                        break;
                    // Send client -> server RPC.
                    case NetInstanceType.Client:
                        SendRPCClient(writer, networkIdentity, deliveryMethod, channel, method, parameters);
                        break;

                    // Invalid network states.
                    case NetInstanceType.None:
                        if(Report) LogError(exception: new InvalidRPCException($"Invalid RPC send: attempted to send RPC '{method}' without connection.")); return;
                    default:
                        if(Report) LogError(exception: new ArgumentOutOfRangeException()); return;
                }
            } catch (Exception e) {
                NetworkPool.ReturnWriter(writer);
                NetProtector.ReportError(e);
            } finally {
                NetworkPool.ReturnWriter(writer);
            }
        }

        internal static void InvokeRPC(RPCFunction rpcFunc)
        {
            if (!initialized)
                LogError(exception: new InvalidOperationException("Network manager is not initialized."));

            INetLogic nObj = null;
            RPCInfo rpcInfo = null;

            lock(NetworkLock) {
                // If the networkID is zero, the RPC's target is this (Network static). Otherwise, search current NetworkObjects for the RPC target.
                if (rpcFunc.NetworkID != 0) {
                    NetworkObjects.TryGetValue(rpcFunc.NetworkID, out nObj);
                }
                if (nObj == null) {
                    if(Report) LogWarning(exception: new Exception($"No NetworkObject for ID: {rpcFunc.NetworkID} found.")); return;
                }

                // Search the client or server RPC lookup tables for the method which the RPC needs to invoke.
                switch (InstanceType) {
                    case NetInstanceType.Server: {
                        if (ServerRPCLookupTable.TryGetRPCInfo(rpcFunc.MethodID, out RPCServerInfo serverRPC)) {
                            rpcInfo = serverRPC;
                        }
                    } break;
                    case NetInstanceType.Client: {
                        if (ClientRPCLookupTable.TryGetRPCInfo(rpcFunc.MethodID, out RPCClientInfo clientRPC)) {
                            rpcInfo = clientRPC;
                        }
                    } break;
                    case NetInstanceType.None:
                        if(Report) LogError(exception: new InvalidRPCException($"Invalid RPC invocation: attempted to invoke RPC '{rpcFunc.MethodID}' without connection.")); return;
                    default:
                        if(Report) LogError(exception: new ArgumentOutOfRangeException()); return;
                }
                if (rpcInfo == null) {
                    if (Report) LogError(exception: new InvalidRPCException($"Invalid RPC invocation: RPC method '{rpcFunc.MethodID}' not found.")); return;
                }

                // If networkID is zero, invoke the RPC on this (Network static).
                object[] parameters = GetSubArray(rpcFunc.Parameters, 0, rpcFunc.ParametersNum);
                if (rpcFunc.NetworkID == 0) {
                    rpcInfo.Invoke(null, parameters);
                } else {
                    // if networkID is greater than zero, find the INetLogic the RPC needs to be invoked on.
                    if (!NetworkObjects.TryGetValue(rpcFunc.NetworkID, out INetLogic nObject)) {
                        if(Report) LogError(exception: new InvalidRPCException($"Invalid RPC invocation: object with networkID {rpcFunc.NetworkID} does not exist.")); return;
                    }

                    // Invoke the RPC.
                    try {
                        rpcInfo.Invoke(nObject, parameters);
                    } catch (Exception e) {
                        if(Report) LogError(exception: new InvalidRPCException($"Failed to invoke RPC ID '{rpcFunc.MethodID}'. Ensure that the parameters passed are valid.", e));
                    }
                }
            }
        }

        private static string GetName(MethodInfo info)
        {
            lock (NetworkLock) {
                methodNameBuilder.Clear();
                ParameterInfo[] pInfo = info.GetParameters();
                methodNameBuilder.Append(info.DeclaringType).Append(info.Name).Append(" (");
                for (int i = 0; i < pInfo.Length; i++) {
                    methodNameBuilder.Append(pInfo[i].ParameterType.FullName);
                }
                return methodNameBuilder.ToString();
            }
        }

        private static string GetMethodSignature(uint networkID, string method, object[] parameters)
        {
            lock (NetworkLock) {
                signatureBuilder.Clear();
                Type t;
                if (networkID == 0) {
                    t = typeof(Network);
                } else if (NetworkObjects.TryGetValue(networkID, out INetLogic nObject)) {
                    t = nObject.GetType();
                } else {
                    if(Report) LogError(exception: new KeyNotFoundException("Method signature not found: " + method)); return null;
                }

                signatureBuilder.Append(t).Append(method).Append(" (");
                if (parameters.Length > 0) {
                    for (int i = 0; i < parameters.Length; i++) {
                        signatureBuilder.Append(parameters[i].GetType().FullName);
                    }
                }

                return signatureBuilder.ToString();
            }
        }
    }
}
