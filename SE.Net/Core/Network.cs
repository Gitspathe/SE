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
using SE.Engine;
using SE.Engine.Networking;
using SE.Engine.Networking.Attributes;
using SE.Engine.Networking.Internal;
using SE.Engine.Networking.Packets;

// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault

namespace SE.Core
{
    // TODO: Option to ignore all errors to improve performance. (don't throw exceptions)
    // TODO: Clean this up, split into multiple classes. Error handling is very ugly and alternatives should be investigated.
    // TODO: Peer-to-peer support? (may be VERY hard to implement at this point.)
    public static class Network
    {
        #region VARIABLES & DELEGATE EVENT HANDLERS
        public static NetInstanceType InstanceType { get; private set; } = NetInstanceType.None;
        public static NetManager Server { get; private set; }
        public static NetManager Client { get; private set; }
        public static bool IsServer => InstanceType == NetInstanceType.Server;
        public static bool IsClient => InstanceType == NetInstanceType.Client;
        public static uint CurrentNetworkID { get; set; } = 1;

        internal static object NetworkLock = new object();

        public static ConcurrentDictionary<uint, SpawnedNetObject> SpawnedNetObjects = new ConcurrentDictionary<uint, SpawnedNetObject>();
        public static ConcurrentDictionary<uint, INetLogic> NetworkObjects = new ConcurrentDictionary<uint, INetLogic>();
        public static Dictionary<string, NetPeer> Connections = new Dictionary<string, NetPeer>();

        private static Dictionary<Type, INetworkExtension> networkExtensions = new Dictionary<Type, INetworkExtension>();
        private static EventBasedNetListener listener;
        private static List<NetPeer> recipients = new List<NetPeer>();
        private static bool initialized;

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

        public static bool ReportErrors { get; set; } = true;

        internal static bool Report => ReportErrors;

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

        // TODO: NetworkLogger.
        internal static void LogInfo(string msg, bool important = false)
            => onLogInfoHandler?.Invoke(msg, important);
        internal static void LogWarning(string msg = null, Exception exception = null)
            => onLogWarningHandler?.Invoke(msg, exception);
        internal static void LogWarning(Exception exception = null)
            => LogWarning(null, exception);
        internal static void LogError(string msg = null, Exception exception = null)
            => onLogErrorHandler?.Invoke(msg, exception);
        internal static void LogError(Exception exception = null)
            => LogError(null, exception);

        #endregion

        #region NETWORK LOOP & MESSAGE HANDLING

        public static T GetNetworkObject<T>(uint networkID) where T : class, INetLogic
        {
            lock (NetworkLock) {
                return NetworkObjects.TryGetValue(networkID, out INetLogic netLogic) ? (T) netLogic : null;
            }
        }

        public static void Initialize()
        {
            if (initialized)
                return;

            LogInfo("Initializing network manager...", true);
            NetworkReflection.Initialize();

            // Adding user-defined net logics before initialization.
            RegisterExtension(new Instantiator());
            foreach (var extension in networkExtensions) {
                SetupNetLogic(extension.Value, true);
            }

            NetworkRPC.Initialize();
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
            lock (NetworkLock) {
                logic.Setup(CurrentNetworkID, isOwner);
                if (logic is INetPersistable persist && reader != null)
                    persist.RestoreNetworkState(reader);

                NetworkObjects.TryAdd(CurrentNetworkID, logic);
                CurrentNetworkID++;
            }
        }

        internal static void SetupNetLogic(INetLogic logic, uint networkID, bool isOwner, NetDataReader reader = null)
        {
            lock (NetworkLock) {
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
                SEPacket packet = new SEPacket(msg);
                PacketProcessor processor = PacketProcessorManager.GetProcessor(packet.PacketType);

                INetLogic netLogic;
                lock (NetworkLock) {
                    if (!NetworkObjects.TryGetValue(packet.NetworkID, out netLogic)) {
                        if(Report) LogWarning($"No object for network ID {packet.NetworkID} found."); return;
                    }
                }

                if (processor != null) {
                    NetDataReader reader = NetworkPool.GetReader(packet.Buffer);
                    processor?.OnReceive(netLogic, reader, peer, deliveryMethod);
                    NetworkPool.ReturnReader(reader);
                }
            } catch (Exception e) {
                NetProtector.ReportError(e, peer);
            }
        }

