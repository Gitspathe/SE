using System;
using System.Collections.Generic;
using System.Text;
using LiteNetLib;

namespace DeeZ.Engine.Networking
{
    public interface INetworkExtension : INetLogic
    {
        void Update();
        void OnPeerConnected(NetPeer peer);
    }
}
