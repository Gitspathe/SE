using System.Collections.Generic;
using System.Reflection;

namespace DeeZ.Engine.Networking.Internal
{
    internal class RPCCache
    {
        public MethodInfo MethodInfo;
        public Dictionary<object, LateBoundMethod> LateBoundMethods = new Dictionary<object, LateBoundMethod>();

        public RPCCache(MethodInfo info)
        {
            MethodInfo = info;
        }
    }
}
