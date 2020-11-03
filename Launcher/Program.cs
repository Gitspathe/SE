using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using SE;
using SE.Core;
using Console = System.Console;

namespace SELauncher
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            CrashLogger.HookUnhandledExceptionEvents();
            using (Game game = new Game(args)) {
                if (!Screen.IsFullHeadless) {
                    try {
                        FreeConsole();
                    } catch (Exception) {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Failed to close console window. This is expected behaviour on non-Windows operating systems.");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                game.Run();
            }
        }
    }

}