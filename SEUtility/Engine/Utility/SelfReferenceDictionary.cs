using System;
using System.Collections.Generic;
using System.Text;

namespace DeeZ.Engine.Utility
{
    /// <summary>
    /// Dictionary which internally uses two regular dictionaries to provide a self-reference between each
    /// key and it's value. A value can be used to find a key, and vice versa.
    /// </summary>
    /// <typeparam name="TKey">Dictionary key.</typeparam>
    /// <typeparam name="TValue">Dictionary value.</typeparam>
    public class SelfReferenceDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> dictionary1 = new Dictionary<TKey, TValue>();
        private Dictionary<TValue, TKey> dictionary2 = new Dictionary<TValue, TKey>();

        public bool ContainsKey(TKey key)
            => dictionary1.ContainsKey(key);

        public bool ContainsValue(TValue value)
            => dictionary2.ContainsKey(value);

        public bool TryGetValue(TKey key, out TValue value)
            => dictionary1.TryGetValue(key, out value);

        public bool TryGetKey(TValue value, out TKey key)
            => dictionary2.TryGetValue(value, out key);

        public void Add(TKey key, TValue value)
        {
            dictionary1.Add(key, value);
            dictionary2.Add(value, key);
        }

        public bool Remove(TKey key)
            => Remove(key, out _);

        public bool Remove(TValue value)
            => Remove(value, out _);

        public bool Remove(TKey key, out TValue value)
        {
            value = default;
            if (dictionary1.TryGetValue(key, out TValue foundValue)) {
                dictionary1.Remove(key);
                value = foundValue;
                return dictionary2.Remove(foundValue);
            }
            return false;
        }

        public bool Remove(TValue value, out TKey key)
        {
            key = default;
            if (dictionary2.TryGetValue(value, out TKey foundKey)) {
                dictionary2.Remove(value);
                key = foundKey;
                return dictionary1.Remove(foundKey);
            }
            return false;
        }
    }
}
