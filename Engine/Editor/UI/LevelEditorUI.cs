//#if EDITOR
//using Button = DeeZ.Engine.UI.Button;
//using System;
//using System.Collections.Generic;
//using DeeZ.Core;
//using DeeZ.Editor.Components.UI;
//using DeeZ.Engine.Common;
//using DeeZ.Engine.Components;
//using DeeZ.Engine.Components.UI;
//using DeeZ.Engine.Input;
//using DeeZ.Engine.Rendering;
//using DeeZ.Engine.UI;
//using Microsoft.Xna.Framework;
//using Vector2 = System.Numerics.Vector2;

//namespace DeeZ.Editor.UI
//{

//    /// <summary>
//    /// This GameObject manages the user interface for the level editor.
//    /// </summary>
//    internal class LevelEditorUI : UIObject
//    {

//        public override bool IsRootUIMenu => true;

//        public override string RootUIName => "LevelEditor";

//        public override int RootUIPriority => 10;

//        public override bool DestroyOnLoad => false;

//        private Color editorColor;
//        private Panel mainPanel;
//        private Panel textPanel;
//        private Text heading;
//        private UIObject tileMenu;
//        //private ScrollView scrollView;
//        //private ScrollBar scrollBar;

//        private UIInputController thisInput;
//        private Dictionary<int, Func<Vector2, GameObject>> tileSet;
//        private List<Button> tileSetButtons = new List<Button>();
//        private List<Button> tileSetPageButtons = new List<Button>();
//        private List<UIObject> tileSetPages = new List<UIObject>();
//        private int selectedTilesetItem = -1;
//        private int selectedTilesetPage = -1;

//        private static readonly Point tileButtonSize = new Point(100, 100);

//        private const int _ITEMS_PER_ROW = 18;
//        private const int _ROWS_PER_PAGE = 9;
//        private const int _ITEMS_PER_PAGE = _ITEMS_PER_ROW * _ROWS_PER_PAGE;

//        public LevelEditorUI() : base(Vector2.Zero)
//        {
//            tileSet = AssetManager.GetDictionary<int, Func<Vector2, GameObject>>(this);
//            editorColor = new Color(0, 0, 0, 200);
//            mainPanel = EditorTheme.CreatePanel(new Vector2(50,50), editorColor, new Point(1920-110, 910), Transform);
//            textPanel = EditorTheme.CreatePanel(new Vector2(100, -40), editorColor, new Point(200, 40), mainPanel.Transform);
//            heading = EditorTheme.CreateText(new Vector2(15, 10), Color.Red, "LEVEL EDITOR", textPanel.Transform, EditorTheme.FontType.Subheading);

//            //scrollView = new ScrollView(new Vector2(0, 910), new Point(1920, 4000), new Rectangle(0, 50, 1920, 910)) {
//            //    Parent = mainPanel.Transform
//            //};
//            //scrollBar = new ScrollBar(new Vector2(-50, 0), new Point(50, 500), new Point(50, 80), Database.Get<SpriteTexture>("floor"), Database.Get<SpriteTexture>("floor")) {
//            //    BackgroundColor = Color.Black,
//            //    HandleColor = Color.White,
//            //    Parent = mainPanel.Transform
//            //};
//            //scrollView.scrollBar = scrollBar;

//            // Tile placement sub-menu.
//            tileMenu = new UIObject(new Vector2(5, 5)) {
//                Parent = mainPanel.Transform
//            };

//            thisInput = new UIInputController();
//            thisInput.OnBack += () => { Disable(); };
//            AddComponent(thisInput);

//            // Create a button for each tile.
//            int rows = 1 + (tileSet.Count / _ITEMS_PER_ROW);
//            int pages = 1 + (rows / _ROWS_PER_PAGE);
//            for (int page = 0; page < pages; page++) {
//                CreateTilesetPage(page);
//            }
//            ChangeTilesetPage(0);

//            for (int i = 0; i < tileSetButtons.Count; i++) {;
//                UIInputController input = tileSetButtons[i].GetComponent<UIInputController>();
//                ConfigureInputController(input, i);
//                if (i == 0) {
//                    thisInput.Down = input;
//                }
//            }

//            // TODO: More submenus (label and zone system?)

//        }

//        protected override void OnEnable(bool enableAllChildren = false)
//        {
//            base.OnEnable(enableAllChildren);
//            UIManager.RootInputFocus = GetComponent<UIInputController>();
//        }

//        private void ChangeTilesetPage(int page)
//        {
//            for (int i = 0; i < tileSetPages.Count; i++) {
//                if (i == page) {
//                    tileSetPages[i].Enable(true);
//                } else {
//                    tileSetPages[i].Disable(true);
//                }
//            }
//        }

