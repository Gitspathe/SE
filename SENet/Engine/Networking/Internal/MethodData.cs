using System.Reflection;

namespace DeeZ.Engine.Networking.Internal
{
    internal class MethodData
    {
        public string ID;
        public MethodInfo Info;

        public MethodData(string id, MethodInfo info)
        {
            ID = id;
            Info = info;
        }
    }
}
