using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement;
using SE.Core;
using SE.Rendering;

namespace SE.Engine.AssetManagement.Processors
{
    public class SpriteTextureProcessor : IAssetProcessor<SpriteTexture>
    {
        private string textureAssetKey;
        private Rectangle sourceRect;

        public SpriteTexture Construct()
        {
            Asset<Texture2D> asset = AssetManager.GetAsset<Texture2D>(textureAssetKey);
            return new SpriteTexture(asset, sourceRect);
        }

        object IAssetProcessor.Construct()
        {
            return Construct();
        }

        public SpriteTextureProcessor(string textureAssetKey, Rectangle sourceRect)
        {
            this.textureAssetKey = textureAssetKey;
            this.sourceRect = sourceRect;
        }

        public SpriteTextureProcessor() { }
    }
}
