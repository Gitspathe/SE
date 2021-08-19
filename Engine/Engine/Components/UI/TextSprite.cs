using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement;
using SE.Rendering;
using System.Threading.Tasks;
using Vector2 = System.Numerics.Vector2;

namespace SE.Components.UI
{
    public sealed class TextSprite : SpriteBase, IUISprite
    {
        /// <summary>Text used to render this sprite.</summary>
        public string Text;

        /// <summary>Font used to render this sprite.</summary>
        public Asset<SpriteFont> SpriteFontAsset {
            get => spriteFontAsset;
            set {
                spriteFontAsset?.RemoveReference(AssetConsumer);
                spriteFontAsset = value;
                SpriteFont = spriteFontAsset?.Get(this);
            }
        }
        private Asset<SpriteFont> spriteFontAsset;

        public SpriteFont SpriteFont { get; private set; }

        public override void Render(Camera2D camera, Space space)
        {
            Vector2 position = ownerTransform.GlobalPositionInternal;
            if (space == Space.World) {
                position -= camera.Position;
                bounds.X -= (int) camera.Position.X;
                bounds.Y -= (int) camera.Position.Y;
            }
            position = new Vector2((int) position.X, (int) position.Y);

            Core.Rendering.SpriteBatch.DrawString(
                SpriteFontAsset.Value,
                Text,
                position,
                color,
                0,
                origin, 
                ownerTransform.GlobalScaleInternal, 
                SpriteEffects.None, 
                LayerDepth + 0.000001f);
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

        /// <summary>Creates a new Text RendererType sprite instance.</summary>
        /// <param name="spriteFontAsset">SpriteFont used.</param>
        /// <param name="text">String to display.</param>
        /// <param name="color">Color of the text.</param>
        public TextSprite(Asset<SpriteFont> spriteFontAsset, string text, Color color)
        {
            SpriteFontAsset = spriteFontAsset;
            Text = text;
            Color = color;
            Origin = Vector2.Zero;
            Size = SpriteFont.MeasureString(text).ToPoint();
        }

        /// <summary>Creates a new Text RendererType sprite instance.</summary>
        /// <param name="spriteFontAsset">SpriteFont used.</param>
        /// <param name="text">String to display.</param>
        /// <param name="color">Color of the text.</param>
        /// <param name="originPoint">Origin point.</param>
        public TextSprite(Asset<SpriteFont> spriteFontAsset, string text, Color color, Vector2 originPoint)
        {
            SpriteFontAsset = spriteFontAsset;
            Text = text;
            Color = color;
            Origin = originPoint;
            Size = SpriteFont.MeasureString(text).ToPoint();
        }

    }
}
