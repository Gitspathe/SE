using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Core;
using SE.Rendering;

namespace SE.AssetManagement.Processors
{
    public class SpriteTextureProcessor : AssetProcessor
    {
        private string textureAssetKey;
        private Rectangle sourceRect;

        public override HashSet<IAsset> GetReferencedAssets()
        {
            IAsset asset = AssetManager.GetIAsset<Texture2D>(textureAssetKey);
            return new HashSet<IAsset> { asset };
        }

        public override object Construct()
        {
            Asset<Texture2D> asset = AssetManager.GetAsset<Texture2D>(textureAssetKey);
            return new SpriteTexture(asset, sourceRect);
        }

        public SpriteTextureProcessor(string textureAssetKey, Rectangle sourceRect)
        {
            this.textureAssetKey = textureAssetKey;
            this.sourceRect = sourceRect;
        }

        public SpriteTextureProcessor() { }
    }
}
