//#if EDITOR
//using DeeZ.Core;
//using DeeZ.Editor.Debug.GameObjects;
//using DeeZ.Editor.UI;
//using DeeZ.Engine.Common;
//using DeeZ.Core.Extensions;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Input;
//using static DeeZ.Core.LevelManager;
//using Vector2 = System.Numerics.Vector2;

//namespace DeeZ.Editor
//{

//    internal static class LevelEditor
//    {
//        public static int selectedTileID = -1;

//        private static bool tilesChanged;
//        private static LevelEditorUI userInterface;
//        private static GameObject cursor;

//        static LevelEditor()
//        {
//            cursor = new Cursor(Vector2.Zero, 0, Vector2.One);
//            userInterface = new LevelEditorUI();
//            userInterface.Disable(true);
//        }

//        public static void Update()
//        {
//            MouseState mouseState = InputManager.MouseState;
//            if (InputManager.KeyCodePressed(Keys.Tab))
//            {
//                if (userInterface.Enabled)
//                {
//                    userInterface.Disable(true);
//                }
//                else
//                {
//                    userInterface.Enable(true);
//                }
//            }

//            if (!userInterface.Enabled)
//            {
//                cursor.Transform.Position = new Vector2(((int)Screen.WorldMousePoint.X).Round(_TILE_SIZE), ((int)Screen.WorldMousePoint.Y).Round(_TILE_SIZE));
//                if (mouseState.LeftButton == ButtonState.Pressed)
//                {
//                    if (selectedTileID == -1 || UIManager.MouseBlocked)
//                        return;

//                    Point pos = new Point((int)(cursor.Transform.globalPosition.X / _TILE_SIZE), (int)(cursor.Transform.globalPosition.Y / _TILE_SIZE));
//                    CurrentLevel.PlaceTile(pos, selectedTileID);
//                    tilesChanged = true;
//                }
//                if (mouseState.RightButton == ButtonState.Pressed)
//                {
//                    Point pos = new Point((int)(cursor.Transform.globalPosition.X / _TILE_SIZE), (int)(cursor.Transform.globalPosition.Y / _TILE_SIZE));
//                    CurrentLevel.RemoveTilesAtPoint(pos);
//                    tilesChanged = true;
//                }
//                if (mouseState.LeftButton == ButtonState.Released && mouseState.RightButton == ButtonState.Released && tilesChanged)
//                {
//                    CurrentLevel.GenerateShadows();
//                    tilesChanged = false;
//                }
//            }
//        }

//        public static void Fill(int tileId)
//        {
//            CurrentLevel.updateShadows = false;
//            for (int x = 0; x < CurrentLevel.mapData.Length; x++)
//            {
//                for (int y = 0; y < CurrentLevel.mapData[x].Length; y++)
//                {
//                    CurrentLevel.PlaceTile(new Point(x, y), tileId);
//                }
//            }
//            CurrentLevel.updateShadows = true;
//            CurrentLevel.GenerateShadows();
//        }

//    }

//}
//#endif