using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement;
using SE.Rendering;
using Vector2 = System.Numerics.Vector2;

namespace SE.Components.UI
{

    public class UISprite : SpriteBase, IUISprite
    {
        public override void Render(Camera2D camera, Space space)
        {
            Rectangle bounds = this.bounds;
            if (space == Space.World) {
                bounds.X -= (int)camera.Position.X;
                bounds.Y -= (int)camera.Position.Y;
            }

            Core.Rendering.SpriteBatch.Draw(
                Data.Material.Texture, 
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
            Rectangle bounds = Bounds;
            Point offset = Offset;
            Point size = Size;
            bounds.X = (int)(Owner.Transform.GlobalPositionInternal.X + offset.X);
            bounds.Y = (int)(Owner.Transform.GlobalPositionInternal.Y + offset.Y);
            bounds.Width = size.X;
            bounds.Height = size.Y;
            Bounds = bounds;
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