        public static void StartServer(int incomingPort, int outgoingPort, int maxConnections = 128, bool loopBack = false)
        {
            if (!initialized)
                LogError(new InvalidOperationException("Network manager not yet initialized."));

            ResetListener();
            listener.ConnectionRequestEvent += request => {
                if (Server.ConnectedPeersCount < maxConnections)
                    request.Accept();
                else
                    request.Reject();
            };

            // TODO: Do not accept other packets until all buffered data is received. This should fix 'No object ID found ...' spam.
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
                LogError(new InvalidOperationException("Network manager not yet initialized."));

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
                LogWarning(new Exception("Failed to forward port " + externalPort + " with UPnP. Exception occurred.", e));
            }
        }

        public static void Disconnect()
        {
            if (!initialized)
                LogError(new InvalidOperationException("Network manager not yet initialized."));

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

        public static void SendPacketServer<T>(INetLogic netLogic, NetDataWriter netWriter, DeliveryMethod deliveryMethod, byte channel, Scope targets, NetPeer[] connections, NetPeer sender) where T : PacketProcessor
        {
            if (!PacketProcessorManager.GetVal(typeof(T), out ushort s)) {
                if(Report) LogError(new Exception($"No network processor for type {typeof(T)} was found.")); return;
            }
            SEPacket packet = new SEPacket(s, netLogic.ID, netWriter.Data, netWriter.Length);
            
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
                
            lock(NetworkLock) {
                // Send the RPC message to any recipient(s).
                NetDataWriter writer = NetworkPool.GetWriter();
                packet.WriteTo(writer);
                for (int i = 0; i < recipients.Count; i++) {
                    recipients[i].Send(writer, channel, deliveryMethod);
                }
                NetworkPool.ReturnWriter(writer);
            }
        }

        public static void SendPacketClient<T>(INetLogic netLogic, NetDataWriter netWriter, DeliveryMethod deliveryMethod, byte channel) where T : PacketProcessor
        {
            if (!PacketProcessorManager.GetVal(typeof(T), out ushort s)) {
                if(Report) LogError(new Exception($"No network processor for type {typeof(T)} was found.")); return;
            }
            SEPacket packet = new SEPacket(s, netLogic.ID, netWriter.Data, netWriter.Length);

            // Find recipients.
            recipients.Clear();
            Client.GetPeersNonAlloc(recipients, ConnectionState.Connected);
            if (recipients.Count <= 0) 
                return;

            lock(NetworkLock) {
                // Send the RPC message to any recipient(s).
                NetDataWriter writer = NetworkPool.GetWriter();
                packet.WriteTo(writer);
                for (int i = 0; i < recipients.Count; i++) {
                    recipients[i].Send(writer, channel, deliveryMethod);
                }
                NetworkPool.ReturnWriter(writer);
            }
        }

        public static void SendRPC(RPCMethod method, params object[] parameters)
            => NetworkRPC.SendRPC(method.NetLogic.ID, method.DeliveryMethod, method.Channel, method.Scope, null, null, method.Method, parameters);
        public static void SendRPC(RPCMethod method, NetPeer recipient, params object[] parameters)
            => NetworkRPC.SendRPC(method.NetLogic.ID, method.DeliveryMethod, method.Channel, method.Scope, new [] { recipient }, null, method.Method, parameters);
        public static void SendRPC(RPCMethod method, NetPeer[] recipients, params object[] parameters) 
            => NetworkRPC.SendRPC(method.NetLogic.ID, method.DeliveryMethod, method.Channel, method.Scope, recipients, null, method.Method, parameters);

        public static T[] GetSubArray<T>(T[] data, int index, int length)
        {
            // TODO: Pooling?
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }

    public ref struct PeerErrorInfo
    {
        public NetPeer Sender;

    }
}
