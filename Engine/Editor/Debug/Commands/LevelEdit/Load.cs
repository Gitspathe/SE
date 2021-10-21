#if EDITOR
using SE.Core;
using SE.Debug.Commands;
using System;

namespace SE.Editor.Debug.Commands.LevelEdit
{
    internal class Load : ICommand
    {
        public string Execute(string[] parameters)
        {
            if (parameters.Length < 2)
                return GetSyntax();

            string nameSpace, name;
            //try {
            nameSpace = parameters[0];
            name = parameters[1];
            if (!SceneManager.SceneExists(nameSpace, name)) {
                return "Level '" + name + "' does not exist.";
            } else {
                SceneManager.SetCurrentScene(nameSpace + "\\" + name);
                return "Loaded level: " + name + ".dat.";
            }
            //} catch (Exception e) {
            //    return GetError(e, parameters);
            //}
        }

        public string GetHelp()
        {
            return GetSyntax() + "\n" +
                   " >LEVELEDIT.LOAD - Loads a level.\n" +
                   "    NAMESPACE: Namespace of the level to load." +
                   "    NAME: Name of the level to load.";
        }

        public string GetError(Exception e, string[] parameters)
        {
            return e.GetType() + ": " + e.Message;
        }

        public string GetName()
        {
            return "leveledit.load";
        }

        public string GetSyntax()
        {
            return "command LEVELEDIT.LOAD syntax: (NAMESPACE, NAME)";
        }
    }
}
#endif