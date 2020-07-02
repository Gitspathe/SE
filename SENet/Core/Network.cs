using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using Open.Nat;
using SE.Core.Exceptions;
using SE.Core.Extensions;
using SE.Core.Extensions.Internal;
using SE.Engine;
using SE.Engine.Networking;
using SE.Engine.Networking.Attributes;
using SE.Engine.Networking.Internal;
using SE.Engine.Networking.Packets;

// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault

namespace SE.Core
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

        public static ConcurrentDictionary<uint, SpawnedNetObject> SpawnedNetObjects = new ConcurrentDictionary<uint, SpawnedNetObject>();
        public static ConcurrentDictionary<uint, INetLogic> NetworkObjects = new ConcurrentDictionary<uint, INetLogic>();
        public static Dictionary<string, NetPeer> Connections = new Dictionary<string, NetPeer>();

        internal static RPCLookupTable<RPCServerInfo> ServerRPCLookupTable;
        internal static RPCLookupTable<RPCClientInfo> ClientRPCLookupTable;

        private static Dictionary<Type, INetworkExtension> networkExtensions = new Dictionary<Type, INetworkExtension>();
        private static bool initialized;

        private static object rpcLock = new object();
        private static object netLogicLock = new object();
        private static object signatureLock = new object();

        // Cache packets to reduce memory allocations and CPU overhead.
        internal static RPCFunction CacheRPCFunc = new RPCFunction();
        private static EventBasedNetListener listener;
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

        internal static void LogInfo(string msg, bool important = false)
            => onLogInfoHandler?.Invoke(msg, important);
        internal static void LogWarning(string msg = null, Exception exception = null)
            => onLogWarningHandler?.Invoke(msg, exception);
        internal static void LogError(string msg = null, Exception exception = null)
            => onLogErrorHandler?.Invoke(msg, exception);

        #endregion

        #region NETWORK LOOP & MESSAGE HANDLING

        public static void Initialize()
        {
            if (initialized)
                return;

            LogInfo("Initializing network manager...", true);

            // Register the RPCFunction packet type.
            PacketProcessorManager.RegisterProcessor(new RPCFunctionProcessor());

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
            ServerRPCLookupTable = new RPCLookupTable<RPCServerInfo>();
            MethodData[] methodData = methodBundlesServerRPC.ToArray();
            methodData = methodData.ToList().OrderBy(a => a.ID).ToArray();
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
            methodData = methodBundlesClientRPC.ToArray();
            methodData = methodData.ToList().OrderBy(a => a.ID).ToArray();
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

        public static void Update(float deltaTime)
        {
            if (!initialized)
                throw new InvalidOperationException("Network manager is not initialized.");

            switch (InstanceType) {
                // Server receiving network messages.
                case NetInstanceType.Server:
                    Server.PollEvents();
                    break;
                // Client receiving network messages.
                case NetInstanceType.Client:
                    Client.PollEvents();
                    break;
                case NetInstanceType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            NetProtector.Update(deltaTime);

            // Delete objects in the destroy buffer.
            foreach (var extension in networkExtensions) {
                extension.Value.Update();
            }
        }

        internal static void SetupNetLogic(INetLogic logic, bool isOwner, NetDataReader reader = null)
        {
            lock (netLogicLock) {
                logic.Setup(CurrentNetworkID, isOwner);
                if (logic is INetPersistable persist && reader != null)
                    persist.RestoreNetworkState(reader);

                NetworkObjects.TryAdd(CurrentNetworkID, logic);
                CurrentNetworkID++;
            }
        }

        internal static void SetupNetLogic(INetLogic logic, uint networkID, bool isOwner, NetDataReader reader = null)
        {
            lock (netLogicLock) {
                logic.Setup(networkID, isOwner);
                if (logic is INetPersistable persist && reader != null)
                    persist.RestoreNetworkState(reader);

                NetworkObjects.TryAdd(networkID, logic);
                if (networkID > CurrentNetworkID)
                    CurrentNetworkID = networkID;
                if (networkID == CurrentNetworkID)
                    CurrentNetworkID++;
            }
        }

        private static void ResetListener()
        {
            listener = new EventBasedNetListener();
            listener.PeerConnectedEvent += peer
                => onPeerConnectedHandler?.Invoke(peer);
            listener.PeerDisconnectedEvent += (peer, disconnectInfo)
                => onPeerDisconnectedHandler?.Invoke(peer, disconnectInfo);
        }

        private static void HandleNetMessage(NetPacketReader msg, NetPeer peer, DeliveryMethod deliveryMethod)
        {
            try {
                // TODO: SEPacket pool.
                SEPacket packet = new SEPacket(msg.GetByte(), msg.GetBytesWithLength());
                IPacketProcessor processor = PacketProcessorManager.GetProcessor(packet.PacketType);
                if (processor != null) {
                    NetDataReader reader = NetworkPool.GetReader(packet.Buffer);
                    processor?.OnReceive(reader, peer, deliveryMethod);
                    NetworkPool.ReturnReader(reader);
                }
            } catch (Exception e) {
                NetProtector.ReportError(e, peer);
            }
        }

        public static void StartServer(int incomingPort, int outgoingPort, int maxConnections = 128, bool loopBack = false)
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
                HandleNetMessage(reader, peer, deliveryMethod);
                reader.Recycle();
            };

            Task forwardPortTask = ForwardPort(incomingPort, outgoingPort, "SE Server Port");
            forwardPortTask.Wait();

            Server = new NetManager(listener) {
                ReconnectDelay = 2000, 
                PingInterval = 2000,
                ChannelsCount = 8
            };
            if (loopBack) {
                Server.Start("localhost", "localhost", incomingPort);
            } else {
                Server.Start(incomingPort);
            }
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
                HandleNetMessage(reader, peer, deliveryMethod);
                reader.Recycle();
            };

            listener.PeerDisconnectedEvent += (peer, disconnectInfo) => {
                Connections.Remove(peer.GetUniqueID());
                Console.WriteLine("Disconnection: " + peer.EndPoint);
            };

            Task forwardPortTask = ForwardPort(incomingPort, outgoingPort, "SE Client Port");
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

        // TODO: More & better SendPacket<T> overloads.

        public static void SendPacketServer<T>(NetDataWriter netWriter, DeliveryMethod deliveryMethod, byte channel, Scope targets, NetPeer[] connections, NetPeer sender) where T : IPacketProcessor
        {
            byte b = PacketProcessorManager.GetByte(typeof(T)) ?? throw new Exception($"No network processor for type {typeof(T)} was found.");
            SEPacket packet = new SEPacket(b, netWriter.Data, netWriter.Length);
            
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
            if (recipients.Count <= 0) 
                return;

            // Send the RPC message to any recipient(s).
            NetDataWriter writer = NetworkPool.GetWriter();
            packet.WriteTo(writer);
            for (int i = 0; i < recipients.Count; i++) {
                recipients[i].Send(writer, channel, deliveryMethod);
            }
            NetworkPool.ReturnWriter(writer);
        }

        public static void SendPacketClient<T>(NetDataWriter netWriter, DeliveryMethod deliveryMethod, byte channel) where T : IPacketProcessor
        {
            byte b = PacketProcessorManager.GetByte(typeof(T)) ?? throw new Exception($"No network processor for type {typeof(T)} was found.");
            SEPacket packet = new SEPacket(b, netWriter.Data, netWriter.Length);
            
            // Find recipients.
            recipients.Clear();
            Client.GetPeersNonAlloc(recipients, ConnectionState.Connected);
            if (recipients.Count <= 0) 
                return;

            // Send the RPC message to any recipient(s).
            NetDataWriter writer = NetworkPool.GetWriter();
            packet.WriteTo(writer);
            for (int i = 0; i < recipients.Count; i++) {
                recipients[i].Send(writer, channel, deliveryMethod);
            }
            NetworkPool.ReturnWriter(writer);
        }

        private static void SendRPCServer(NetDataWriter writer, uint networkIdentity, DeliveryMethod deliveryMethod, byte channel, Scope targets, NetPeer[] connections, NetPeer sender, string method, params object[] parameters)
        {
            if (targets == Scope.None)
                throw new Exception("Invalid RPC send: server RPC target scope not specified during method " + method + ".");

            // Get the unique method signature string for the RPC.
            string methodSignature = method;
            if (!method.Contains("("))
                methodSignature = GetMethodSignature(networkIdentity, method, parameters);

            // Convert the method signature into an ID which can then be sent over the network.
            ushort? methodID = ClientRPCLookupTable.GetUshortID(methodSignature);
            if (!methodID.HasValue)
                throw new InvalidRPCException("Invalid RPC send: server RPC method '" + method + "' not found.");

            lock (rpcLock) {
                // Construct NetOutgoingMessage.
                CacheRPCFunc.Reset(networkIdentity, methodID.Value, parameters);
                CacheRPCFunc.WriteTo(writer);

                SendPacketServer<RPCFunctionProcessor>(writer, deliveryMethod, channel, targets, connections, sender);
            }
        }

        private static void SendRPCClient(NetDataWriter writer, uint networkIdentity, DeliveryMethod deliveryMethod, byte channel, string method, params object[] parameters)
        {
            // TODO: Allow clients to specify scope in some way. Needs to be secure. Maybe use attributes??
            // Get the unique method signature string for the RPC.
            string methodSignature = method;
            if (!method.Contains("("))
                methodSignature = GetMethodSignature(networkIdentity, method, parameters);

            // Convert the method signature into an ID which can then be sent over the network.
            ushort? methodID = ServerRPCLookupTable.GetUshortID(methodSignature);
            if (!methodID.HasValue)
                throw new InvalidRPCException("Invalid RPC send: client RPC method '" + method + "' not found.");

            lock (rpcLock) {
                // Construct a data writer.
                CacheRPCFunc.Reset(networkIdentity, methodID.Value, parameters);
                CacheRPCFunc.WriteTo(writer);

                SendPacketClient<RPCFunctionProcessor>(writer, deliveryMethod, channel);

                // Find recipients.
                recipients.Clear();
                Client.GetPeersNonAlloc(recipients, ConnectionState.Connected);

                if (recipients.Count <= 0) 
                    return;
            }

            // Send the RPC message to the server.
            for (int i = 0; i < recipients.Count; i++) {
                recipients[i].Send(writer, channel, deliveryMethod);
            }
        }

        #region RPC METHODS
        internal static void SendRPC(uint networkIdentity, DeliveryMethod deliveryMethod, byte channel, Scope targets, NetPeer[] connections, NetPeer sender, string method, params object[] parameters)
        {
            NetDataWriter writer = NetworkPool.GetWriter();
            try {
                if (!initialized)
                    throw new InvalidOperationException("Network manager is not initialized.");

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
                        throw new InvalidRPCException("Invalid RPC send: attempted to send RPC '" + method + "' without connection.");
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            } catch (Exception e) {
                NetworkPool.ReturnWriter(writer);
                NetProtector.ReportError(e);
            }
            NetworkPool.ReturnWriter(writer);
        }

        public static void SendRPC(RPCMethod method, params object[] parameters)
            => SendRPC(method.NetLogic.ID, method.DeliveryMethod, method.Channel, method.Scope, null, null, method.Method, parameters);
        public static void SendRPC(RPCMethod method, NetPeer recipient, params object[] parameters)
            => SendRPC(method.NetLogic.ID, method.DeliveryMethod, method.Channel, method.Scope, new [] { recipient }, null, method.Method, parameters);
        public static void SendRPC(RPCMethod method, NetPeer[] recipients, params object[] parameters) 
            => SendRPC(method.NetLogic.ID, method.DeliveryMethod, method.Channel, method.Scope, recipients, null, method.Method, parameters);

        internal static void InvokeRPC(RPCFunction rpcFunc)
        {
            if (!initialized)
                throw new InvalidOperationException("Network manager is not initialized.");

            INetLogic nObj = null;
            RPCInfo rpcInfo = null;

            lock(rpcLock) {
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
            lock (signatureLock) {
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
            lock (signatureLock) {
                signatureBuilder.Clear();
                Type t;
                if (networkID == 0) {
                    t = typeof(Network);
                } else if (NetworkObjects.TryGetValue(networkID, out INetLogic nObject)) {
                    t = nObject.GetType();
                } else throw new KeyNotFoundException("Method signature not found: " + method);

                signatureBuilder.Append(t).Append(method).Append(" (");
                if (parameters.Length > 0) {
                    for (int i = 0; i < parameters.Length; i++) {
                        signatureBuilder.Append(parameters[i].GetType().FullName);
                    }
                }

                return signatureBuilder.ToString();
            }
        }
        #endregion
    }
}
