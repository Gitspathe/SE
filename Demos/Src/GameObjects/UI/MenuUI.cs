using Microsoft.Xna.Framework;
using SE.Core;
using SE.Editor.UI;
using SE.UI;
using Button = SE.UI.Button;
using Vector2 = System.Numerics.Vector2;

namespace SEDemos.GameObjects.UI
{

    public class MenuUI : UIObject
    {
        public override bool IsRootUIMenu => true;

        public override string RootUIName => "Menu";

        public override int RootUIPriority => 2;

        public override bool DestroyOnLoad => true;

        public MenuUI() : base(Vector2.Zero)
        {
            if (Screen.IsFullHeadless)
                return;

            Button networkTestBtn = EditorTheme.CreateButton(new Vector2(200, 100), new Point(200, 100),
                "Network Test", Transform, EditorTheme.ColorSet.Blue);

            Button tilemapTestBtn = EditorTheme.CreateButton(new Vector2(200, 220), new Point(200, 100),
                "Tilemap Test", Transform, EditorTheme.ColorSet.Blue);

            networkTestBtn.Clicked += (sender, args) => SceneManager.SetCurrentScene("_DEMOS\\networktest");
            tilemapTestBtn.Clicked += (sender, args) => SceneManager.SetCurrentScene("_DEMOS\\tilemaptest");
        }

    }

}
