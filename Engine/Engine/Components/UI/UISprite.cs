using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement;
using SE.Core;
using SE.Rendering;
using System;
using Vector2 = System.Numerics.Vector2;

namespace SE.Components.UI
{

    public class UISprite : SpriteBase, IUISprite
    {
        public Material Material {
            get => material;
            set {
                if (value == null)
                    value = Core.Rendering.BlankMaterial;
                if (value.Equals(material))
                    return;

                material = value;
                material.RegenerateDrawCall();
            }
        }
        private Material material = Core.Rendering.BlankMaterial;

        protected Rectangle TextureSourceRectangle;

        public Asset<SpriteTexture> SpriteTextureAsset {
            set {
                if (value == spriteTextureAssetInternal)
                    return;

                spriteTextureAssetInternal?.RemoveReference(AssetConsumer);
                spriteTextureAssetInternal = value;
                SpriteTexture = spriteTextureAssetInternal.Get(this);
                if (!Screen.IsFullHeadless && SpriteTexture.Texture == null)
                    throw new NullReferenceException("The specified SpriteTexture has no Texture2D asset. Ensure that the asset exists, and that it's being set.");

                Material.Texture = SpriteTexture.Texture;
                TextureSourceRectangle = SpriteTexture.SourceRectangle;
            }
        }
        private Asset<SpriteTexture> spriteTextureAssetInternal;

        public SpriteTexture SpriteTexture { get; private set; }

        public override void Render(Camera2D camera, Space space)
        {
            Rectangle bounds = this.bounds;
            if (space == Space.World) {
                bounds.X -= (int)camera.Position.X;
                bounds.Y -= (int)camera.Position.Y;
            }

            Core.Rendering.SpriteBatch.Draw(
                Material.Texture,
                bounds,
                TextureSourceRectangle,
                color,
                0f,
                origin,
                SpriteEffects.None,
                layerDepth);
        }

        public override void RecalculateBounds()
        {
            Point offset = Offset;
            Point size = Size;
            bounds.X = (int)(ownerTransform.GlobalPositionInternal.X + offset.X);
            bounds.Y = (int)(ownerTransform.GlobalPositionInternal.Y + offset.Y);
            bounds.Width = size.X;
            bounds.Height = size.Y;
        }

        /// <summary>Creates a new UI RendererType sprite instance. </summary>
        /// <param name="offsetPixels">Sprite offset for UIObject in pixels.</param>
        /// <param name="sizePixels">Sprite size for UIObject in pixels.</param>
        /// <param name="color">Color used.</param>
        /// <param name="spriteTexture">SpriteTexture used to draw the UI element.</param>
        public UISprite(Point offsetPixels, Point sizePixels, Color color, Asset<SpriteTexture> spriteTexture)
        {
            unscaledOffset = offsetPixels;
            unscaledSize = sizePixels;
            Color = color;
            SpriteTextureAsset = spriteTexture;
            Origin = Vector2.Zero;
        }

        /// <summary>Creates a new UI RendererType sprite instance. </summary>
        /// <param name="sizePixels">Sprite size for UIObject in pixels.</param>
        /// <param name="color">Color used.</param>
        /// <param name="spriteTexture">SpriteTexture used to draw the UI element.</param>
        public UISprite(Point sizePixels, Color color, Asset<SpriteTexture> spriteTexture)
        {
            unscaledSize = sizePixels;
            Color = color;
            SpriteTextureAsset = spriteTexture;
            Origin = Vector2.Zero;
        }
    }

    public interface IUISprite { }
}
