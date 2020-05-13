using DeeZ;
using DeeZ.Core;
using DeeZ.Editor.UI;
using DeeZ.Engine.UI;
using Microsoft.Xna.Framework;
using Button = DeeZ.Engine.UI.Button;
using Vector2 = System.Numerics.Vector2;

namespace DeeZEngine_Demos.GameObjects.UI
{

    public class MenuUI : UIObject
    {
        public override bool IsRootUIMenu => true;

        public override string RootUIName => "Menu";

        public override int RootUIPriority => 2;

        public override bool DestroyOnLoad => true;

        public MenuUI() : base(Vector2.Zero)
        {
            if (GameEngine.IsFullHeadless)
                return;

            Button networkTestBtn = EditorTheme.CreateButton(new Vector2(200, 100), new Point(200, 100), 
                "Network Test", Transform, EditorTheme.ColorSet.Blue);
            networkTestBtn.Clicked += (sender, args) => LevelManager.SetCurrentLevel("_DEMOS\\networktest");
        }

    }

}
