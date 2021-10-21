using System;
using Console = SE.Core.Console;

namespace SE.Debug.Commands
{
    internal class ToggleNavigation : ICommand
    {
        public string Execute(string[] parameters)
        {
            Console.ToggleNavGrid();
            return null;
        }

        public string GetError(Exception e, string[] parameters)
        {
            return "Unknown error occured.";
        }

        public string GetHelp()
        {
            return GetSyntax() + "\n" +
                   " >TOGGLENAVIGATION - Toggles debug navigation grid.";
        }

        public string GetName()
        {
            return "togglenavigation";
        }

        public string GetSyntax()
        {
            return "command TOGGLENAVIGATION syntax: no parameters";
        }
    }
}
