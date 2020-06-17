using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Rendering;
using Vector2 = System.Numerics.Vector2;

namespace SE.Components.UI
{
    public sealed class TextSprite : SpriteBase, IUISprite
    {
        /// <summary>Text used to render this sprite.</summary>
        public string Text;

        /// <summary>Font used to render this sprite.</summary>
        public SpriteFont SpriteFont;

        public override void Render(Camera2D camera, Space space)
        {
            Vector2 position = Owner.Transform.GlobalPositionInternal;
            Rectangle bounds = Bounds;
            if (space == Space.World) {
                position.X -= camera.Position.X;
                position.Y -= camera.Position.Y;
                bounds.X -= (int) camera.Position.X;
                bounds.Y -= (int) camera.Position.Y;
            }
            position = new Vector2((int) position.X, (int) position.Y);

            Core.Rendering.SpriteBatch.DrawString(
                SpriteFont,
                Text,
                position, 
                color, 
                0, 
                origin, 
                Owner.Transform.GlobalScaleInternal, 
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
        /// <param name="spriteFont">SpriteFont used.</param>
        /// <param name="text">String to display.</param>
        /// <param name="color">Color of the text.</param>
        public TextSprite(SpriteFont spriteFont, string text, Color color)
        {
            SpriteFont = spriteFont;
            Text = text;
            Color = color;
            Origin = Vector2.Zero;
            Size = spriteFont.MeasureString(text).ToPoint();
        }

        /// <summary>Creates a new Text RendererType sprite instance.</summary>
        /// <param name="spriteFont">SpriteFont used.</param>
        /// <param name="text">String to display.</param>
        /// <param name="color">Color of the text.</param>
        /// <param name="originPoint">Origin point.</param>
        public TextSprite(SpriteFont spriteFont, string text, Color color, Vector2 originPoint)
        {
            SpriteFont = spriteFont;
            Text = text;
            Color = color;
            Origin = originPoint;
            Size = spriteFont.MeasureString(text).ToPoint();
        }

    }
}
