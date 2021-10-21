using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Core;
using SE.Rendering;
using System.Collections.Generic;

namespace SE.AssetManagement.Processors
{
    public class SpriteTextureProcessor : AssetProcessor
    {
        private string textureAssetKey;
        private Rectangle sourceRect;

        public override HashSet<Asset> GetReferencedAssets()
        {
            // Don't reference Texture2D if in headless mode!
            return Screen.IsFullHeadless
                ? new HashSet<Asset>()
                : new HashSet<Asset> { AssetManager.GetAsset<Texture2D>(textureAssetKey) };
        }

        public override object Construct()
        {
            Asset<Texture2D> asset = Screen.IsFullHeadless
                ? null
                : AssetManager.GetAsset<Texture2D>(textureAssetKey);

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
