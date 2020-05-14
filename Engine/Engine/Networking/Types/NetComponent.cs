using SE.Common;
using SE.Components.Network;
using SE.Core;
using SE.Engine.Networking;

namespace SE.Networking.Types
{
    /// <summary>
    /// Containers for logic which are added to GameObjects.
    /// </summary>
    public class NetComponent : Component, INetLogic
    {
        public uint ID { get; protected set; }
        public bool IsSetup { get; protected set; }
        public bool IsOwner { get; protected set; }

        /// <inheritdoc />
        public void Setup(uint id, bool isOwner, string netState = null)
        {
            NetworkIdentity netID = Owner.Transform.Root.GameObject.NetIdentity;
            if (!(this is NetworkIdentity) && netID == null) {
                Console.LogError(new System.Exception("Attempted to setup a NetComponent on a GameObject which does not have a NetIdentity."));
                return;
            }

            ID = id;
            IsOwner = isOwner;
            if (this is INetPersistable netPersist && !string.IsNullOrEmpty(netState)) {
                netPersist.RestoreNetworkState(netState);
            }
            netID.NetIDs.Add(id);
            IsSetup = true;
        }
    }
}
