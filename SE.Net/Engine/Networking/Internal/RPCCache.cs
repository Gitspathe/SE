using System.Collections.Generic;
using System.Reflection;

namespace SE.Engine.Networking.Internal
{
    public class RPCCache
    {
        public MethodInfo MethodInfo;
        public Dictionary<object, LateBoundMethod> LateBoundMethods = new Dictionary<object, LateBoundMethod>();

        public RPCCache(MethodInfo info)
        {
            MethodInfo = info;
        }
    }
}
