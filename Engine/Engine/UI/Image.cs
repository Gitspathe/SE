using Microsoft.Xna.Framework;
using SE.AssetManagement;
using SE.Components.UI;
using SE.Rendering;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;

namespace SE.UI
{
    public class Image : UIObject
    {
        public UISprite Sprite { get; }

        public Asset<SpriteTexture> SpriteTextureAsset {
            set => Sprite.SpriteTextureAsset = value;
        }

        public SpriteTexture SpriteTexture => Sprite.SpriteTexture;

        public Point OriginalSize { get; }

        public Point Size {
            get => Sprite.Size;
            set => Sprite.Size = value;
        }

        public Color Color {
            get => Sprite.Color;
            set => Sprite.Color = value;
        }

        public Image(Vector2 pos, Point size, Asset<SpriteTexture> spriteTex) : base(pos, size)
        {
            OriginalSize = size;
            Sprite = new UISprite(size, Color.White, spriteTex);
            AddComponent(Sprite);
            Bounds = new RectangleF(Transform.GlobalPositionInternal.X, Transform.GlobalPositionInternal.Y, size.X, size.Y);
        }

    }
}
