using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeeZ.Core.Exceptions;
using DeeZ.Core.Extensions;
using DeeZ.Engine.Networking;
using DeeZ.Engine.Networking.Attributes;
using DeeZ.Engine.Networking.Internal;
using DeeZ.Engine.Networking.Packets;
using DeeZ.Engine.Utility;
using LiteNetLib;
using LiteNetLib.Utils;
using Open.Nat;
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault

namespace DeeZ.Core
{
    public static class Network
    {
        #region VARIABLES & DELEGATE EVENT HANDLERS
        public static NetInstanceType InstanceType { get; private set; } = NetInstanceType.None;
        public static NetManager Server { get; private set; }
        public static NetManager Client { get; private set; }
        public static bool IsServer => InstanceType == NetInstanceType.Server;
        public static bool IsClient => InstanceType == NetInstanceType.Client;
        public static uint CurrentNetworkID { get; set; } = 1;

        public static Dictionary<uint, SpawnedNetObject> SpawnedNetObjects = new Dictionary<uint, SpawnedNetObject>();
        public static Dictionary<uint, INetLogic> NetworkObjects = new Dictionary<uint, INetLogic>();
        public static Dictionary<string, NetPeer> Connections = new Dictionary<string, NetPeer>();

        private static EventBasedNetListener listener;
        private static RPCLookupTable<RPCServerInfo> serverRPCLookupTable;
        private static RPCLookupTable<RPCClientInfo> clientRPCLookupTable;

        private static Dictionary<Type, INetworkExtension> networkExtensions = new Dictionary<Type, INetworkExtension>();
        private static bool initialized;

        // Cache packets to reduce memory allocations and CPU overhead.
        private static NetDataWriter cachedWriter = new NetDataWriter();
        private static NetDataReader cachedReader = new NetDataReader();
        private static RPCFunction cacheFunc = new RPCFunction();
        private static DeeZPacket cachePacket = new DeeZPacket();
        private static StringBuilder signatureBuilder = new StringBuilder(256);
        private static StringBuilder methodNameBuilder = new StringBuilder(256);
        private static List<NetPeer> recipients = new List<NetPeer>();

        private static OnServerStartedEventHandler onServerStartedHandler;
        public static event OnServerStartedEventHandler OnServerStarted {
            add => onServerStartedHandler += value;
            remove => onServerStartedHandler -= value;
        }

        private static OnPeerConnectedEventHandler onPeerConnectedHandler;
        public static event OnPeerConnectedEventHandler OnPeerConnected {
            add => onPeerConnectedHandler += value;
            remove => onPeerConnectedHandler -= value;
        }

        private static OnPeerDisconnectedEventHandler onPeerDisconnectedHandler;
        public static event OnPeerDisconnectedEventHandler OnPeerDisconnected {
            add => onPeerDisconnectedHandler += value;
            remove => onPeerDisconnectedHandler -= value;
        }

        private static OnLogInfo onLogInfoHandler;
        public static event OnLogInfo OnLogInfo {
            add => onLogInfoHandler += value;
            remove => onLogInfoHandler -= value;
        }

        private static OnLogWarning onLogWarningHandler;
        public static event OnLogWarning OnLogWarning {
            add => onLogWarningHandler += value;
            remove => onLogWarningHandler -= value;
        }

        private static OnLogError onLogErrorHandler;
        public static event OnLogError OnLogError {
            add => onLogErrorHandler += value;
            remove => onLogErrorHandler -= value;
        }

        #endregion

        #region PROPERTIES
        public static NetStatistics Statistics {
            get {
                switch (InstanceType) {
                    case NetInstanceType.Client:
                        return Client.Statistics;
                    case NetInstanceType.Server:
                        return Server.Statistics;
                    default:
                        return null;
                }
            }
        }

        // TODO: Implement CurrentIPAddress property.
        public static string CurrentIPAddress {
            get {
                switch (InstanceType) {
                    case NetInstanceType.None:
                        return "No connection.";
                    case NetInstanceType.Server:
                        return "";
                    case NetInstanceType.Client:
                        return "";
                    default:
                        return "ERROR.";
                }
            }
        }

        // TODO: Implement AverageRTT property.
        public static int AverageRTT {
            get {
                switch (InstanceType) {
                    case NetInstanceType.Client:
                        //if (Client.ServerConnection != null)
                        //    return Convert.ToInt32(Client.ServerConnection.AverageRoundtripTime * 1000);
                        //else
                        return -1;
                    default:
                        return -1;
                }
            }
        }
        #endregion

        #region LOGGING

