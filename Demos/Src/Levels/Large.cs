using SE.World;
using SEDemos.GameObjects.UI;

namespace SEDemos.Levels
{

    public class Large : SceneScript
    {
        public override string LevelName => "large";
        public override string LevelNamespace => "_DEMOS";

        public override void AfterSceneLoad()
        {
            NetworkTestMenu menu = new NetworkTestMenu();
        }
    }

}
