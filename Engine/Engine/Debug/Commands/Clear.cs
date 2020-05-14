using System;
using Console = SE.Core.Console;

namespace SE.Debug.Commands
{
    internal class Clear : ICommand
    {
        public string Execute(string[] parameters)
        {
            Console.Clear();
            return null;
        }

        public string GetError(Exception e, string[] parameters)
        {
            return "Unknown error occured.";
        }

        public string GetHelp()
        {
            return GetSyntax() + "\n" +
                   " >CLEAR - Clears the console.";
        }

        public string GetName()
        {
            return "clear";
        }

        public string GetSyntax()
        {
            return "command CLEAR syntax: no parameters.";
        }
    }
}
