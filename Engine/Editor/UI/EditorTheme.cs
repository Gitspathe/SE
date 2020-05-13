using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement;
using SE.Common;
using SE.Components.UI;
using SE.Core;
using SE.Rendering;
using SE.UI;
using SE.UI.Builders;
using Button = SE.UI.Button;
using Vector2 = System.Numerics.Vector2;

namespace SE.Editor.UI
{

    /// <summary>
    /// User interface theme used for the editor.
    /// </summary>
    public static class EditorTheme
    {
        public static SlicedImage9 PanelOpaque;
        public static SlicedImage9 PanelTransparent;
        public static SlicedImage9 PanelFullTransparent;

        private static SpriteFont editor;
        private static SpriteFont editorSubheading;
        private static SpriteFont editorHeading;

        private static AssetConsumerContext assetConsumerContext = new AssetConsumerContext();

        public static Panel CreatePanel(Vector2 pos, Color col, Point size, Transform parent = null)
        {
            return new PanelBuilder(pos, size)
               .Image(PanelOpaque, col, 8)
               .Parent(parent);
        }

        public static Toggle GetToggle(Vector2 pos, Color col, Color toggleColor, Transform parent = null)
        {
            SpriteTexture st = new SpriteTexture(AssetManager.GetAsset<Texture2D>("EditorUI"), new Rectangle(16,0,16,16));
            return new Toggle(pos, new Point(32, 32), st, st) {
                BackgroundColor = col,
                ToggleColor = toggleColor,
                Parent = parent
            };
        }

        public static Text CreateText(Vector2 pos, Color col, string text, Transform parent = null, FontType type = FontType.Normal)
        {
            TextBuilder tBuilder = new TextBuilder(pos, text)
                .Parent(parent);
            switch (type) {
                case FontType.Normal:
                    tBuilder.Font(editor, col);
                    break;
                case FontType.Subheading:
                    tBuilder.Font(editorSubheading, col);
                    break;
                case FontType.Heading:
                    tBuilder.Font(editorHeading, col);
                    break;
            }
            return tBuilder;
        }

