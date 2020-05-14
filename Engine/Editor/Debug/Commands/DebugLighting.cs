#if EDITOR
using System;
using SE.Debug.Commands;

namespace SE.Editor.Debug.Commands
{
    public class DebugLighting : ICommand
    {
        public string Execute(string[] parameters)
        {
            Core.Lighting.Debug = !Core.Lighting.Debug;
            return "Toggled lighting debugging.";
        }

        public string GetHelp()
        {
            return GetSyntax() + "\n" +
                   " >DEBUGLIGHTING - Toggles lighting debugging on or off.";
        }

        public string GetError(Exception e, string[] parameters)
        {
            return e.GetType() + ": " + e.Message;
        }

        public string GetName()
        {
            return "debuglighting";
        }

        public string GetSyntax()
        {
            return "command DEBUGLIGHTING syntax: NO PARAMETERS";
        }
    }
}
#endif