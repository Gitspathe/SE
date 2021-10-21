using LiteNetLib;

namespace SE.Engine.Networking
{
    public interface INetworkExtension : INetLogic
    {
        void Update();
        void OnPeerConnected(NetPeer peer);
    }
}
