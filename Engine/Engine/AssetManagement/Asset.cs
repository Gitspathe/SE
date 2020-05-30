using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SE.Common;
using SE.Core;
using SE.Rendering;

namespace SE.AssetManagement
{
    public class Asset<T> : SEObject, IAsset, IAssetConsumer
    {
        public ulong AssetID { get; internal set; }

        /// <value>Returns the inner resource of the asset.</value>
        /// <exception cref="NullReferenceException">Thrown when the asset isn't loaded into memory.</exception>
        internal T Value {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                if (!isLoaded)
                    Load();

                return value;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => this.value = value;
        }
        private T value;

        public string ID { get; internal set; }

        public ContentLoader ContentLoader { get; }

        public uint LoadOrder { get; set; }
        public bool Loaded { 
            get => isLoaded;
            set => isLoaded = value;
        }
        private bool isLoaded; // Optimization.

        public AssetConsumer AssetConsumer { get; }
        public HashSet<AssetConsumer> References { get; set; } = new HashSet<AssetConsumer>();

        private IAssetProcessor processor;

        internal T Get(IAssetConsumer consumer)
        {
            if (consumer == null)
                throw new NullReferenceException("The IAssetConsumer instance was null.");
            if(consumer == this)
                throw new InvalidOperationException("Attempted to retrieve self.");

            AddReference(consumer.AssetConsumer);
            return GetNoRef();
        }

        internal T GetNoRef()
        {
            Load();
            return Value;
        }

        public void RemoveReference(AssetConsumer consumer)
        {
            if (consumer == AssetConsumer)
                throw new NullReferenceException("The IAssetConsumer instance was null.");

            References.Remove(consumer);
            consumer.RemoveReference(this);
            if (References.Count < 1) {
                Unload();
            }
        }

        public void AddReference(AssetConsumer consumer)
        {
            if (consumer == null)
                throw new NullReferenceException("The IAssetConsumer instance was null.");
            if(consumer == AssetConsumer)
                return;

            References.Add(consumer);
            consumer.AddReference(this);
            if (References.Count > 0) {
                Load();
            }
        }

        public void Load()
        {
            if(isLoaded)
                return;

            value = (T) processor.Construct();
            AssetConsumer.ReferenceAssets();
            Loaded = true;
            ContentLoader.AddReference(this);
        }

        public void Unload()
        {
            if(!isLoaded)
                return;

            DrawCallDatabase.PruneAsset(Value);
            Loaded = false;
            AssetConsumer.DereferenceAssets();
            ContentLoader.RemoveReference(this);
            Value = default;
        }

        public void Purge()
        {
            if (References.Count < 1) {
                Unload();
            }
        }

        public IAsset AsIAsset() => this;

        /// <summary>
        /// Creates a new asset.
        /// </summary>
        /// <param name="processor">Function called whenever the asset is reloaded.</param>
        /// <param name="contentLoader">Content loader the asset will be added to.</param>
        /// <param name="referencedAssets">Asset dependencies.</param>
        public Asset(string id, IAssetProcessor processor, ContentLoader contentLoader, HashSet<IAsset> referencedAssets = null)
        {
            ID = id;
            LoadOrder = AssetManager.CurrentAssetPriority++;

            AssetConsumer = new AssetConsumer();
            ContentLoader = contentLoader;
            this.processor = processor;
            if (referencedAssets == null) 
                return;

            foreach (IAsset refAsset in referencedAssets) {
                refAsset?.AddReference(AssetConsumer);
            }
        }

        /// <summary>
        /// Creates a new asset.
        /// </summary>
        /// <param name="processor">Asset processor used to construct the asset instance.</param>
        /// <param name="contentLoader">Content loader the asset will be added to.</param>
        /// <param name="referencedAsset">Asset dependency.</param>
        public Asset(string id, IAssetProcessor processor, ContentLoader contentLoader, IAsset referencedAsset = null) 
            : this(id, processor, contentLoader, new HashSet<IAsset> { referencedAsset }) { }

        /// <summary>
        /// Creates a new asset.
        /// </summary>
        /// <param name="processor">Asset processor used to construct the asset instance.</param>
        /// <param name="contentLoader">Content loader the asset will be added to.</param>
        public Asset(string id, IAssetProcessor processor, ContentLoader contentLoader) 
            : this(id, processor, contentLoader, new HashSet<IAsset>()) { }
    }

    public interface IAsset
    {
        uint LoadOrder { get; set; } 
        bool Loaded { get; set; }
        HashSet<AssetConsumer> References { get; set; }

        void RemoveReference(AssetConsumer reference);
        void AddReference(AssetConsumer reference);
        void Load();
        void Unload();
        void Purge();
    }
}
