#if EDITOR
using SE.Debug.Commands;
using System;

namespace SE.Editor.Debug.Commands
{
    public class ToggleLighting : ICommand
    {
        public string Execute(string[] parameters)
        {
            Core.Lighting.Enabled = !Core.Lighting.Enabled;
            return "Toggled lighting.";
        }

        public string GetHelp()
        {
            return GetSyntax() + "\n" +
                   " >TOGGLELIGHTING - Toggles lighting on or off.";
        }

        public string GetError(Exception e, string[] parameters)
        {
            return e.GetType() + ": " + e.Message;
        }

        public string GetName()
        {
            return "togglelighting";
        }

        public string GetSyntax()
        {
            return "command TOGGLELIGHTING syntax: NO PARAMETERS";
        }
    }

}
#endif