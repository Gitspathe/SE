using System;
using System.Collections.Generic;
using SE.AssetManagement;
using SE.Core.Exceptions;

namespace SE.Core
{
    /// <summary>
    /// Handles asset management. Responsible for loading and unloading game resources during runtime.
    /// </summary>
    public static class AssetManager
    {
        // Dynamic is of type of AssetDictionary<T>.
        internal static Dictionary<Type, dynamic> LookupDictionary = new Dictionary<Type, dynamic>();
        
        //Increments for each new Asset instance.
        internal static uint CurrentAssetPriority;

        private static Dictionary<string, ContentLoader> contentManagers = new Dictionary<string, ContentLoader>();

        internal static ContentLoader GetLoader(string id)
        {
            if (contentManagers.TryGetValue(id, out ContentLoader val)) {
                return val;
            }
            return null;
        }

        internal static void AddContentManager(ContentLoader wrapper)
        {
            contentManagers.Add(wrapper.ID, wrapper);
        }

        public static Asset<TValue> Add<TValue>(Asset<TValue> value, bool replace = true)
        {
            Type type = value.GetType();
            try {
                if (LookupDictionary.ContainsKey(type)) {
                    LookupDictionary[type].Add(value.ID, value, replace);
                } else {
                    AssetDictionary<TValue> assetDict = new AssetDictionary<TValue>();
                    assetDict.Add(value.ID, value, replace);
                    LookupDictionary.Add(type, assetDict);
                }
            } catch (ArgumentNullException e) {
                if (Screen.IsFullHeadless) {
                    throw new HeadlessNotSupportedException("Cannot add asset of type '"+type+"' while in fully headless display mode.", e);
                }
            }
            return value;
        }

        public static Asset<TValue> Add<TValue>(AssetBuilder<TValue> value, bool replace = true)
        {
            Asset<TValue> asset = value.Finish();
            Add(asset, replace);
            return asset;
        }

        /// <summary>
        /// Attempts to return an unpacked asset instance.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TValue">Asset type.</typeparam>
        /// <param name="consumer">Consumer reference for the access request.</param>
        /// <param name="key">Key used for retrieval.</param>
        /// <param name="value">Array resource returned.</param>
        /// <returns>True if successful.</returns>
        public static bool TryGet<TValue>(IAssetConsumer consumer, string key, out dynamic value) 
            => LookupDictionary[typeof(Asset<TValue>)].TryGet(consumer, key, out value);

        /// <summary>
        /// Retrieves an unpacked asset instance.
        /// </summary>
        /// <typeparam name="TValue">Asset type.</typeparam>
        /// <param name="consumer">Consumer reference for the access request.</param>
        /// <param name="key">Key used for retrieval.</param>
        /// <returns>An unpacked asset.</returns>
        public static TValue Get<TValue>(IAssetConsumer consumer, dynamic key)
        {
            if (LookupDictionary.ContainsKey(typeof(Asset<TValue>))) {
                return LookupDictionary[typeof(Asset<TValue>)].Get(consumer, key);
            }
            return default;
        }

        /// <summary>
        /// Gets an asset in as IAsset.
        /// </summary>
        /// <typeparam name="TValue">Asset type.</typeparam>
        /// <param name="key">Key used for retrieval</param>
        /// <returns>IAsset instance.</returns>
        public static IAsset GetIAsset<TValue>(dynamic key)
        {
            Asset<TValue> asset = GetAsset<TValue>(key);
            return asset?.AsIAsset();
        }

        /// <summary>
        /// Gets a packed Asset instance.
        /// </summary>
        /// <typeparam name="TValue">Asset type.</typeparam>
        /// <param name="key">Key used for retrieval.</param>
        /// <returns>Packed Asset instance.</returns>
        public static Asset<TValue> GetAsset<TValue>(dynamic key)
        {
            if (LookupDictionary.ContainsKey(typeof(Asset<TValue>))) {
                return LookupDictionary[typeof(Asset<TValue>)].GetAsset(key);
            }
            return default;
        }

        public static Dictionary<string, TValue> GetDictionary<TValue>(IAssetConsumer consumer)
        {
            if (LookupDictionary.ContainsKey(typeof(Asset<TValue>))) {
                return LookupDictionary[(dynamic) typeof(Asset<TValue>)].GetData(consumer);
            }
            return null;
        }

        public static void Unload()
        {
            foreach (KeyValuePair<string, ContentLoader> pair in contentManagers) {
                pair.Value.Unload();
                pair.Value.Dispose();
            }
            contentManagers.Clear();
            foreach (KeyValuePair<Type, dynamic> pair in LookupDictionary) {
                pair.Value.Clear();
            }
            LookupDictionary.Clear();
        }
    }
}
