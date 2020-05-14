using System;

namespace SE.Debug.Commands
{
    public interface ICommand
    {
        string Execute(string[] parameters);
        string GetName();
        string GetSyntax();
        string GetError(Exception e, string[] parameters);
        string GetHelp();
    }
}