        public static void LogInfo(string msg, bool important = false)
            => onLogInfoHandler?.Invoke(msg, important);
        public static void LogWarning(string msg = null, Exception exception = null)
            => onLogWarningHandler?.Invoke(msg, exception);
        public static void LogError(string msg = null, Exception exception = null)
            => onLogErrorHandler?.Invoke(msg, exception);

        #endregion

        #region NETWORK LOOP & MESSAGE HANDLING

        public static NetDataWriter GetWriter()
        {
            cachedWriter.Reset();
            return cachedWriter;
        }

        public static NetDataReader GetReader()
        {
            cachedReader.Clear();
            return cachedReader;
        }

        // TODO: Expression trees are much slower than MethodInfo.Invoke when a function isn't a lot.
        // TODO: Therefore, MethodInfo should still be used to invoke 'uncommon' methods.
        public static void Initialize()
        {
            if (initialized)
                return;

            LogInfo("Initializing network manager...", true);

            // Adding user-defined net logics before initialization.
            RegisterExtension(new Instantiator());
            foreach (var extension in networkExtensions) {
                SetupNetLogic(extension.Value, true);
            }

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
            serverRPCLookupTable = new RPCLookupTable<RPCServerInfo>();
            MethodData[] methodData = methodBundlesServerRPC.ToArray();
            methodData = methodData.ToList().OrderBy(a => a.ID).ToArray();
            for (ushort i = 0; i < methodData.Length; i++) {
                MethodData data = methodData[i];
                ServerRPCAttribute attribute = data.Info.GetCustomAttribute<ServerRPCAttribute>();
                serverRPCLookupTable.Add(new RPCServerInfo(
                    attribute, 
                    new RPCCache(data.Info), 
                    i, 
                    data.ID));
            }

            // Construct client RPC tables.
            clientRPCLookupTable = new RPCLookupTable<RPCClientInfo>();
            methodData = methodBundlesClientRPC.ToArray();
            methodData = methodData.ToList().OrderBy(a => a.ID).ToArray();
            for (ushort i = 0; i < methodData.Length; i++) {
                MethodData data = methodData[i];
                ClientRPCAttribute attribute = data.Info.GetCustomAttribute<ClientRPCAttribute>();
                clientRPCLookupTable.Add(new RPCClientInfo(
                    attribute,
                    new RPCCache(data.Info), 
                    i, 
                    data.ID));
            }
            initialized = true;
        }

