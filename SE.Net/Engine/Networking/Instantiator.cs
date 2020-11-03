using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using SE.Core.Extensions;
using SE.Engine.Networking.Attributes;
using SE.Utility;
using static SE.Core.Network;

namespace SE.Engine.Networking
{
    public class Instantiator : INetworkExtension
    {
        private RPCMethod instantiateMethod;
        private RPCMethod destroyMethod;

        private static readonly SelfReferenceDictionary<string, Type> spawnables = new SelfReferenceDictionary<string, Type>();
        private static readonly List<uint> clearBuffer = new List<uint>();

        public uint ID { get; private set; }
        public bool IsSetup { get; private set; }
        public bool IsOwner { get; private set; }

        // Cache...
        private List<string> netStates = new List<string>();
        private List<uint> netIDs = new List<uint>();

        public void Setup(uint id, bool isOwner)
        {
            ID = id;
            IsOwner = isOwner;
            IsSetup = true;
        }

        public void Update()
        {
            for (int i = 0; i < clearBuffer.Count; i++) {
                clearBuffer.RemoveAt(i);
            }
        }

        public void OnPeerConnected(NetPeer peer)
        {
            lock (NetworkLock) {
                foreach (uint i in SpawnedNetObjects.Keys) {
                    InstantiateFromBuffer(SpawnedNetObjects[i], peer, i);
                }
            }
        }

        internal bool CleanNetObject(SpawnedNetObject netObj)
        {
            lock (NetworkLock) {
                SpawnedNetObjects.Remove(netObj.NetworkID);
                if (netObj.NetLogic is INetInstantiatable instantiatable)
                    instantiatable.NetClean();
            }

            return true;
        }

        internal void InstantiateFromBuffer(SpawnedNetObject netObj, NetPeer conn, uint netID, bool isOwner = false)
        {
            if (InstanceType != NetInstanceType.Server)
                return;

            byte[] data = null;
            byte[] netState = null;
            byte[] instantiateParams = null;
            NetDataWriter writer = null;
            lock (NetworkLock) {
                try {
                    if (netObj.NetLogic is INetInstantiatable instantiatable) {
                        writer = NetworkPool.GetWriter();
                        object[] parameters = instantiatable.InstantiateParameters;
                        foreach (object writeObj in parameters) {
                            writer.Put(NetData.PacketConverters[writeObj.GetType()]);
                            NetData.Write(writeObj.GetType(), writeObj, writer);
                        }

                        instantiateParams = writer.CopyData();
                        NetworkPool.ReturnWriter(writer);

                        writer = NetworkPool.GetWriter();
                        data = instantiatable.GetBufferedData(writer);
                        NetworkPool.ReturnWriter(writer);
                    }

                    if (netObj.NetLogic is INetPersistable persist) {
                        writer = NetworkPool.GetWriter();
                        netState = persist.SerializeNetworkState(writer);
                        NetworkPool.ReturnWriter(writer);
                    }
                } catch (Exception) {
                    NetworkPool.ReturnWriter(writer);
                    throw;
                }
            }

            SendRPC(instantiateMethod, 
                conn, 
                netObj.SpawnableID, 
                conn.GetUniqueID() == netObj.Owner,
                netObj.NetworkID, 
                netState ?? new byte[0], 
                data ?? new byte[0], 
                instantiateParams ?? new byte[0]);
        }

        public void Instantiate(string type, string owner = "SERVER", object[] parameters = null)
        {
            NetDataWriter writer = null;
            INetLogic logic = null;
            byte[] returned = null;
            byte[] netState = null;
            byte[] paramsData;

            lock(NetworkLock) {
                try {
                    if (InstanceType != NetInstanceType.Server)
                        throw new InvalidOperationException("Attempted to instantiate on a client. Instantiate can only be called on the server.");
                    if (!spawnables.TryGetValue(type, out Type t))
                        throw new Exception("No Spawnable of Type " + type + " found in dictionary.");

                    object obj = parameters != null
                        ? Activator.CreateInstance(t, parameters)
                        : Activator.CreateInstance(t);

                    if (obj is INetLogicProxy proxy)
                        logic = proxy.NetLogic;
                    else if (obj is INetLogic netLogic)
                        logic = netLogic;

                    if (logic == null)
                        throw new Exception("NetLogic not found.");

                    SetupNetLogic(logic, owner == "SERVER");
                    SpawnedNetObjects.Add(logic.ID, new SpawnedNetObject(logic, logic.ID, type, owner));

                    writer = NetworkPool.GetWriter();
                    if (parameters != null) {
                        foreach (object writeObj in parameters) {
                            writer.Put(NetData.PacketConverters[writeObj.GetType()]);
                            NetData.Write(writeObj.GetType(), writeObj, writer);
                        }
                    }

                    paramsData = writer.Length > 0 ? writer.CopyData() : null;
                    NetworkPool.ReturnWriter(writer);

                    if (logic is INetInstantiatable instantiatable) {
                        writer = NetworkPool.GetWriter();
                        instantiatable.OnNetworkInstantiatedServer(type, owner, writer);
                        NetworkPool.ReturnWriter(writer);

                        writer = NetworkPool.GetWriter();
                        returned = instantiatable.GetBufferedData(writer);
                        NetworkPool.ReturnWriter(writer);
                    }

                    if (logic is INetPersistable persist) {
                        writer = NetworkPool.GetWriter();
                        netState = persist.SerializeNetworkState(writer);
                        NetworkPool.ReturnWriter(writer);
                    }
                } catch (Exception e) {
                    NetworkPool.ReturnWriter(writer);
                    LogError(null, new Exception("", e));
                    return;
                }
            }

            foreach (string playerID in Connections.Keys) {
                SendRPC(instantiateMethod, 
                    Connections[playerID], 
                    type, 
                    playerID == owner, 
                    logic.ID, 
                    netState ?? new byte[0], 
                    returned ?? new byte[0], 
                    paramsData ?? new byte[0]);
            }
        }

