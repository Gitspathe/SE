using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Common;
using SE.Core;
using Vector2 = System.Numerics.Vector2;

namespace SE.UI.Builders
{
    public class TextBuilder
    {
        private Text text;

        public TextBuilder Value(string str)
        {
            text.Value = str;
            return this;
        }

        public TextBuilder Value(string prefix, string str)
        {
            text.Prefix = prefix;
            text.Value = str;
            return this;
        }

        public TextBuilder Prefix(string prefix)
        {
            text.Prefix = prefix;
            return this;
        }

        public TextBuilder Font(SpriteFont font, Color color)
        {
            text.Font = font;
            text.SpriteColor = color;
            return this;
        }

        public TextBuilder Font(SpriteFont font)
        {
            text.Font = font;
            return this;
        }

        public TextBuilder Color(Color color)
        {
            text.SpriteColor = color;
            return this;
        }

        public TextBuilder Alignment(Alignment alignment)
        {
            text.Align = alignment;
            return this;
        }

        public TextBuilder Parent(Transform parent)
        {
            text.Parent = parent;
            return this;
        }

        public Text Done()
        {
            return text;
        }

        public TextBuilder(Vector2 position, string str = "")
        {
            text = new Text(UIManager.DefaultFont, position, str);
        }

        public TextBuilder(Text existing)
        {
            text = existing;
        }

        public static implicit operator TextBuilder(Text x)
        {
            return new TextBuilder(x);
        }

        public static implicit operator Text(TextBuilder x)
        {
            return x.text;
        }
    }
}