        public static void Update(float deltaTime)
        {
            if (!initialized)
                throw new InvalidOperationException("Network manager is not initialized.");

            //NetIncomingMessage msg;
            switch (InstanceType) {
                // Server receiving network messages.
                case NetInstanceType.Server:
                    Server.PollEvents();
                    NetProtector.Update(deltaTime);
                    break;

                // Client receiving network messages.
                case NetInstanceType.Client:
                    Client.PollEvents();
                    NetProtector.Update(deltaTime);
                    break;
                case NetInstanceType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Delete objects in the destroy buffer.
            foreach (var extension in networkExtensions) {
                extension.Value.Update();
            }
        }

        public static void SetupNetLogic(INetLogic logic, bool isOwner, string netState = null)
        {
            logic.Setup(CurrentNetworkID, isOwner, netState);
            NetworkObjects.Add(CurrentNetworkID, logic);
            CurrentNetworkID++;
        }

        public static void SetupNetLogic(INetLogic logic, uint networkID, bool isOwner, string netState = null)
        {
            logic.Setup(networkID, isOwner, netState);
            NetworkObjects.Add(networkID, logic);
            if(networkID > CurrentNetworkID)
                CurrentNetworkID = networkID;
            if (networkID == CurrentNetworkID)
                CurrentNetworkID++;
        }

        private static void ResetListener()
        {
            listener = new EventBasedNetListener();
            listener.PeerConnectedEvent += peer
                => onPeerConnectedHandler?.Invoke(peer);
            listener.PeerDisconnectedEvent += (peer, disconnectInfo)
                => onPeerDisconnectedHandler?.Invoke(peer, disconnectInfo);
        }

        private static void HandleNetMessageServer(NetPacketReader msg, NetPeer peer, DeliveryMethod deliveryMethod)
        {
            cachePacket.PeekType(msg);
            switch (cachePacket.PacketType) {

                // Client -> server RPC request.
                case DeeZPacketType.RPC:
                    try {
                        // Invoke on server.
                        cacheFunc.Reset(msg);
                        InvokeRPC(cacheFunc);

                        // If the options has CallClientRPC flag, invoke the RPC on all clients.
                        serverRPCLookupTable.TryGetRPCInfo(cacheFunc.MethodID, out RPCServerInfo info);
                        if (info.Options.CallClientRPC) {
                            SendRPC(cacheFunc.NetworkID, deliveryMethod, 0, Scope.Broadcast, null, peer, info.StringID, cacheFunc.Parameters);
                        }
                    } catch (Exception e) {
                        NetProtector.ReportError(e, peer);
                    }
                    break;

                case DeeZPacketType.None:
                    NetProtector.ReportError(new Exception("Server recieved packet with invalid type."), peer);
                    break;
            }
        }

        private static void HandleNetMessageClient(NetPacketReader msg, NetPeer peer, DeliveryMethod deliveryMethod)
        {
            cachePacket.PeekType(msg);
            DeeZPacketType packetType = cachePacket.PacketType;
            switch (packetType) {

                // Client receives an RPC packet from the server.
                case DeeZPacketType.RPC: {
                    try {
                        cacheFunc.Reset(msg);
                        InvokeRPC(cacheFunc);
                    } catch (Exception e) {
                        NetProtector.ReportError(e, peer);
                    }
                    break;
                }

                case DeeZPacketType.None:
                    NetProtector.ReportError(new Exception("Client recieved packet with invalid type."), peer); 
                    break;
            }
        }

        public static void StartServer(int incomingPort, int outgoingPort, int maxConnections = 128)
        {
            if (!initialized)
                throw new InvalidOperationException("Network manager not yet initialized.");

            ResetListener();
            listener.ConnectionRequestEvent += request => {
                if (Server.ConnectedPeersCount < maxConnections)
                    request.Accept();
                else
                    request.Reject();
            };

            listener.PeerConnectedEvent += peer => {
                Console.WriteLine("Connection established: " + peer.EndPoint);
                if (Connections.ContainsKey(peer.GetUniqueID())) 
                    return;

                Connections.Add(peer.GetUniqueID(), peer);
                foreach (var extension in networkExtensions) {
                    extension.Value.OnPeerConnected(peer);
                }
                NetProtector.AddPeer(peer);
            };

            listener.PeerDisconnectedEvent += (peer, disconnectInfo) => {
                Connections.Remove(peer.GetUniqueID());
                Console.WriteLine("Disconnection: " + peer.EndPoint);
                NetProtector.RemovePeer(peer);
            };

            listener.NetworkReceiveEvent += (peer, reader, deliveryMethod) => {
                HandleNetMessageServer(reader, peer, deliveryMethod);
                reader.Recycle();
            };

            Task forwardPortTask = ForwardPort(incomingPort, outgoingPort, "DeeZNet Server Port");
            forwardPortTask.Wait();

            Server = new NetManager(listener) {
                ReconnectDelay = 2000, 
                PingInterval = 2000,
                ChannelsCount = 8
            };
            Server.Start(incomingPort);
            Server.EnableStatistics = true;

            InstanceType = NetInstanceType.Server;
            onServerStartedHandler?.Invoke();
        }

        public static void Connect(string host, int incomingPort, int outgoingPort)
        {
            if (!initialized)
                throw new InvalidOperationException("Network manager not yet initialized.");

            ResetListener();

            listener.PeerConnectedEvent += peer => {
                if (!Connections.ContainsKey(peer.GetUniqueID())) {
                    Connections.Add(peer.GetUniqueID(), peer);
                    Console.WriteLine("Connection established: " + peer.EndPoint);
                }
            };

            listener.NetworkReceiveEvent += (peer, reader, deliveryMethod) => {
                HandleNetMessageClient(reader, peer, deliveryMethod);
                reader.Recycle();
            };

            listener.PeerDisconnectedEvent += (peer, disconnectInfo) => {
                Connections.Remove(peer.GetUniqueID());
                Console.WriteLine("Disconnection: " + peer.EndPoint);
            };

            Task forwardPortTask = ForwardPort(incomingPort, outgoingPort, "DeeZNet Client Port");
            forwardPortTask.Wait();

            Client = new NetManager(listener) {
                ReconnectDelay = 2000, 
                PingInterval = 2000,
                ChannelsCount = 8
            };
            Client.Start(incomingPort);
            Client.Connect(host, outgoingPort, "");
            Client.EnableStatistics = true;
            InstanceType = NetInstanceType.Client;
        }

        private static async Task ForwardPort(int internalPort, int externalPort, string description)
        {
            try {
                NatDiscoverer discoverer = new NatDiscoverer();
                CancellationTokenSource cts = new CancellationTokenSource(2000);
                NatDevice device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);
                await device.CreatePortMapAsync(new Mapping(Protocol.Udp, internalPort, externalPort, description));
                Console.WriteLine("Forwarded external port '" + externalPort + "' to internal port '" + internalPort + "' using UPnP.");
            } catch (Exception e) {
                LogWarning(exception: new Exception("Failed to forward port " + externalPort + " with UPnP. Exception occurred.", e));
            }
        }

