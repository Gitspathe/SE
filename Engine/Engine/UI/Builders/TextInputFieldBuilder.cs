using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Common;
using SE.Components.UI;
using Vector2 = System.Numerics.Vector2;

namespace SE.UI.Builders
{
    public class TextInputFieldBuilder
    {
        private TextInputField textInputField;

        public TextInputFieldBuilder Background(SlicedImage image, Color? color = null, int? borderSize = null)
        {
            textInputField.SetBackground(image, textInputField.Size, borderSize, color);
            return this;
        }

        public TextInputFieldBuilder Background(SlicedImage image, Point? size, Color? color = null, int? borderSize = null)
        {
            textInputField.SetBackground(image, size, borderSize, color);
            return this;
        }

        public TextInputFieldBuilder BackgroundColor(Color color)
        {
            textInputField.BackgroundColor = color;
            return this;
        }

        public TextInputFieldBuilder BorderSize(int size)
        {
            textInputField.Background.BorderSize = size;
            return this;
        }

        public TextInputFieldBuilder Size(Point size)
        {
            textInputField.Size = size;
            return this;
        }

        public TextInputFieldBuilder Font(SpriteFont font, Color? color = null)
        {
            textInputField.SetFont(font, color);
            return this;
        }

        public TextInputFieldBuilder Text(string input)
        {
            textInputField.Value = input;
            return this;
        }

        public TextInputFieldBuilder Text(string prompt, string input)
        {
            textInputField.Prompt = prompt;
            textInputField.Value = input;
            return this;
        }

        public TextInputFieldBuilder Prompt(string prompt)
        {
            textInputField.Prompt = prompt;
            return this;
        }

        public TextInputFieldBuilder TextAlignment(Alignment alignment)
        {
            textInputField.TextAlignment = alignment;
            return this;
        }

        public TextInputFieldBuilder Alignment(Alignment alignment)
        {
            textInputField.Align = alignment;
            return this;
        }

        public TextInputFieldBuilder Parent(Transform parent)
        {
            textInputField.Parent = parent;
            return this;
        }

        public TextInputFieldBuilder(Vector2 pos, Point size)
        {
            textInputField = new TextInputField(pos, size);
        }

        public TextInputFieldBuilder(TextInputField existing)
        {
            textInputField = existing;
        }

        public TextInputFieldBuilder Animation(Color idle, Color highlighted, Color clicked, Color? toggled = null, float transition = 0.333f)
        {
            textInputField.RemoveComponentsOfType<UIColorAnimator>();

            UIColorAnimator animator = new UIColorAnimator(textInputField.Background);
            textInputField.AddComponent(animator);
            animator.Configure(idle, highlighted, clicked, toggled, transition);
            return this;
        }

        public TextInputField Done()
        {
            return textInputField;
        }

        public static implicit operator TextInputFieldBuilder(TextInputField x)
        {
            return new TextInputFieldBuilder(x);
        }

        public static implicit operator TextInputField(TextInputFieldBuilder x)
        {
            return x.textInputField;
        }
    }
}
