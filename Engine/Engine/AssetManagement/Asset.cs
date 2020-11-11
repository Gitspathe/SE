using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SE.AssetManagement.Processors;
using SE.Common;
using SE.Core;
using SE.Rendering;

namespace SE.AssetManagement
{
    public abstract class Asset : SEObject, IAssetConsumer
    {
        public ulong AssetID { get; internal set; }
        public uint LoadOrder { get; internal set; } 
        public bool Loaded { get; internal set; }
        internal HashSet<AssetConsumer> References { get; set; } = new HashSet<AssetConsumer>();
        public AssetConsumer AssetConsumer { get; }

        internal abstract void RemoveReference(AssetConsumer reference);
        internal abstract void AddReference(AssetConsumer reference);
        internal abstract void Load();
        internal abstract void Unload();
        internal abstract void Purge();

        public Asset()
        {
            AssetConsumer = new AssetConsumer();
        }
    }

    public class Asset<T> : Asset
    {
        public string ID { get; internal set; }
        public ContentLoader ContentLoader { get; }

        private IAssetProcessor processor;

        /// <value>Returns the inner resource of the asset.</value>
        internal T Value {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                if (!Loaded)
                    Load();

                return value;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => this.value = value;
        }
        private T value;

        /// <summary>
        /// Creates a new asset.
        /// </summary>
        /// <param name="processor">Function called whenever the asset is reloaded.</param>
        /// <param name="contentLoader">Content loader the asset will be added to.</param>
        internal Asset(string id, AssetProcessor processor, ContentLoader contentLoader)
        {
            ID = id;
            LoadOrder = AssetManager.CurrentAssetPriority++;

            ContentLoader = contentLoader;
            this.processor = processor;
            HashSet<Asset> refAssets = processor.GetReferencedAssets();
            if(refAssets == null)
                return;

            foreach (Asset refAsset in processor.GetReferencedAssets()) {
                refAsset?.AddReference(AssetConsumer);
            }
        }

        internal T Get(IAssetConsumer consumer)
        {
            if (consumer == null)
                throw new NullReferenceException("The IAssetConsumer instance was null.");
            if(consumer == this)
                throw new InvalidOperationException("Attempted to retrieve self.");

            AddReference(consumer.AssetConsumer);
            return GetNoRef();
        }

        private T GetNoRef()
        {
            Load();
            return Value;
        }

        internal override void AddReference(AssetConsumer consumer)
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

        internal override void RemoveReference(AssetConsumer consumer)
        {
            if (consumer == AssetConsumer)
                throw new NullReferenceException("The IAssetConsumer instance was null.");

            References.Remove(consumer);
            consumer.RemoveReference(this);
            if (References.Count < 1) {
                Unload();
            }
        }

        internal override void Load()
        {
            if(Loaded)
                return;

            value = (T) processor.Construct();
            AssetConsumer.ReferenceAssets();
            Loaded = true;
            ContentLoader.AddReference(this);
        }

        internal override void Unload()
        {
            if(!Loaded)
                return;

            DrawCallDatabase.PruneAsset(Value);
            Loaded = false;
            AssetConsumer.DereferenceAssets();
            ContentLoader.RemoveReference(this);
            Value = default;
        }

        internal override void Purge()
        {
            if (References.Count < 1) {
                Unload();
            }
        }
    }
}
