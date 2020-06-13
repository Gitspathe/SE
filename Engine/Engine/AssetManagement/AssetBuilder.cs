using System;
using System.Collections.Generic;
using SE.AssetManagement.Processors;

namespace SE.AssetManagement
{
    /// <summary>
    /// Builder class for <c>Asset</c> instances.
    /// </summary>
    /// <typeparam name="T">Asset type. I.e, Texture2D, SpriteTexture, etc.</typeparam>
    public class AssetBuilder<T>
    {
        private string id;
        private IAssetProcessor processor;
        private ContentLoader contentLoader;
        private HashSet<IAsset> references = new HashSet<IAsset>();

        public AssetBuilder<T> ID(string id)
        {
            this.id = id;   
            return this;
        }

        /// <summary>
        /// Initializes the asset's load function.
        /// </summary>
        /// <param name="processor">Function called whenever the asset is reloaded.</param>
        /// <returns>Asset builder.</returns>
        public AssetBuilder<T> Create(IAssetProcessor processor)
        {
            this.processor = processor;
            return this;
        }

        /// <summary>
        /// Initializes the asset's content loader reference.
        /// </summary>
        /// <param name="contentLoader">Content loader that the asset belongs to.</param>
        /// <returns>Asset builder.</returns>
        public AssetBuilder<T> FromContent(ContentLoader contentLoader)
        {
            this.contentLoader = contentLoader;
            return this;
        }

        /// <summary>
        /// Initializes the asset's references.
        /// </summary>
        /// <param name="references">References/dependencies.</param>
        /// <returns>Asset builder.</returns>
        public AssetBuilder<T> References(params IAsset[] references)
        {
            for (int i = 0; i < references.Length; i++) {
                this.references.Add(references[i]);
            }
            return this;
        }

        /// <summary>
        /// Finalizes the asset builder and returns an asset instance.
        /// Note: Calling this isn't necessary to add the asset to the asset manager.
        /// </summary>
        /// <returns>Asset instance.</returns>
        public Asset<T> Finish()
        {
            if (processor == null)
                throw new Exception("Asset creation function wasn't set.");
            if (contentLoader == null)
                throw new Exception("Asset wasn't bound to a ContentLoader.");

            Asset<T> asset = new Asset<T>(id, processor, contentLoader, references);
            //references.Clear();
            //references = null;
            //contentLoader = null;
            //processor = null;
            return asset;
        }
    }
}
