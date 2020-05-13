using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement;

namespace SE.Rendering
{

    /// <summary>
    /// Class which bundles a texture sheet and a source rectangle together to render a sprite.
    /// </summary>
    public struct SpriteTexture : IEquatable<SpriteTexture>
    {
        /// <summary>The source rectangle used to grab the sprite from the texture sheet.</summary>
        public Rectangle SourceRectangle;

        /// <summary>The Texture2D texture sheet used to render the sprite.</summary>
        public Texture2D Texture => textureAsset?.Value;
        private Asset<Texture2D> textureAsset;

        /// <summary>
        /// Returns a new SpriteTexture instance.
        /// </summary>
        /// <param name="texture">Texture sheet.</param>
        /// <param name="rect">Source rectangle of the texture sheet.</param>
        public SpriteTexture(Asset<Texture2D> texture, Rectangle rect)
        {
            textureAsset = texture;
            SourceRectangle = rect;
        }

        public static bool operator ==(SpriteTexture a, SpriteTexture b) => a.Equals(b);
        public static bool operator !=(SpriteTexture a, SpriteTexture b) => !(a == b);

        public bool Equals(SpriteTexture other) 
            => Equals(textureAsset, other.textureAsset) && SourceRectangle.Equals(other.SourceRectangle);

        public override bool Equals(object obj) 
            => obj is SpriteTexture other && Equals(other);

        public override int GetHashCode() 
            => HashCode.Combine(textureAsset, SourceRectangle);
    }

}