        public static void Disconnect()
        {
            if (!initialized)
                throw new InvalidOperationException("Network manager not yet initialized.");

            if (InstanceType == NetInstanceType.Client) {
                Client.DisconnectAll();
            } else if (InstanceType == NetInstanceType.Server) {
                Server.DisconnectAll();
            }
        }
        #endregion

        #region EXTENSIONS

        public static void RegisterExtension(INetworkExtension netLogic) 
            => networkExtensions.Add(netLogic.GetType(), netLogic);

        public static INetworkExtension GetExtension<T>() where T : INetworkExtension 
            => networkExtensions.TryGetValue(typeof(T), out INetworkExtension extension) ? extension : null;

        #endregion

        #region RPC METHODS
        internal static void SendRPC(uint networkIdentity, DeliveryMethod deliveryMethod, byte channel, Scope targets, NetPeer[] connections, NetPeer sender, string method, params object[] parameters)
        {
            try {
                if (!initialized)
                    throw new InvalidOperationException("Network manager is not initialized.");

                // Get the unique method signature string for the RPC.
                string methodSignature = method;
                if (!method.Contains("("))
                    methodSignature = GetMethodSignature(networkIdentity, method, parameters);

                switch (InstanceType) {
                    // Send server -> client(s) RPC.
                    case NetInstanceType.Server: {
                        if (targets == Scope.None)
                            throw new Exception("Invalid RPC send: server RPC target scope not specified during method " + method + ".");

                        // Convert the method signature into an ID which can then be sent over the network.
                        ushort? methodID = clientRPCLookupTable.GetUshortID(methodSignature);
                        if (!methodID.HasValue)
                            throw new InvalidRPCException("Invalid RPC send: server RPC method '" + method + "' not found.");
                        
                        // Construct NetOutgoingMessage.
                        cacheFunc.Reset(networkIdentity, methodID.Value, parameters);
                        cacheFunc.WriteTo(GetWriter());

                        // Find recipients who should receive the RPC.
                        recipients.Clear();
                        switch (targets) {
                            case Scope.Unicast:
                                recipients.Add(connections[0]);
                                break;
                            case Scope.Multicast:
                                recipients.AddRange(connections);
                                break;
                            case Scope.Broadcast:
                                Server.GetPeersNonAlloc(recipients, ConnectionState.Connected);
                                recipients.Remove(sender);
                                break;
                            case Scope.None:
                                break;
                        }

                        if (recipients.Count > 0) {
                            // Send the RPC message to any recipient(s).
                            for (int i = 0; i < recipients.Count; i++) {
                                recipients[i].Send(cachedWriter, channel, deliveryMethod);
                            }
                        }
                        break;
                    }

                    // TODO: Allow clients to specify scope in some way. Needs to be secure. Maybe use attributes??
                    // Send client -> server request.
                    case NetInstanceType.Client: {
                        // Convert the method signature into an ID which can then be sent over the network.
                        ushort? methodID = serverRPCLookupTable.GetUshortID(methodSignature);
                        if (!methodID.HasValue)
                            throw new InvalidRPCException("Invalid RPC send: client RPC method '" + method + "' not found.");

                        // Construct a data writer.
                        cacheFunc.Reset(networkIdentity, methodID.Value, parameters);
                        cacheFunc.WriteTo(GetWriter());

                        // Find recipients.
                        recipients.Clear();
                        Client.GetPeersNonAlloc(recipients, ConnectionState.Connected);

                        // Send the RPC message to the server.
                        if (recipients.Count > 0) {
                            for (int i = 0; i < recipients.Count; i++) {
                                recipients[i].Send(cachedWriter, channel, deliveryMethod);
                            }
                        }
                        break;
                    }

                    // Invalid network states.
                    case NetInstanceType.None:
                        throw new InvalidRPCException("Invalid RPC send: attempted to send RPC '" + method + "' without connection.");
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            } catch (Exception e) {
                NetProtector.ReportError(e);
            }
        }

        public static void SendRPC(RPCMethod method, params object[] parameters)
            => SendRPC(method.NetLogic.ID, method.DeliveryMethod, method.Channel, method.Scope, null, null, method.Method, parameters);

        public static void SendRPC(RPCMethod method, NetPeer recipient, params object[] parameters)
            => SendRPC(method.NetLogic.ID, method.DeliveryMethod, method.Channel, method.Scope, new [] { recipient }, null, method.Method, parameters);

        public static void SendRPC(RPCMethod method, NetPeer[] recipients, params object[] parameters) 
            => SendRPC(method.NetLogic.ID, method.DeliveryMethod, method.Channel, method.Scope, recipients, null, method.Method, parameters);

        private static void InvokeRPC(RPCFunction rpcFunc)
        {
            if (!initialized)
                throw new InvalidOperationException("Network manager is not initialized.");

            INetLogic nObj = null;
            RPCInfo rpcInfo = null;

            // If the networkID is zero, the RPC's target is this (Network static). Otherwise, search current NetworkObjects for the RPC target.
            if (rpcFunc.NetworkID != 0) {
                NetworkObjects.TryGetValue(rpcFunc.NetworkID, out nObj);
            }
            if (nObj == null) {
                LogWarning(exception: new Exception("No NetworkObject for ID: " + rpcFunc.NetworkID + " found."));
                return;
            }

            // Search the client or server RPC lookup tables for the method which the RPC needs to invoke.
            switch (InstanceType) {
                case NetInstanceType.Server:
                    if (serverRPCLookupTable.TryGetRPCInfo(rpcFunc.MethodID, out RPCServerInfo serverRPC)) {
                        rpcInfo = serverRPC;
                    }
                    break;
                case NetInstanceType.Client:
                    if (clientRPCLookupTable.TryGetRPCInfo(rpcFunc.MethodID, out RPCClientInfo clientRPC)) {
                        rpcInfo = clientRPC;
                    }
                    break;
                case NetInstanceType.None:
                    throw new InvalidRPCException("Invalid RPC invocation: attempted to invoke RPC '" + rpcFunc.MethodID + "' without connection.");
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (rpcInfo == null)
                throw new InvalidRPCException("Invalid RPC invocation: RPC method " + rpcFunc.MethodID + " not found.");

            // If networkID is zero, invoke the RPC on this (Network static).
            object[] parameters = GetSubArray(rpcFunc.Parameters, 0, rpcFunc.ParametersNum);
            if (rpcFunc.NetworkID == 0) {
                rpcInfo.Invoke(null, parameters);
            } else {
                // if networkID is greater than zero, find the INetLogic the RPC needs to be invoked on.
                if (!NetworkObjects.TryGetValue(rpcFunc.NetworkID, out INetLogic nObject))
                    throw new InvalidRPCException("Invalid RPC invocation: object with networkID "+rpcFunc.NetworkID+" does not exist.");
                
                // Invoke the RPC.
                try {
                    rpcInfo.Invoke(nObject, parameters);
                } catch (Exception e) {
                    throw new InvalidRPCException("Failed to invoke RPC ID '"+rpcFunc.MethodID+"'. Ensure that the parameters passed are valid.", e);
                }
            }
        }

        public static T[] GetSubArray<T>(T[] data, int index, int length)
        {
            // TODO: Pooling?
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        #endregion

        #region UTILITY METHODS
        private static string GetName(MethodInfo info)
        {
            methodNameBuilder.Clear();
            ParameterInfo[] pInfo = info.GetParameters();
            methodNameBuilder.Append(info.DeclaringType).Append(" Void ").Append(info.Name).Append(" (");
            for (int i = 0; i < pInfo.Length; i++) {
                methodNameBuilder.Append(pInfo[i].ParameterType.FullName);
                if (i + 1 < pInfo.Length) {
                    methodNameBuilder.Append(", ");
                }
            }
            methodNameBuilder.Append(")");
            return methodNameBuilder.ToString();
        }

        private static string GetMethodSignature(uint networkID, string method, object[] parameters)
        {
            signatureBuilder.Clear();
            Type t;
            if (networkID == 0) {
                t = typeof(Network);
            } else if(NetworkObjects.TryGetValue(networkID, out INetLogic nObject)) {
                t = nObject.GetType();
            } else throw new KeyNotFoundException("Method signature not found.");

            signatureBuilder.Append(t).Append(" Void ").Append(method).Append(" (");
            if (parameters.Length > 0) {
                for (int i = 0; i < parameters.Length; i++) {
                    signatureBuilder.Append(parameters[i].GetType().FullName);
                    if (i + 1 < parameters.Length) {
                        signatureBuilder.Append(", ");
                    }
                }
            }
            signatureBuilder.Append(")");
            return signatureBuilder.ToString();
        }
        #endregion
    }
}