        public static TextInputField CreateTextField(Vector2 pos, Point size, string prompt = "", Transform parent = null, ColorSet theme = ColorSet.Light)
        {
            TextInputFieldBuilder tBuilder = new TextInputFieldBuilder(pos, size)
               .TextAlignment(Alignment.Left)
               .Prompt(prompt)
               .Parent(parent);
            switch (theme) {
                case ColorSet.Light:
                    tBuilder.Background(PanelOpaque, Color.White, 4)
                       .Font(editor, Color.Black)
                       .Animation(Color.White, Color.Yellow, Color.Orange, Color.Red);
                    break;
                case ColorSet.Dark:
                    tBuilder.Background(PanelTransparent, new Color(Color.Black, 0.667f), 4)
                       .Font(editor, Color.Red)
                       .Animation(new Color(Color.Black, 0.667f), Color.DarkGray, Color.LightGray, Color.White);
                    break;
                case ColorSet.Blue:
                    tBuilder.Background(PanelTransparent, Color.Blue, 4)
                       .Font(editor, Color.White)
                       .Animation(Color.Blue, Color.DarkBlue, Color.Indigo, Color.Purple);
                    break;
                case ColorSet.Red:
                    tBuilder.Background(PanelTransparent, Color.Red, 4)
                       .Font(editor, Color.Black)
                       .Animation(Color.Red, Color.OrangeRed, Color.Orange, Color.Yellow);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
            TextInputField textField = tBuilder;
            UIColorAnimator colorAnim = textField.GetComponent<UIColorAnimator>();
            if (colorAnim != null) {
                textField.OnFocus += () => colorAnim.Toggle();
                textField.OnUnfocus += () => colorAnim.Untoggle();
            }
            return textField;
        }

        public static Button CreateButton(Vector2 pos, Point size, string text = "", Transform parent = null, ColorSet theme = ColorSet.Light)
        {
            ButtonBuilder bBuilder = new ButtonBuilder(pos, size, text)
               .Parent(parent);
            switch (theme) {
                case ColorSet.Light:
                    bBuilder.Background(PanelTransparent, Color.White)
                       .Font(editor, Color.Black)
                       .Animation(Color.White, Color.Yellow, Color.Orange, Color.Red);
                    break;
                case ColorSet.Dark:
                    bBuilder.Background(PanelOpaque, new Color(Color.Black, 0.667f))
                       .Font(editor, Color.White)
                       .Animation(new Color(Color.Black, 0.667f), Color.DarkGray, Color.Orange, Color.Red);
                    break;
                case ColorSet.Blue:
                    bBuilder.Background(PanelTransparent, Color.CornflowerBlue)
                       .Font(editor, Color.White)
                       .Animation(Color.CornflowerBlue, Color.Blue, Color.DarkBlue, Color.DarkBlue);
                    break;
                case ColorSet.Red:
                    bBuilder.Background(PanelTransparent, Color.Red)
                       .Font(editor, Color.Black)
                       .Animation(Color.Red, Color.Orange, Color.Yellow, Color.Yellow);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
            return bBuilder;
        }

        public static Button CreateButton(Vector2 pos, Point size, string text = "", SpriteTexture? image = null, Point? imageSize = null, Transform parent = null, ColorSet theme = ColorSet.Light)
        {
            ButtonBuilder bBuilder = new ButtonBuilder(pos, size, text)
               .Image(image ?? new SpriteTexture(), imageSize, Color.White)
               .Parent(parent);
            switch (theme) {
                case ColorSet.Light:
                    bBuilder.Background(PanelOpaque, Color.White)
                       .Font(editor, Color.Black)
                       .Animation(Color.White, Color.Yellow, Color.Orange, Color.Red);
                    break;
                case ColorSet.Dark:
                    bBuilder.Background(PanelFullTransparent, new Color(Color.Black, 0.667f))
                       .Font(editor, Color.White)
                       .Animation(new Color(Color.Black, 0.667f), Color.Gray, Color.LightGray, Color.White);
                    break;
                case ColorSet.Blue:
                    bBuilder.Background(PanelTransparent, Color.CornflowerBlue)
                       .Font(editor, Color.White)
                       .Animation(Color.CornflowerBlue, Color.Blue, Color.DarkBlue, Color.DarkBlue);
                    break;
                case ColorSet.Red:
                    bBuilder.Background(PanelTransparent, Color.Red)
                       .Font(editor, Color.Black)
                       .Animation(Color.Red, Color.Orange, Color.Yellow, Color.Yellow);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
            return bBuilder;
        }

        static EditorTheme()
        {
            // Create the default panel style.
            Texture2D texture = AssetManager.Get<Texture2D>(assetConsumerContext, "EditorUI");

            Rectangle left = new Rectangle(0, 4, 4, 24);
            Rectangle up = new Rectangle(4, 0, 24, 4);
            Rectangle upRight = new Rectangle(28, 0, 4, 4);
            Rectangle upLeft = new Rectangle(0, 0, 4, 4);
            Rectangle right = new Rectangle(28, 4, 4, 24);
            Rectangle down = new Rectangle(4, 28, 24, 4);
            Rectangle downRight = new Rectangle(28, 28, 4, 4);
            Rectangle downLeft = new Rectangle(0, 28, 4, 4);
            Rectangle middle = new Rectangle(4, 4, 24, 24);
            PanelOpaque = new SlicedImage9(texture, left, up, upLeft, upRight, right, down, downLeft, downRight, middle);

            left = new Rectangle(0 + 32, 4, 4, 24);
            up = new Rectangle(4 + 32, 0, 24, 4);
            upRight = new Rectangle(28 + 32, 0, 4, 4);
            upLeft = new Rectangle(0 + 32, 0, 4, 4);
            right = new Rectangle(28 + 32, 4, 4, 24);
            down = new Rectangle(4 + 32, 28, 24, 4);
            downRight = new Rectangle(28 + 32, 28, 4, 4);
            downLeft = new Rectangle(0 + 32, 28, 4, 4);
            middle = new Rectangle(4 + 32, 4, 24, 24);
            PanelTransparent = new SlicedImage9(texture, left, up, upLeft, upRight, right, down, downLeft, downRight, middle);

            left = new Rectangle(0 + 64, 4, 4, 24);
            up = new Rectangle(4 + 64, 0, 24, 4);
            upRight = new Rectangle(28 + 64, 0, 4, 4);
            upLeft = new Rectangle(0 + 64, 0, 4, 4);
            right = new Rectangle(28 + 64, 4, 4, 24);
            down = new Rectangle(4 + 64, 28, 24, 4);
            downRight = new Rectangle(28 + 64, 28, 4, 4);
            downLeft = new Rectangle(0 + 64, 28, 4, 4);
            middle = new Rectangle(4 + 64, 4, 24, 24);
            PanelFullTransparent = new SlicedImage9(texture, left, up, upLeft, upRight, right, down, downLeft, downRight, middle);

            editor = AssetManager.Get<SpriteFont>(assetConsumerContext, "editor");
            editorSubheading = AssetManager.Get<SpriteFont>(assetConsumerContext, "editor_subheading");
            editorHeading = AssetManager.Get<SpriteFont>(assetConsumerContext, "editor_heading");
        }

        public enum FontType
        {
            Normal,
            Subheading,
            Heading
        }

        public enum ColorSet
        {
            Light,
            Dark,
            Blue,
            Red
        }

    }

}