        [ClientRPC(frequent: true)]
        public void Instantiate_CLIENT(string type, bool isOwner, uint netID, byte[] netState, byte[] data, byte[] paramsData)
        {
            lock(NetworkLock) {
                NetDataReader reader = null;
                try {
                    if (!spawnables.TryGetValue(type, out Type t))
                        throw new Exception("No Spawnable of Type " + type + " found in dictionary.");

                    List<object> instantiateParameters = new List<object>();
                    reader = NetworkPool.GetReader(paramsData);
                    while (!reader.EndOfData) {
                        instantiateParameters.Add(NetData.Read(NetData.PacketBytes[reader.GetByte()], reader));
                    }
                    NetworkPool.ReturnReader(reader);

                    // TODO: Temporary fix to prevent error where JSON deserializes floats to doubles.
                    for (int i = 0; i < instantiateParameters.Count; i++) {
                        if (instantiateParameters[i] is double) {
                            instantiateParameters[i] = Convert.ToSingle(instantiateParameters[i]);
                        }
                    }

                    object obj = Activator.CreateInstance(t, instantiateParameters.ToArray());

                    INetLogic logic = null;
                    if (obj is INetLogicProxy proxy)
                        logic = proxy.NetLogic;
                    else if (obj is INetLogic netLogic)
                        logic = netLogic;

                    if (logic == null)
                        throw new Exception("NetLogic not found.");

                    reader = NetworkPool.GetReader(netState);
                    SetupNetLogic(logic, netID, isOwner, reader);
                    NetworkPool.ReturnReader(reader);

                    SpawnedNetObjects.Add(logic.ID, new SpawnedNetObject(logic, logic.ID, type));

                    if (logic is INetInstantiatable instantiatable) {
                        reader = NetworkPool.GetReader(data);
                        instantiatable.OnNetworkInstantiatedClient(type, isOwner, reader);
                        NetworkPool.ReturnReader(reader);
                    }
                } catch (Exception) {
                    NetworkPool.ReturnReader(reader);
                    throw;
                }
            }
        }

        public void Destroy(uint netID)
        {
            try {
                // For security, only the server is authorized to destroy networked GameObjects.
                if (InstanceType != NetInstanceType.Server)
                    throw new InvalidOperationException("Destroying a networked GameObject is only authorized for the server.");

                Destroy_CLIENT(netID);
                SendRPC(destroyMethod, netID);
            } catch (Exception e) {
                LogError(exception: e);
            }
        }

        [ClientRPC(frequent: true)]
        public void Destroy_CLIENT(uint netID)
        {
            lock (NetworkLock) {
                if (SpawnedNetObjects.TryGetValue(netID, out SpawnedNetObject netObj)) {
                    CleanNetObject(netObj);
                } else {
                    clearBuffer.Add(netID);
                }
            }
        }

        public bool AddSpawnable(string key, Type spawn)
        {
            if (spawnables.ContainsKey(key))
                throw new Exception("Attemped to add Type '" + spawn + "' with already existing key '" + key + "' to spawnables.");

            spawnables.Add(key, spawn);
            return true;
        }

        public static bool RemoveSpawnable(string key)
            => spawnables.Remove(key);

        internal Instantiator()
        {
            instantiateMethod = new RPCMethod(this, "Instantiate", DeliveryMethod.ReliableOrdered, 1, Scope.Unicast);
            destroyMethod = new RPCMethod(this, "Destroy", DeliveryMethod.ReliableOrdered, 1, Scope.Broadcast);
        }
    }
}
