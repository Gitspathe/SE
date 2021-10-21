using System.Collections.Generic;

namespace SE.Engine.Networking.Internal
{
    public class RPCLookupTable<T> where T : RPCInfo
    {
        public Dictionary<ushort, T> RPCTableInt = new Dictionary<ushort, T>();
        public Dictionary<string, T> RPCTableString = new Dictionary<string, T>();

        public void Add(T toAdd)
        {
            RPCTableInt.Add(toAdd.UshortID, toAdd);
            RPCTableString.Add(toAdd.StringID, toAdd);
        }

        public bool TryGetRPCInfo(ushort id, out T c)
        {
            c = null;
            if (RPCTableInt.TryGetValue(id, out T val)) {
                c = val;
                return true;
            }
            return false;
        }

        public bool TryGetRPCInfo(string id, out T c)
        {
            c = null;
            if (RPCTableString.TryGetValue(id, out T val)) {
                c = val;
                return true;
            }
            return false;
        }

        public T GetRPCInfo(ushort id)
            => RPCTableInt[id];

        public T GetRPCInfo(string id)
            => RPCTableString[id];

        public bool TryGetUshortID(string id, out ushort ushortID)
        {
            ushortID = 0;
            if (TryGetRPCInfo(id, out T c)) {
                ushortID = c.UshortID;
                return true;
            }
            return false;
        }

        public bool TryGetStringID(ushort id, out string stringID)
        {
            stringID = null;
            if (TryGetRPCInfo(id, out T c)) {
                stringID = c.StringID;
                return true;
            }
            return false;
        }
    }
}
