using System;
using SE.Engine.Networking.Attributes;

namespace SE.Engine.Networking.Internal
{
    public class RPCInfo
    {
        public RPCCache Cache;
        public ushort UshortID;
        public string StringID;
        public bool FrequentMethod;
        public Type[] ParameterTypes;

        public void Invoke(object target, object[] parameters)
        {
            if (FrequentMethod) {
                if (Cache.LateBoundMethods.TryGetValue(target, out LateBoundMethod lbm)) {
                    lbm.Invoke(target, parameters);
                    return;
                }
                lbm = DelegateFactory.Create(Cache.MethodInfo, target);
                Cache.LateBoundMethods.Add(target, lbm);
                lbm.Invoke(target, parameters);
            } else {
                Cache.MethodInfo.Invoke(target, parameters);
            }
        }

        public RPCInfo(RPCCache cache, ushort ushortID, string stringID, bool frequentMethod, Type[] paramTypes)
        {
            Cache = cache;
            UshortID = ushortID;
            StringID = stringID;
            FrequentMethod = frequentMethod;
            ParameterTypes = paramTypes;
        }
    }

    public class RPCClientInfo : RPCInfo
    {
        public RPCClientInfoOptions Options;

        public RPCClientInfo(ClientRPCAttribute attribute, RPCCache cache, ushort ushortID, string stringID, Type[] paramTypes)
            : base(cache, ushortID, stringID, attribute.Frequent, paramTypes)
        {
            Options = new RPCClientInfoOptions();
        }

        public class RPCClientInfoOptions
        {
            public RPCClientInfoOptions() { }
        }
    }

    public class RPCServerInfo : RPCInfo
    {
        public RPCServerInfoOptions Options;

        public RPCServerInfo(ServerRPCAttribute attribute, RPCCache cache, ushort ushortID, string stringID, Type[] paramTypes)
            : base(cache, ushortID, stringID, attribute.Frequent, paramTypes)
        {
            Options = new RPCServerInfoOptions(attribute.CallClientRPC);
        }

        public class RPCServerInfoOptions
        {
            public bool CallClientRPC;

            public RPCServerInfoOptions(bool callClientRPC)
            {
                CallClientRPC = callClientRPC;
            }
        }
    }
}
