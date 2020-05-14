using System;
using Vector2 = System.Numerics.Vector2;

namespace SE.Debug.Commands
{
    internal class Teleport : ICommand
    {
        public string Execute(string[] parameters)
        {
            if (parameters == null || parameters.Length < 1)
                return "Command invalid. " + GetSyntax();

            int x, y;
            try {
                x = Convert.ToInt32(parameters[0]);
                y = Convert.ToInt32(parameters[1]);
                GameEngine.Engine.Player.Transform.Position = new Vector2(x,y);
            } catch (Exception e) {
                return GetError(e, parameters);
            }
            return "Teleported player to X: "+x+", Y: "+y;
        }

        public string GetError(Exception e, string[] parameters)
        {
            return e.GetType() + ": " + e.Message;
        }

        public string GetHelp()
        {
            return GetSyntax() + "\n" +
                   " >TELEPORT - Teleports the player to given coordinates.\n" +
                   "    X = Position in horizontal axis.\n" +
                   "    Y = Position in vertical axis.";
        }

        public string GetName()
        {
            return "teleport";
        }

        public string GetSyntax()
        {
            return "TELEPORT Syntax: (X,Y)";
        }
    }
}
