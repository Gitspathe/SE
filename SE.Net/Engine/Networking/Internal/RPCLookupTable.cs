using System.Collections.Generic;
using SE.Core.Exceptions;

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

        public ushort? GetUshortID(string id)
        {
            if (TryGetRPCInfo(id, out T c)) {
                return c.UshortID;
            }
            throw new InvalidRPCException("RPC with string ID '"+id+"' not found.");
        }

        public string GetStringID(ushort id)
        {
            if (TryGetRPCInfo(id, out T c)) {
                return c.StringID;
            }
            throw new InvalidRPCException("RPC with ushort ID '" + id + "' not found.");
        }
    }
}
