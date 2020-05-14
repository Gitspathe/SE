using System;
using Console = SE.Core.Console;

namespace SE.Debug.Commands
{
    internal class ToggleGrid : ICommand
    {
        public string Execute(string[] parameters)
        {
            Console.ToggleGrid();
            return null;
        }

        public string GetError(Exception e, string[] parameters)
        {
            return "Unknown error occured.";
        }

        public string GetHelp()
        {
            return GetSyntax() + "\n" +
                   " >TOGGLEGRID - Toggles debug spatial partitioning grid.";
        }

        public string GetName()
        {
            return "togglegrid";
        }

        public string GetSyntax()
        {
            return "command TOGGLEGRID syntax: no parameters";
        }
    }
}
