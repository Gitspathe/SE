using System.IO;
using Microsoft.Xna.Framework;
using SE.World;

namespace SE.Core
{
    public static class SceneManager
    {
        /// <summary>Level tile size in pixels.</summary>
        public const int _TILE_SIZE = 64;

        /// <summary>The currently loaded level.</summary>
        public static Scene CurrentScene { get; private set; }

        /// <summary>World size in X and Y axis.</summary>
        public static Point WorldSize { get; internal set; }

        /// <summary>
        /// Changes the current level.
        /// </summary>
        /// <param name="sceneName">Level to load. If a key for the levelName exists within the levels dictionary, the level will be
        /// loaded with that level's Type. Otherwise, it will be loaded with default behaviour.</param>
        public static void SetCurrentScene(string sceneName)
        {
            LoadingScreen.Initialize();
            string[] str = sceneName.Split('\\');
            Scene scene = new Scene {LevelNamespace = str[0], LevelName = str[1]};
            SetCurrentScene(scene);
        }

        /// <summary>
        /// Changes the current level.
        /// </summary>
        /// <param name="scene">Level to load.</param>
        public static void SetCurrentScene(Scene scene)
        {
            LoadingScreen.SetProgress("Unloading current level...");
            if (CurrentScene != null) {
                CurrentScene.Unload();
                CurrentScene = null;
            }

            CurrentScene = scene;
            CurrentScene.Load();
            LoadingScreen.SetProgress("Finalizing...");
            GameEngine.RequestGC();
            LoadingScreen.Finished();
        }

        /// <summary>
        /// Checks if a level of a specific name exists in the filesystem.
        /// </summary>
        /// <param name="nameSpace">Namespace the level is located in.</param>
        /// <param name="sceneName">Level name to check.</param>
        /// <returns>True if the level is found.</returns>
        public static bool SceneExists(string nameSpace, string sceneName) 
            => FileIO.FileExists(Path.Combine(nameSpace, "Levels", sceneName + ".semap"));

        internal static void Update()
        {
            CurrentScene?.Update();
        }
    }
}
