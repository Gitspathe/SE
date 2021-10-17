#if EDITOR
using SE.Debug.Commands;
using System;

namespace SE.Editor.Debug.Commands.LevelEdit
{
    internal class Fill : ICommand
    {
        public string Execute(string[] parameters)
        {
            if (parameters.Length < 1)
                return GetSyntax();

            int id;
            try {
                id = Convert.ToInt32(parameters[0]);
                //LevelEditor.Fill(id);
            } catch (Exception e) {
                return GetError(e, parameters);
            }
            return "Filled level with tiles of ID: " + id.ToString() + ".";
        }

        public string GetHelp()
        {
            return GetSyntax() + "\n" +
                   " >LEVELEDIT.FILL - Fills the map with a specified tile type.\n" +
                   "    TILE_ID: Which tile the map will be filled with.";
        }

        public string GetError(Exception e, string[] parameters)
        {
            return e.GetType() + ": " + e.Message;
        }

        public string GetName()
        {
            return "leveledit.fill";
        }

        public string GetSyntax()
        {
            return "command LEVELEDIT.FILL syntax: (TILE_ID)";
        }
    }
}
#endif