using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement;
using SE.Components;
using SE.Core;
using SE.Rendering;
using Vector2 = System.Numerics.Vector2;

namespace SE.Debug
{
    public static class DrawUtility
    {
        private static SpriteTexture debugSpriteTex;
        private static AssetConsumerContext assetConsumerContext = new AssetConsumerContext();

        public static void DrawRectangle(Camera2D camera, Rectangle srcRect, Color? color = null, int borderWidth = 4)
        {
            if(debugSpriteTex.Texture == null)
                debugSpriteTex = AssetManager.Get<SpriteTexture>(assetConsumerContext, "floor");
            
            Color c = color ?? Color.Purple;
            SpriteBatch spriteBatch = Core.Rendering.SpriteBatch;
            Texture2D texture = debugSpriteTex.Texture;
            Rectangle r = srcRect;
            Vector2 viewPos = camera.Position;
            int bw = borderWidth;

            spriteBatch.Draw(texture, new Rectangle(r.Left - (int)viewPos.X, r.Top - (int)viewPos.Y, bw, r.Height),
                debugSpriteTex.SourceRectangle, c, 0.0f, Vector2.Zero, SpriteEffects.None, 0f); // Left

            spriteBatch.Draw(texture, new Rectangle(r.Right - (int)viewPos.X, r.Top - (int)viewPos.Y, bw, r.Height),
                debugSpriteTex.SourceRectangle, c, 0.0f, Vector2.Zero, SpriteEffects.None, 0f); // Right

            spriteBatch.Draw(texture, new Rectangle(r.Left - (int)viewPos.X, r.Top - (int)viewPos.Y, r.Width, bw),
                debugSpriteTex.SourceRectangle, c, 0.0f, Vector2.Zero, SpriteEffects.None, 0f); // Top

            spriteBatch.Draw(texture, new Rectangle(r.Left - (int)viewPos.X, r.Bottom - (int)viewPos.Y, r.Width, bw), 
                debugSpriteTex.SourceRectangle, c, 0.0f, Vector2.Zero, SpriteEffects.None, 0f); // Bottom
        }
    }
}
