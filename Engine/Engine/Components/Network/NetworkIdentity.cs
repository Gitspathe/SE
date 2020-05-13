using System;
using System.Collections.Generic;
using System.Numerics;
using DeeZ.Engine.Networking;
using LiteNetLib.Utils;
using SE.Networking.Internal;
using SE.Networking.Types;
using static DeeZ.Core.Network;

namespace SE.Components.Network
{
    public class NetworkIdentity : NetComponent, INetInstantiatable, INetPersistable
    {
        public override int Queue => int.MinValue;

        public Func<string> OnSerializeNetworkState;

        public Action<string> OnRestoreNetworkState;

        public Action<bool> OnSetup;

        internal SpawnedGameObject SpawnedGameObject;
        internal List<uint> NetIDs = new List<uint>();

        private List<string> netStates = new List<string>();
        private List<uint> netIDs = new List<uint>();

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Owner.NetIdentity = this;
        }

        public void RestoreNetworkState(string jsonString)
        {
            OnRestoreNetworkState?.Invoke(jsonString);
        }

        public string SerializeNetworkState()
        {
            return OnSerializeNetworkState?.Invoke();
        }

        /// <summary>
        /// Called when the GameObject has it's network ID and owner values filled in by the server.
        /// Any network logic relating to the 
        /// </summary>
        public new virtual void Setup(uint id, bool isOwner, string netState = null)
        {
            ID = id;
            IsOwner = isOwner;
            if (!string.IsNullOrEmpty(netState)) {
                RestoreNetworkState(netState);
            }
            NetIDs.Add(id);
            IsSetup = true;
            OnSetup?.Invoke(isOwner);
        }

        public void OnNetworkInstantiatedServer(string type, string owner)
        {
            netStates.Clear();
            netIDs.Clear();

            netStates.Add(SerializeNetworkState());
            netIDs.Add(ID);

            foreach (NetComponent nComponent in Owner.GetAllComponents<NetComponent>()) {
                // Skip NetworkIdentity component since it's already processed.
                if (nComponent is NetworkIdentity)
                    continue;

                nComponent.Setup(CurrentNetworkID, owner == "SERVER");
                NetworkObjects.Add(CurrentNetworkID, nComponent);
                CurrentNetworkID++;
                if (nComponent is INetPersistable netPersist) {
                    netStates.Add(netPersist.SerializeNetworkState());
                } else {
                    netStates.Add(null); // Padding for deserialization.
                }

                netIDs.Add(nComponent.ID);
            }
        }

        public void OnNetworkInstantiatedClient(string type, bool isOwner, byte[] data)
        {
            InstantiateData buffer = new InstantiateData(data);
            string[] netStates = buffer.NetStates;
            uint[] netIDs = buffer.NetIDs;

            // Handle any NetComponents on the GameObject.
            int index = 1;
            foreach (NetComponent nComponent in Owner.GetAllComponents<NetComponent>()) {
                // Skip NetworkIdentity component since it's already processed.
                if (nComponent is NetworkIdentity)
                    continue;

                nComponent.Setup(netIDs[index], isOwner, netStates[index]);
                NetworkObjects.Add(netIDs[index], nComponent);
                index++;
            }
        }

        private static NetDataWriter writer = new NetDataWriter();

        public byte[] GetBufferedData()
        {
            netStates.Clear();
            netIDs.Clear();

            netStates.Add(SerializeNetworkState());
            netIDs.Add(ID);
            foreach (NetComponent nComponent in Owner.GetAllComponents<NetComponent>()) {
                if (nComponent is INetPersistable netPersist) {
                    netStates.Add(netPersist.SerializeNetworkState());
                } else {
                    netStates.Add(null); // Padding for deserialization.
                }
                netIDs.Add(nComponent.ID);
            }

            InstantiateData buffer = new InstantiateData(netStates.ToArray(), netIDs.ToArray());
            return buffer.Serialize(GetWriter());
        }

        public void NetClean()
        {
            List<uint> goNetIDs = NetIDs;
            for (int i = 0; i < goNetIDs.Count; i++) {
                NetworkObjects.Remove(goNetIDs[i]);
                SpawnedNetObjects.Remove(goNetIDs[i]);
            }
            Owner.OnDestroyInternal();
        }

        public object[] InstantiateParameters
            => new object[] { Owner.Transform.GlobalPositionInternal, 0f, Vector2.One };

        public struct InstantiateData : IInstantiateSerializer
        {
            public string[] NetStates;
            public uint[] NetIDs;

            public InstantiateData(string[] netStates, uint[] netIDs)
            {
                NetStates = netStates;
                NetIDs = netIDs;
            }

            public InstantiateData(byte[] bytes)
            {
                NetDataReader reader = new NetDataReader(bytes);
                uint len = reader.GetUInt();
                NetStates = new string[len];
                NetIDs = new uint[len];
                for (int i = 0; i < len; i++) {
                    NetStates[i] = reader.GetString();
                    NetIDs[i] = reader.GetUInt();
                }
            }

            public byte[] Serialize(NetDataWriter writer)
            {
                writer.Put((uint) NetStates.Length);
                for (int i = 0; i < NetStates.Length; i++) {
                    writer.Put(NetStates[i]);
                    writer.Put(NetIDs[i]);
                }
                return writer.CopyData();
            }
        }
    }
}
