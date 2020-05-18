using System;
using System.Collections.Generic;
using System.Text;
using LiteNetLib;

namespace SE.Engine
{
    public interface IPacketProcessor
    {
        void OnReceive(NetPacketReader reader, NetPeer peer, DeliveryMethod deliveryMethod);
        // TODO void OnSend();
    }
}
