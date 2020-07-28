using System;
using System.Collections.Generic;
using System.Numerics;
using LiteNetLib.Utils;
using SE.Engine.Networking;
using SE.Networking.Internal;
using SE.Networking.Types;
using static SE.Core.Network;

namespace SE.Components.Network
{
    public class NetworkIdentity : NetComponent, INetInstantiatable, INetPersistable
    {
        public override int Queue => int.MinValue;

        public Func<byte[]> OnSerializeNetworkState;

        public Action<NetDataReader> OnRestoreNetworkState;

        public Action<bool> OnSetup;

        internal SpawnedGameObject SpawnedGameObject;
        internal List<uint> NetIDs = new List<uint>();

        private List<byte[]> netStates = new List<byte[]>();
        private List<uint> netIDs = new List<uint>();

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Owner.NetIdentity = this;
        }

        public void RestoreNetworkState(NetDataReader reader)
        {
            OnRestoreNetworkState?.Invoke(reader);
        }

        public byte[] SerializeNetworkState(NetDataWriter writer)
        {
            return OnSerializeNetworkState?.Invoke() ?? new byte[0];
        }

        /// <summary>
        /// Called when the GameObject has it's network ID and owner values filled in by the server.
        /// Any network logic relating to the 
        /// </summary>
        public new virtual void Setup(uint id, bool isOwner)
        {
            ID = id;
            IsOwner = isOwner;
            NetIDs.Add(id);
            IsSetup = true;
            OnSetup?.Invoke(isOwner);
        }

        public void OnNetworkInstantiatedServer(string type, string owner, NetDataWriter writer)
        {
            netStates.Clear();
            netIDs.Clear();

            netStates.Add(SerializeNetworkState(writer));
            netIDs.Add(ID);

            foreach (NetComponent nComponent in Owner.GetAllComponents<NetComponent>()) {
                // Skip NetworkIdentity component since it's already processed.
                if (nComponent is NetworkIdentity)
                    continue;

                nComponent.Setup(CurrentNetworkID, owner == "SERVER");
                NetworkObjects.TryAdd(CurrentNetworkID, nComponent);
                CurrentNetworkID++;
                if (nComponent is INetPersistable netPersist) {
                    netStates.Add(NetLogicHelper.SerializePersistable(netPersist));
                } else {
                    netStates.Add(null); // Padding for deserialization.
                }

                netIDs.Add(nComponent.ID);
            }
        }

        public void OnNetworkInstantiatedClient(string type, bool isOwner, NetDataReader reader)
        {
            InstantiateData buffer = new InstantiateData(reader);
            List<byte[]> netStates = buffer.NetStates;
            uint[] netIDs = buffer.NetIDs;

            // Handle any NetComponents on the GameObject.
            int index = 1;
            foreach (NetComponent nComponent in Owner.GetAllComponents<NetComponent>()) {
                // Skip NetworkIdentity component since it's already processed.
                if (nComponent is NetworkIdentity)
                    continue;

                nComponent.Setup(netIDs[index], isOwner);
                if (nComponent is INetPersistable persist && netStates[index] != null)
                    NetLogicHelper.RestorePersistable(persist, netStates[index]);

                NetworkObjects.TryAdd(netIDs[index], nComponent);
                index++;
            }
        }

        public byte[] GetBufferedData(NetDataWriter writer)
        {
            netStates.Clear();
            netIDs.Clear();

            netStates.Add(SerializeNetworkState(writer));
            netIDs.Add(ID);
            foreach (NetComponent nComponent in Owner.GetAllComponents<NetComponent>()) {
                if (nComponent is INetPersistable netPersist) {
                    netStates.Add(NetLogicHelper.SerializePersistable(netPersist));
                } else {
                    netStates.Add(null); // Padding for deserialization.
                }
                netIDs.Add(nComponent.ID);
            }

            InstantiateData buffer = new InstantiateData(netStates, netIDs.ToArray());
            return buffer.Serialize(new NetDataWriter());
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
            public List<byte[]> NetStates;
            public uint[] NetIDs;

            public InstantiateData(List<byte[]> netStates, uint[] netIDs)
            {
                NetStates = netStates;
                NetIDs = netIDs;
            }

            public InstantiateData(NetDataReader reader)
            {
                uint len = reader.GetUInt();
                NetStates = new List<byte[]>();
                NetIDs = new uint[len];
                for (int i = 0; i < len; i++) {
                    NetStates.Add(reader.GetBytesWithLength());
                    NetIDs[i] = reader.GetUInt();
                }
            }

            public byte[] Serialize(NetDataWriter writer)
            {
                writer.Put((uint) NetStates.Count);
                for (int i = 0; i < NetStates.Count; i++) {
                    writer.PutBytesWithLength(NetStates[i]);
                    writer.Put(NetIDs[i]);
                }
                return writer.CopyData();
            }
        }
    }
}
