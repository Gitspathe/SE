#if EDITOR
using Microsoft.Xna.Framework;
using SE.Core;
using SE.Debug.Commands;
using SE.World;
using System;

namespace SE.Editor.Debug.Commands.LevelEdit
{
    internal class New : ICommand
    {
        public string Execute(string[] parameters)
        {
            if (parameters.Length < 4)
                return GetSyntax();

            int x, y;
            string nameSpace, name;
            Scene scene;
            try {
                x = Convert.ToInt32(parameters[2]);
                y = Convert.ToInt32(parameters[3]);

                nameSpace = parameters[0].ToLower();
                name = parameters[1].ToLower();
                scene = new Scene {
                    LevelName = name,
                    LevelNamespace = nameSpace
                };
                scene.CreateLevel(new Point(x, y));
                SceneManager.SetCurrentScene(scene);
                SceneManager.CurrentScene.LevelName = name;
                SceneManager.CurrentScene.LevelNamespace = nameSpace;
            } catch (Exception e) {
                return GetError(e, parameters);
            }
            return "Created and loaded a new level.";
        }

        public string GetHelp()
        {
            return GetSyntax() + "\n" +
                   " >LEVELEDIT.NEW - Creates and loads a new level.\n" +
                   "    NAMESPACE: Where the level is located.\n" +
                   "    NAME: Name the level will use.\n" +
                   "    X: Level size in the horizontal axis.\n" +
                   "    Y: Level size in the vertical axis.";
        }

        public string GetError(Exception e, string[] parameters)
        {
            return e.GetType() + ": " + e.Message;
        }

        public string GetName()
        {
            return "leveledit.new";
        }

        public string GetSyntax()
        {
            return "command LEVELEDIT.NEW syntax: (NAMESPACE, NAME,X,Y)";
        }
    }
}
#endif