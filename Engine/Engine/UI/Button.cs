using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Components.UI;
using SE.Rendering;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;

namespace SE.UI
{
    public class Button : UIObject
    {
        public override bool BlocksSelection { get; set; } = true;

        public Panel Background { get; private set; }

        public Image Image { get; private set; }
        
        public Text Text { get; private set; }

        public Color BackgroundColor
        {
            get => Background.SpriteColor;
            set => Background.SpriteColor = value;
        }

        public Color ImageColor
        {
            get => Image?.Color ?? Color.White;
            set {
                if (Image != null) {
                    Image.Color = value;
                }
            }
        }

        public Color TextColor {
            get => Text.SpriteColor;
            set => Text.SpriteColor = value;
        }

        public string TextString {
            get => Text.Value;
            set => Text.Value = value;
        }

        public SpriteFont Font {
            get => Text.Font;
            set => Text.Font = value;
        }

        public int BorderSize {
            get => Background.SlicedSprite.BorderSize;
            set => Background.SlicedSprite.BorderSize = value;
        }

        private Point size;
        public Point Size {
            get => size;
            set {
                size = value;
                if (Background != null) {
                    Background.SlicedSprite.Size = value;
                }
            }
        }

        public void SetImage(SpriteTexture image, Color? color, Point? imageSize = null)
        {
            if(image.Texture == null)
                return;

            // Calculate image size.
            if (imageSize.HasValue) {
                imageSize = new Point(imageSize.Value.X - (BorderSize * 2), imageSize.Value.Y - (BorderSize * 2));
            }
            Point tmpSize = imageSize ?? new Point(image.SourceRectangle.Width, image.SourceRectangle.Height);

            // Recreate or reset Image.
            if (Image != null) {
                Image.SpriteTexture = image;
                Image.Color = color ?? Color.White;
                Image.Size = tmpSize;
            } else {
                Image = new Image(Vector2.Zero, tmpSize, image) {
                    Align = Alignment.Center,
                    Parent = Background.Transform,
                    SpriteColor = color ?? Color.White
                };
            }
        }

        public void SetBackground(SlicedImage image, Color? color, int borderSize = 8)
        {
            if(image == null)
                return;

            // Destroy and recreate background. Or just create a background if one doesn't exist.
            if (Background != null) {
                Background.SlicedSprite = new UISlicedSprite(size, color ?? Color.White, image, borderSize);
            } else {
                Background = new Panel(Vector2.Zero, size, image, borderSize) {
                    Parent = Transform
                };
            }
        }

        public void SetFont(SpriteFont font, Color color)
        {
            if(font == null)
                return;

            // Create new Text UIObject.
            if (Text != null) {
                Text.Font = font;
                Text.SpriteColor = color;
                Text.Align = Alignment.Center;
            } else {
                Text = new Text(font, Vector2.Zero) {
                    Align = Alignment.Center,
                    Parent = Background.Transform,
                    SpriteColor = color
                };
            }
        }

        public Button(Vector2 pos, Point size, SpriteFont font, SlicedImage panelImage, int borderSize = 10) : base(pos, size)
        {
            this.size = size;
            SetBackground(panelImage, Color.White, borderSize);
            SetFont(font, Color.White);
            Interactable = true;
            Bounds = new RectangleF(Transform.GlobalPositionInternal.X, Transform.GlobalPositionInternal.Y, size.X, size.Y);
        }
    }
}