//        private void CreateTilesetPage(int page)
//        {
//            Button b = EditorTheme.CreateButton(new Vector2(1700 - (page * 40), 905), new Point(40, 40),
//                (page + 1).ToString(), tileMenu.Transform, EditorTheme.ColorSet.Dark);

//            UIObject tmpPage = new UIObject(Vector2.Zero) {
//                Parent = tileMenu.Transform
//            };
//            //UIObject tmpPage = new UIObject(Vector2.Zero) {
//            //    Parent = scrollView.Transform,
//            //};
//            b.AddComponent(new IDHolder(page));
//            b.Clicked += (sender, args) => {
//                if (args.MouseButton == MouseButtons.Left) {
//                    ClickedTilesetPageButton(((UIObject)sender).GetComponent<IDHolder>().id);
//                }
//            };
//            tileSetPageButtons.Add(b);
//            tileSetPages.Add(tmpPage);
//            int rows = 1 + ((tileSet.Count)-(page*_ITEMS_PER_PAGE)) / _ITEMS_PER_ROW;
//            int index = 0 + (page * _ITEMS_PER_PAGE);
//            for (int y = 0; y < rows; y++) {
//                if (y >= _ROWS_PER_PAGE)
//                    break;

//                for (int x = 0; x < _ITEMS_PER_ROW; x++) {
//                    if (index >= tileSet.Count)
//                        break;

//                    CreateTileButton(new Vector2(x * 100, y * 100), index, tileSetPages[page]);
//                    index++;
//                }
//            }
//        }

//        private void CreateTileButton(Vector2 pos, int i, UIObject parent)
//        {
//            Func<Vector2, GameObject> f = tileSet[i];
//            GameObject tmp = f.Invoke(new Vector2(0, 0));
//            SpriteTexture tmpTex = GetSpriteTextureFromExisting(tmp.GetComponent<Sprite>().spriteTexture);
//            Button b = EditorTheme.CreateButton(pos, tileButtonSize, null, tmpTex, null, parent.Transform);
//            b.ImageColor = tmp.GetComponent<Sprite>().Color;

//            b.AddComponent(new IDHolder(i));
//            b.Clicked += (sender, args) => {
//                if (args.MouseButton == MouseButtons.Left) {
//                    ClickedUIButton(((UIObject)sender).GetComponent<IDHolder>().id);
//                }
//                //b.Transform.Scale *= 1.1f; // Testing code.
//            };

//            UIInputController input = new UIInputController();
//            input.Back = thisInput;
//            b.AddComponent(input);
//            tileSetButtons.Add(b);
//            tmp.Destroy();
//        }

//        private void ConfigureInputController(UIInputController input, int i)
//        {
//            if (i - _ITEMS_PER_ROW > -1) {
//                input.Up = tileSetButtons[i - _ITEMS_PER_ROW].GetComponent<UIInputController>();
//            }
//            if (i + 1 < tileSetButtons.Count) {
//                input.Right = tileSetButtons[i + 1].GetComponent<UIInputController>();
//            }
//            if (i + _ITEMS_PER_ROW < tileSetButtons.Count) {
//                input.Down = tileSetButtons[i + _ITEMS_PER_ROW].GetComponent<UIInputController>();
//            }
//            if (i - 1 > -1) {
//                input.Left = tileSetButtons[i - 1].GetComponent<UIInputController>();
//            }
//        }

//        private void ClickedUIButton(int id)
//        {
//            selectedTilesetItem = id;
//            LevelEditor.selectedTileID = selectedTilesetItem;
//            for (int i = 0; i < tileSetButtons.Count; i++) {
//                Button b = tileSetButtons[i];
//                if (i == selectedTilesetItem) {
//                    b.GetComponent<UIAnimation>().Toggle();
//                } else {
//                    b.GetComponent<UIAnimation>().Untoggle();
//                }
//            }
//        }

//        private void ClickedTilesetPageButton(int id)
//        {
//            if (selectedTilesetPage == id)
//                return;

//            selectedTilesetPage = id;
//            ChangeTilesetPage(selectedTilesetPage);
//            for (int i = 0; i < tileSetPageButtons.Count; i++) {
//                Button b = tileSetPageButtons[i];
//                if (i == selectedTilesetPage) {
//                    b.GetComponent<UIAnimation>().Toggle();
//                } else {
//                    b.GetComponent<UIAnimation>().Untoggle();
//                }
//            }
//        }

//        private SpriteTexture GetSpriteTextureFromExisting(SpriteTexture tex)
//        {
//            Dictionary<string, SpriteTexture> dict = AssetManager.GetDictionary<string, SpriteTexture>(this);
//            foreach (SpriteTexture spriteTex in dict.Values) {
//                if (spriteTex.Texture == tex.Texture && spriteTex.sourceRectangle == tex.sourceRectangle) {
//                    return spriteTex;
//                }
//            }
//            return null;
//        }

//    }

//}
//#endif