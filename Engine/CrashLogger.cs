﻿using SE.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Console = SE.Core.Console;
using Random = SE.Utility.Random;

namespace SE
{
    public static class CrashLogger
    {
        private static bool unhandledExceptionEventSetup;

        private static string[] crashMessages = {
            "You just got pranked.",
            "Whoops. Sorry dude.",
            "What else did you expect from a program called \"Spaghetti Engine\"?",
            "That wasn't supposed to happen.",
            "Illegal user input detected: \"0w0\". Terminating.",
            "At least the crash logging works, right?"
        };

        public static void HookUnhandledExceptionEvents()
        {
            if (unhandledExceptionEventSetup)
                return;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        // TODO: Write crash logs instead of just writing to LOG.txt.
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try {
                WriteCrashLog((e.ExceptionObject as Exception));
            } catch (Exception) { /* ignored */ }
        }

        private static void WriteCrashLog(Exception exception)
        {
            DateTime now = DateTime.Now;
            CultureInfo culture = new CultureInfo("en-US");
            char sep = Path.DirectorySeparatorChar;
            string crashLogDirectory = FileIO.BaseDirectory + "CrashLogs";
            if (!Directory.Exists(crashLogDirectory)) {
                Directory.CreateDirectory(crashLogDirectory);
            }

            string date = now.ToString(culture)
               .Replace('/', '-')
               .Replace(':', '-');

            string crashFile = crashLogDirectory + sep + date + ".txt";
            Console.WriteLine($"\n\nFatal error caused the application to crash. Check \"{crashFile}\" for more information.");
            using (StreamWriter sw = new StreamWriter(crashFile, false)) {
                sw.WriteLine("---------------CRASH LOG---------------");
                try {
                    sw.WriteLine($"Framework:            {RuntimeInformation.FrameworkDescription}");
                    sw.WriteLine($"Operating System:     {RuntimeInformation.OSDescription}");
                    sw.WriteLine($"OS Architecture:      {RuntimeInformation.OSArchitecture}");
                    sw.WriteLine($"Process Architecture: {RuntimeInformation.ProcessArchitecture}");
                } catch (Exception) {
                    sw.WriteLine("Failed to retrieve system information.");
                }

                sw.WriteLine("\nUnhandled exception:");
                sw.WriteLine(exception.ToString());
                sw.WriteLine("---------------------------------------");
                sw.WriteLine(crashMessages[Random.Next(0, crashMessages.Length)]);
                sw.Close();
            }
        }

    }
}
