using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement;
using SE.Common;
using SE.Core;
using SE.Rendering;
using SE.World;
using SEDemos.GameObjects;
using SEDemos.GameObjects.UI;
using Vector2 = System.Numerics.Vector2;

namespace SEDemos.Levels
{
    public class TilemapTest : SceneScript
    {
        public override string LevelName => "tilemaptest";

        public override string LevelNamespace => "_DEMOS";

        public AssetConsumerContext assetConsumer = new AssetConsumerContext();

        //private UnloadTestObj testObj;

        public override void AfterSceneLoad()
        {
            UnloadTestObj testObj = (UnloadTestObj)GameObject.Instantiate(typeof(UnloadTestObj), Vector2.Zero, 0f, Vector2.One);
            testObj.Transform.Position = new Vector2(400, 400);
            NetworkTestMenu menu = new NetworkTestMenu();

            Material test = new Material(AssetManager.Get<Texture2D>(assetConsumer, "grasstex"));

            GameObject tilemapGo = new GameObject();
            TileMap map = (TileMap)tilemapGo.AddComponent(new TileMap());
            map.TileSet.Add(new GenericTileProvider(test, new Rectangle(0, 0, 64, 64)));

            for (int x = 0; x < 55; x++) {
                for (int y = 0; y < 45; y++) {
                    map.PlaceTile(0, new Point(x, y), 0);
                }
            }
        }
    }

}
