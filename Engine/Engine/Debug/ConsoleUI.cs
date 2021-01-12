using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SE.Core;
using SE.Editor.UI;
using SE.Pooling;
using SE.Rendering;
using SE.UI;
using Vector2 = System.Numerics.Vector2;

namespace SE.Debug
{

    public class ConsoleUI : UIObject
    {
        public ScrollView ScrollView;
        public ScrollBar ScrollBar;
        public TextInputField TextInputField;

        private Color consoleColor;
        private Image background;
        //public Text text;

        private List<Text> textLines;
        private GameObjectPool<Text> textPool;
        private int maxLines = 250;

        public override bool IsRootUIMenu => true;

        public override string RootUIName => "Console";

        public override int RootUIPriority => 99;

        public override bool DestroyOnLoad => false;

        public ConsoleUI() : base(Vector2.Zero)
        {
            consoleColor = new Color(0, 0, 0, 200);
            if (Screen.DisplayMode != DisplayMode.Normal) {
                background = new Image(new Vector2(0, 0), new Point(1920 - 30, 1040), AssetManager.GetAsset<SpriteTexture>("uiRect")) {
                    SpriteColor = consoleColor,
                    Parent = Transform
                };

                ScrollView = new ScrollView(new Vector2(0, 0), new Point(1920 - 30, 1040),
                    new Rectangle(0, 40, 1920, 1040)) {
                    Parent = background.Transform,
                    DisableHidden = true
                };

                TextInputField = EditorTheme.CreateTextField(new Vector2(0, 1040), new Point(1920, 40), ">", 
                    background.Transform);

                ScrollBar = new ScrollBar(new Vector2(1920 - 30, 0), new Point(30, 1040), new Point(50, 50), AssetManager.GetAsset<SpriteTexture>("uiRect"), AssetManager.GetAsset<SpriteTexture>("uiRect")) {
                    BackgroundColor = Color.Black,
                    HandleColor = Color.White,
                    Parent = background.Transform
                };
            } else {
                background = new Image(new Vector2(0, 1080 - 300), new Point(1920 - 30, 300), AssetManager.GetAsset<SpriteTexture>("uiRect")) {
                    SpriteColor = consoleColor,
                    Parent = Transform
                };

                ScrollView = new ScrollView(new Vector2(0, 0), new Point(1920 - 30, 300),
                    new Rectangle(0, 1080 - 300, 1920, 300)) {
                    Parent = background.Transform,
                    DisableHidden = true
                };

                TextInputField = EditorTheme.CreateTextField(new Vector2(0, 265), new Point(1920, 35), ">", 
                    background.Transform);

                ScrollBar = new ScrollBar(new Vector2(1920 - 30, 0), new Point(30, 265), new Point(50, 50), AssetManager.GetAsset<SpriteTexture>("uiRect"), AssetManager.GetAsset<SpriteTexture>("uiRect")) {
                    BackgroundColor = Color.Black,
                    HandleColor = Color.White,
                    Parent = background.Transform
                };
            }
            textLines = new List<Text>(maxLines);
            textPool = new GameObjectPool<Text>(() => EditorTheme.CreateText(Vector2.Zero, Color.Red, "", ScrollView.Transform), maxLines) {
                Behaviour = PoolBehavior.Fixed,
                ReturnOnDestroy = true
            };

            ScrollView.ScrollBar = ScrollBar;
            TextInputField.DefocusOnConfirm = false;
        }

        protected override void OnEnable(bool enableAllChildren = true)
        {
            base.OnEnable(enableAllChildren);
            TextInputField.IsActive = true;
            UIManager.AddAssumedKeyboardControl(this);
        }

        protected override void OnDisable(bool isRoot = false)
        {
            base.OnDisable(isRoot);
            UIManager.RemoveAssumedKeyboardControl(this);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            int height = Math.Clamp(64 + (textLines.Count * 24), 300, 99999);
            if (Screen.DisplayMode != DisplayMode.Normal) {
                ScrollView.ScrollPortSize = new Point(1920 - 30, height);
            } else {
                ScrollView.ScrollPortSize = new Point(1920 - 30, height);
            }
        }

        public void Output(string str, Color color)
        {
            Text t = textPool.Take();
            if (t == null) {
                t = textLines[0];
                textLines.Remove(t);
            }
            t.Value = str;
            t.SpriteColor = color;
            textLines.Add(t);

            for (int i = 0; i < textLines.Count; i++) {
                textLines[i].Transform.Position = new Vector2(20, 20 + (24 * i));
            }
        }

    }

}
