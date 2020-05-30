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
                    iAsset.Load();

                return value;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => this.value = value;
        }
        private T value;

        public string ID { get; internal set; }

        public ContentLoader ContentLoader { get; }

        uint IAsset.LoadOrder { get; set; }
        bool IAsset.Loaded { 
            get => isLoaded;
            set => isLoaded = value;
        }
        private bool isLoaded; // Optimization.

        HashSet<IAsset> IAssetConsumer.ReferencedAssets { get; set; } = new HashSet<IAsset>();

        HashSet<IAssetConsumer> IAsset.References { get; set; } = new HashSet<IAssetConsumer>();

        private IAssetProcessor processor;
        private IAsset iAsset;
        private IAssetConsumer iAssetConsumer;

        internal T Get(IAssetConsumer consumer)
        {
            if (consumer == null)
                throw new NullReferenceException("The IAssetConsumer instance was null.");
            if(consumer == this)
                throw new InvalidOperationException("Attempted to retrieve self.");

            iAsset.AddReference(consumer);
            return GetNoRef();
        }

        internal T GetNoRef()
        {
            iAsset.Load();
            return Value;
        }

        void IAsset.RemoveReference(IAssetConsumer consumer)
        {
            if (consumer == null)
                throw new NullReferenceException("The IAssetConsumer instance was null.");

            iAsset.References.Remove(consumer);
            consumer.RemoveReference(this);
            if (iAsset.References.Count < 1) {
                iAsset.Unload();
            }
        }

        void IAsset.AddReference(IAssetConsumer consumer)
        {
            if (consumer == null)
                throw new NullReferenceException("The IAssetConsumer instance was null.");
            if(consumer == this)
                return;

            iAsset.References.Add(consumer);
            consumer.AddReference(this);
            if (iAsset.References.Count > 0) {
                iAsset.Load();
            }
        }

        void IAsset.Load()
        {
            if(isLoaded)
                return;

            value = (T) processor.Construct();
            iAssetConsumer.ReferenceAssets();
            iAsset.Loaded = true;
            ContentLoader.AddReference(this);
        }

        void IAsset.Unload()
        {
            if(!isLoaded)
                return;

            DrawCallDatabase.PruneAsset(Value);
            iAsset.Loaded = false;
            iAssetConsumer.DereferenceAssets();
            ContentLoader.RemoveReference(this);
            Value = default;
        }

        void IAsset.Purge()
        {
            if (iAsset.References.Count < 1) {
                iAsset.Unload();
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
            SetupInterfaces();
            iAsset.LoadOrder = AssetManager.CurrentAssetPriority++;

            ContentLoader = contentLoader;
            this.processor = processor;
            if (referencedAssets == null) 
                return;

            foreach (IAsset refAsset in referencedAssets) {
                refAsset?.AddReference(this);
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

        private void SetupInterfaces()
        {
            iAsset = this;
            iAssetConsumer = this;
        }

    }
}
