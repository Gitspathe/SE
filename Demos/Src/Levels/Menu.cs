using SE.World;
using SEDemos.GameObjects.UI;

namespace SEDemos.Levels
{
    public class Menu : SceneScript
    {
        public override string LevelName => "menu";
        public override string LevelNamespace => "_DEMOS";

        public override void AfterSceneLoad()
        {
            MenuUI menu = new MenuUI();
        }
    }
}
