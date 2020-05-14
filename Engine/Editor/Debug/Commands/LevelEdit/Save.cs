#if EDITOR
using System;
using System.IO;
using SE.Core;
using SE.Debug.Commands;
using SE.Core.Extensions;
using static SE.Core.FileIO;

namespace SE.Editor.Debug.Commands.LevelEdit
{
    internal class Save : ICommand
    {
        public string Execute(string[] parameters)
        {
            string nameSpace, name, data;
            try {
                nameSpace = SceneManager.CurrentScene.LevelNamespace;
                name = SceneManager.CurrentScene.LevelName;
                data = SceneManager.CurrentScene.MakeData().Serialize();
                SaveFile(data, Path.Combine(nameSpace, "Levels", name + ".dzmap"));
            } catch(Exception e) {
                return GetError(e, parameters);
            }
            return "Saved level to " + Path.Combine(DataDirectory, nameSpace, "Levels", name + ".dzmap") + ".";
        }

        public string GetHelp()
        {
            return GetSyntax() + "\n" +
                   " >LEVELEDIT.SAVE - Saves the current level into into the filesystem.";
        }

        public string GetError(Exception e, string[] parameters)
        {
            return e.GetType() + ": " + e.Message;
        }

        public string GetName()
        {
            return "leveledit.save";
        }

        public string GetSyntax()
        {
            return "command LEVELEDIT.SAVE syntax: no parameters.";
        }
    }
}
#endif