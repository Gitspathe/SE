using SE.Common;
using SE.Components.Network;
using SE.Core;
using SE.Engine.Networking;
using SE.Utility;

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
        public void Setup(uint id, bool isOwner)
        {
            NetworkIdentity netID = Owner.Transform.Root.GameObject.NetIdentity;
            if (!(this is NetworkIdentity) && netID == null) {
                Console.LogError(new SEException("Attempted to setup a NetComponent on a GameObject which does not have a NetIdentity."));
                return;
            }

            ID = id;
            IsOwner = isOwner;
            netID.NetIDs.Add(id);
            IsSetup = true;
        }
    }
}
