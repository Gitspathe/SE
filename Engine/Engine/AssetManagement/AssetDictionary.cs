using System.Collections.Generic;

namespace SE.AssetManagement
{
    /// <summary>
    /// Generic container dictionary for assets.
    /// </summary>
    /// <typeparam name="TValue">Value type.</typeparam>
    internal class AssetDictionary<TValue>
    {
        private Dictionary<string, Asset<TValue>> data;

        public Dictionary<string, TValue> GetData(IAssetConsumer consumer)
        {
            Dictionary<string, TValue> dict = new Dictionary<string, TValue>();
            foreach ((string key, Asset<TValue> value) in data) {
                dict.Add(key, value.Get(consumer));
            }
            return dict;
        }

        public void Add(string key, Asset<TValue> value, bool replace = true)
        {
            if (data.ContainsKey(key)) {
                if (replace) {
                    data.Remove(key);
                    data.Add(key, value);
                }
            } else {
                data.Add(key, value);
            }
        }

        public void Remove(string key)
        {
            data.Remove(key);
        }

        public void Clear()
        {
            foreach (KeyValuePair<string, Asset<TValue>> pair in data) {
                pair.Value.AsIAsset().Unload();
            }
            data.Clear();
        }

        public bool Contains(string key) => data.ContainsKey(key);

        public bool TryGet(IAssetConsumer consumer, string key, out TValue value)
        {
            value = default;
            if (data.TryGetValue(key, out Asset<TValue> asset)) {
                value = asset.Get(consumer);
                return true;
            }
            return false;
        }

        public TValue Get(IAssetConsumer consumer, string key) 
            => data.TryGetValue(key, out Asset<TValue> asset) ? asset.Get(consumer) : default;

        public Asset<TValue> GetAsset(string key) 
            => data.TryGetValue(key, out Asset<TValue> asset) ? asset : default;

        public AssetDictionary(int initialCapacity = 1)
        {
            data = new Dictionary<string, Asset<TValue>>(initialCapacity);
        }
    }
}
