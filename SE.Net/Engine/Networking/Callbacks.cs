using System;
using LiteNetLib;

namespace SE.Engine.Networking
{
    public delegate void OnServerStartedEventHandler();
    public delegate void OnPeerConnectedEventHandler(NetPeer peer);
    public delegate void OnPeerDisconnectedEventHandler(NetPeer peer, DisconnectInfo disconnectInfo);
    public delegate void OnLogInfo(string msg, bool important = false);
    public delegate void OnLogWarning(string msg = null, Exception exception = null);
    public delegate void OnLogError(string msg = null, Exception exception = null);
}
