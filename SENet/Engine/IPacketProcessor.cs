using System;
using System.Collections.Generic;
using System.Text;
using LiteNetLib;
using LiteNetLib.Utils;

namespace SE.Engine
{
    public interface IPacketProcessor
    {
        void OnReceive(NetDataReader reader, NetPeer peer, DeliveryMethod deliveryMethod);
    }
}
