using System.Numerics;
using SE.Common;
using SE.World;
using SEDemos.GameObjects;
using SEDemos.GameObjects.UI;

namespace SEDemos.Levels
{
    public class NetworkTest : SceneScript
    {
        public override string LevelName => "networktest";

        public override string LevelNamespace => "_DEMOS";

        //private UnloadTestObj testObj;

        public override void AfterSceneLoad()
        {
            UnloadTestObj testObj = (UnloadTestObj)GameObject.Instantiate(typeof(UnloadTestObj), Vector2.Zero, 0f, Vector2.One);
            testObj.Transform.Position = new Vector2(400, 400);
            NetworkTestMenu menu = new NetworkTestMenu();
        }
    }

}
