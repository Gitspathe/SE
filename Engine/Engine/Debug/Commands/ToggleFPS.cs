using System;
using Console = SE.Core.Console;

namespace SE.Debug.Commands
{
    internal class ToggleFPS : ICommand
    {
        public string Execute(string[] parameters)
        {
            Console.ToggleFps();
            return null;
        }

        public string GetError(Exception e, string[] parameters)
        {
            return "Unknown error occured.";
        }

        public string GetHelp()
        {
            return GetSyntax() + "\n" +
                   " >TOGGLEFPS - Toggles FPS counter.";
        }

        public string GetName()
        {
            return "togglefps";
        }

        public string GetSyntax()
        {
            return "command TOGGLEFPS syntax: no parameters";
        }
    }
}
