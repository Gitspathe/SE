using LiteNetLib;

namespace SE.Engine.Networking
{
    public class RPCMethod
    {
        internal INetLogic NetLogic;
        internal DeliveryMethod DeliveryMethod;
        internal byte Channel;
        internal string Method;
        internal Scope Scope;

        public RPCMethod(INetLogic netLogic, string method, DeliveryMethod deliveryMethod = DeliveryMethod.Unreliable, byte channel = 0, Scope scope = Scope.Broadcast)
        {
            NetLogic = netLogic;
            DeliveryMethod = deliveryMethod;
            Channel = channel;
            Method = method;
            Scope = scope;
        }
    }
}
