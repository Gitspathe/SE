using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement;
using SE.Core.Exceptions;
using System;
using System.Collections.Generic;

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

        private static HashSet<Type> noHeadlessSupportTypes = new HashSet<Type> {
            typeof(Texture2D),
            typeof(SpriteFont)
        };

        public static void Update(float deltaTime)
        {
            FileMarshal.Update();
            foreach (var pair in contentManagers) {
                pair.Value.Update(deltaTime);
            }
        }

        internal static ContentLoader GetLoader(string id)
        {
            if (contentManagers.TryGetValue(id, out ContentLoader val)) {
                return val;
            }
            return null;
        }

        internal static void AddContentManager(ContentLoader wrapper)
        {
            if (contentManagers.ContainsKey(wrapper.ID)) {
                contentManagers.Remove(wrapper.ID);
            }
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
                    throw new HeadlessNotSupportedException("Cannot add asset of type '" + type + "' while in fully headless display mode.", e);
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
        /// <typeparam name="TValue">Asset type.</typeparam>
        /// <param name="consumer">Consumer reference for the access request.</param>
        /// <param name="key">Key used for retrieval.</param>
        /// <param name="value">Array resource returned.</param>
        /// <returns>True if successful.</returns>
        public static bool TryGet<TValue>(IAssetConsumer consumer, string key, out dynamic value)
        {
            if (Screen.IsFullHeadless && noHeadlessSupportTypes.Contains(typeof(TValue))) {
                Console.LogWarning($"Asset with key '{key}' was not retrieved in headless mode.");
                value = default;
                return false;
            }
            return LookupDictionary[typeof(Asset<TValue>)].TryGet(consumer, key, out value);
        }

        /// <summary>
        /// Retrieves an unpacked asset instance.
        /// </summary>
        /// <typeparam name="TValue">Asset type.</typeparam>
        /// <param name="consumer">Consumer reference for the access request.</param>
        /// <param name="key">Key used for retrieval.</param>
        /// <returns>An unpacked asset.</returns>
        public static TValue Get<TValue>(IAssetConsumer consumer, string key)
        {
            if (Screen.IsFullHeadless && noHeadlessSupportTypes.Contains(typeof(TValue))) {
                Console.LogWarning($"Asset with key '{key}' was not retrieved in headless mode.");
                return default;
            }
            if (LookupDictionary.ContainsKey(typeof(Asset<TValue>))) {
                return LookupDictionary[typeof(Asset<TValue>)].Get(consumer, key);
            }
            throw new KeyNotFoundException("No asset for the specified key was found.");
        }

        /// <summary>
        /// Gets a packed Asset instance.
        /// </summary>
        /// <typeparam name="TValue">Asset type.</typeparam>
        /// <param name="key">Key used for retrieval.</param>
        /// <returns>Packed Asset instance.</returns>
        public static Asset<TValue> GetAsset<TValue>(string key)
        {
            if (Screen.IsFullHeadless && noHeadlessSupportTypes.Contains(typeof(TValue))) {
                Console.LogWarning($"Asset with key '{key}' was not retrieved in headless mode.");
                return default;
            }
            if (LookupDictionary.ContainsKey(typeof(Asset<TValue>))) {
                return LookupDictionary[typeof(Asset<TValue>)].GetAsset(key);
            }
            throw new KeyNotFoundException("No asset for the specified key was found.");
        }

        public static Dictionary<string, TValue> GetDictionary<TValue>(IAssetConsumer consumer)
        {
            if (LookupDictionary.ContainsKey(typeof(Asset<TValue>))) {
                return LookupDictionary[(dynamic)typeof(Asset<TValue>)].GetData(consumer);
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
