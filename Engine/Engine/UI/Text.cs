using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement;
using SE.Components.UI;
using SE.Pooling;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;

namespace SE.UI
{
    public class Text : UIObject, IPoolableGameObject
    {
        private TextSprite sprite;
        private string textString = "";
        private string prefix = "";

        public bool ReturnOnDestroy { get; set; }
        public IGameObjectPool MyPool { get; set; }

        public string Value {
            get => textString;
            set {
                textString = value ?? "";
                Microsoft.Xna.Framework.Vector2 size;
                if (string.IsNullOrEmpty(prefix)) {
                    sprite.Text = textString;
                    size = sprite.SpriteFont.MeasureString(textString);
                } else {
                    sprite.Text = prefix + textString;
                    size = sprite.SpriteFont.MeasureString(textString + prefix);
                }
                sprite.UnscaledSize = size.ToPoint();
                Bounds = new RectangleF(Transform.GlobalPositionInternal.X, Transform.GlobalPositionInternal.Y, (int)size.X, (int)size.Y);
                Transform.UpdateTransformation();
                RecalculateBoundsInternal();
            }
        }

        public string Prefix {
            get => prefix;
            set {
                prefix = value;
                Value = Value;
            }
        }

        public Asset<SpriteFont> Font {
            get => sprite.SpriteFontAsset;
            set => sprite.SpriteFontAsset = value;
        }

        public Text(Asset<SpriteFont> font, Vector2 pos, string str = "") : base(pos)
        {
            sprite = new TextSprite(font, str, Color.White);
            AddComponent(sprite);
            Interactable = false;
        }

        public void TakenFromPool() { }
        public void ReturnedToPool() { }
        public void PoolInitialize() { }
    }
}