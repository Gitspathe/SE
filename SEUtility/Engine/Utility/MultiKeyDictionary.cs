using System.Collections.Generic;

namespace SE.Utility
{
    public class MultiKeyDictionary<TKey1, TKey2, TValue>
        where TValue : class
    {
        private Dictionary<TKey1, TValue> dictionary1 = new Dictionary<TKey1, TValue>();
        private Dictionary<TKey2, TValue> dictionary2 = new Dictionary<TKey2, TValue>();

        public bool ContainsKey(TKey1 key1) 
            => dictionary1.ContainsKey(key1);

        public bool ContainsKey(TKey2 key2) 
            => dictionary2.ContainsKey(key2);

        public bool TryGetValue(TKey1 key1, out TValue value) 
            => dictionary1.TryGetValue(key1, out value);

        public bool TryGetValue(TKey2 key2, out TValue value)
            => dictionary2.TryGetValue(key2, out value);

        public void Add(TKey1 key1, TKey2 key2, TValue value)
        {
            dictionary1.Add(key1, value);
            dictionary2.Add(key2, value);
        }

        public bool Remove(TKey1 key) 
            => Remove(key, out _, out _);

        public bool Remove(TKey2 key)
            => Remove(key, out _, out _);

        public bool Remove(TKey1 key, out TKey2 key2, out TValue value)
        {
            key2 = default;
            value = null;
            if (!dictionary1.Remove(key))
                return false;

            foreach (KeyValuePair<TKey2, TValue> pair in dictionary2) {
                TKey2 theKey = pair.Key;
                TValue value1 = pair.Value;
                if (value1 == value) {
                    dictionary2.Remove(theKey);
                    key2 = theKey;
                    return true;
                }
            }
            return false;
        }

        public bool Remove(TKey2 key, out TKey1 key1, out TValue value)
        {
            key1 = default;
            value = null;
            if (!dictionary2.Remove(key))
                return false;

            foreach (KeyValuePair<TKey1, TValue> pair in dictionary1) {
                TKey1 theKey = pair.Key;
                TValue value1 = pair.Value;
                if (value1 == value) {
                    dictionary1.Remove(theKey);
                    key1 = theKey;
                    return true;
                }
            }
            return false;
        }

        public bool Remove(TValue value)
        {
            foreach (KeyValuePair<TKey1, TValue> pair in dictionary1) {
                TKey1 theKey = pair.Key;
                TValue value1 = pair.Value;
                if (value1 == value) {
                    return Remove(theKey);
                }
            }
            return false;
        }

    }
}
