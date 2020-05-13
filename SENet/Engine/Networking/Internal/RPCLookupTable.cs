using System.Collections.Generic;
using System.Reflection;
using DeeZ.Core.Exceptions;
using DeeZ.Engine.Utility;

namespace DeeZ.Engine.Networking.Internal
{
    internal class RPCLookupTable<T> where T : RPCInfo
    {
        public QuickList<KeyValuePair<ushort, T>> RPCTableInt = new QuickList<KeyValuePair<ushort, T>>();
        public QuickList<KeyValuePair<string, T>> RPCTableString = new QuickList<KeyValuePair<string, T>>();

        public void Add(T toAdd)
        {
            RPCTableInt.Add(new KeyValuePair<ushort, T>(toAdd.UshortID, toAdd));
            RPCTableString.Add(new KeyValuePair<string, T>(toAdd.StringID, toAdd));
        }

        public bool TryGetRPCInfo(ushort id, out T c)
        {
            c = null;
            KeyValuePair<ushort, T>[] array = RPCTableInt.Array;
            for (int i = 0; i < RPCTableInt.Count; i++) {
                if (array[i].Key == id) {
                    c = array[i].Value;
                    return true;
                }
            }
            return false;
        }

        public bool TryGetRPCInfo(string id, out T c)
        {
            c = null;
            KeyValuePair<string, T>[] array = RPCTableString.Array;
            for (int i = 0; i < RPCTableString.Count; i++) {
                if (array[i].Key == id) {
                    c = array[i].Value;
                    return true;
                }
            }
            return false;
        }

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
