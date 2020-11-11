using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement;
using SE.Common;
using SE.Components.UI;
using SE.Core;
using SE.Rendering;
using Vector2 = System.Numerics.Vector2;

namespace SE.UI.Builders
{

    public class ButtonBuilder
    {
        private Button button;

        public ButtonBuilder Text(string text, Asset<SpriteFont> font = null, Color? color = null)
        {
            if (button.Text == null)
                return this;

            if (font != null) {
                button.Font = font;
            }
            if (color.HasValue) {
                button.TextColor = color.Value;
            }
            button.TextString = text;
            return this;
        }

        public ButtonBuilder Font(Asset<SpriteFont> font, Color? color = null)
        {
            button.SetFont(font, color ?? Color.White);
            return this;
        }

        public ButtonBuilder BackgroundColor(Color color)
        {
            button.BackgroundColor = color;
            return this;
        }

        public ButtonBuilder Image(Asset<SpriteTexture> image, Point? size = null, Color? color = null)
        {
            button.SetImage(image, color, size);
            return this;
        }

        public ButtonBuilder ImageColor(Color color)
        {
            if(button.Image == null)
                return this;

            button.ImageColor = color;
            return this;
        }

        public ButtonBuilder TextColor(Color color)
        {
            button.TextColor = color;
            return this;
        }

        public ButtonBuilder BorderSize(int size)
        {
            button.BorderSize = size;
            return this;
        }

        public ButtonBuilder Background(SlicedImage image, int borderSize = 8)
        {
            button.SetBackground(image, null, borderSize);
            return this;
        }

        public ButtonBuilder Background(SlicedImage image, Color color, int borderSize = 8)
        {
            button.SetBackground(image, color, borderSize);
            return this;
        }

        public ButtonBuilder Parent(Transform parent)
        {
            button.Parent = parent;
            return this;
        }

        public ButtonBuilder Animation(Color idle, Color highlighted, Color clicked, Color? toggled = null, float transition = 0.333f)
        {
            button.RemoveComponentsOfType<UIColorAnimator>();

            UIColorAnimator animator = new UIColorAnimator(button.Background);
            button.AddComponent(animator);
            animator.Configure(idle, highlighted, clicked, toggled, transition);
            return this;
        }

        public Button Done()
        {
            return button;
        }

        public ButtonBuilder(Vector2 pos, Point size, string text)
        {
            button = new Button(pos, size, UIManager.DefaultFont, UIManager.DefaultSlicedImage) {
                TextString = text
            };
        }

        public ButtonBuilder(Button existing)
        {
            button = existing;
        }

        public static implicit operator ButtonBuilder(Button x)
        {
            return new ButtonBuilder(x);
        }

        public static implicit operator Button(ButtonBuilder x)
        {
            return x.button;
        }
    }
}
