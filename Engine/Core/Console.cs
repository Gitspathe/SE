#if EDITOR
#endif
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SE.AssetManagement;
using SE.Components;
using SE.Debug;
using SE.Debug.Commands;
using SE.Editor.Debug.Commands;
using SE.Editor.Debug.Commands.LevelEdit;
using SE.Rendering;

//using SE.Engine.Physics;

namespace SE.Core
{

    public static class Console
    {
        /// <summary>If true, all exceptions passed to the console are unhandled and terminate the program.</summary>
        public static bool UnhandledExceptions = true;
        public static bool PrintStackTrace = false;

        /// <summary>Logging level. Higher levels may result in degraded performance, but is useful for debugging.</summary>
        public static LoggingLevel LoggingLevel = LoggingLevel.All;

        public static SpriteFont DebugFont { get; private set; }
        public static SpriteFont ConsoleFont { get; private set; }

        private static bool active;

        private static bool showfps = true;
        public static bool ShowFPS {
            get => showfps;
            set {
                showfps = value;
                statsUI.SetActive(value);
            }
        }

        private static StreamWriter outputWriter;

        private static bool showGrid = false;
        private static bool showNavGrid = false;
        private static bool debugNetwork = true;

        private static List<string> outputLines = new List<string>();
        private static Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>();

        private static SpriteTexture spriteTex;

        private static FPSCounter fpsCounter = new FPSCounter();
        private static StatsUI statsUI;

        private static ConsoleUI consoleUI;

        private static List<string> commandLineBuffer = new List<string>(10);
        private static int bufferIndex;
        private static List<string> tmpStrings = new List<string>(100);
        private static bool initialized;

        private static Dictionary<ConsoleColor, Color> colorTranslationTable = new Dictionary<ConsoleColor, Color> {
            {ConsoleColor.Red, Color.Red},
            {ConsoleColor.DarkRed, Color.DarkRed},
            {ConsoleColor.White, Color.White},
            {ConsoleColor.Yellow, Color.Yellow},
            {ConsoleColor.DarkYellow, Color.YellowGreen},
            {ConsoleColor.Blue, Color.Blue},
            {ConsoleColor.DarkBlue, Color.DarkBlue},
            {ConsoleColor.Magenta, Color.Magenta},
            {ConsoleColor.DarkMagenta, Color.DarkMagenta},
            {ConsoleColor.Cyan, Color.Cyan},
            {ConsoleColor.DarkCyan, Color.DarkCyan},
            {ConsoleColor.Gray, Color.Gray},
            {ConsoleColor.DarkGray, Color.DarkGray},
            {ConsoleColor.Black, Color.Black},
            {ConsoleColor.Green, Color.Green},
            {ConsoleColor.DarkGreen, Color.DarkGreen}
        };

        private static AssetConsumerContext assetConsumerContext = new AssetConsumerContext();
        public static string LogPath { get; } = FileIO.DataDirectory + "\\LOG.txt";

        static Console()
        {
            AddCommand(new Teleport());
            AddCommand(new Clear());
            AddCommand(new ToggleFPS());
            AddCommand(new ToggleGrid());
            AddCommand(new ToggleNavigation());
        #if EDITOR
            AddCommand(new New());
            AddCommand(new Save());
            AddCommand(new Fill());
            AddCommand(new Load());
            AddCommand(new ToggleLighting());
            AddCommand(new DebugLighting());
        #endif
        }

        public static void Initialize()
        {
            IAssetConsumer consumer;
            if (!Screen.IsFullHeadless) {
                DebugFont = AssetManager.Get<SpriteFont>(assetConsumerContext, "editor_subheading");
                ConsoleFont = AssetManager.Get<SpriteFont>(assetConsumerContext, "editor");

                consoleUI = new ConsoleUI();
                consoleUI.TextInputField.Confirmed += (sender, value) => {
                    Parse(value);
                };
            }

            if (!Screen.IsFullHeadless) {
                if (Screen.DisplayMode != DisplayMode.Normal) {
                    consoleUI.Enable();
                } else {
                    consoleUI.Disable(true);
                }
            }

            try {
                outputWriter = new StreamWriter(LogPath) {
                    AutoFlush = true
                };
            } catch(IOException) { } 

            initialized = true;
        }

        public static void InitializeStats(SpriteBatch spriteBatch)
        {
            statsUI = new StatsUI(spriteBatch, fpsCounter);
        }

        public static void Update()
        {
            if (showfps) {
                fpsCounter.Update();
            }
            if (InputManager.KeyCodePressed(Keys.F1) && Screen.DisplayMode == DisplayMode.Normal) {
                if (active) {
                    Close();
                } else {
                    Open();
                }
            }

            if (active) {
                if (InputManager.KeyCodePressed(Keys.Up)) {
                    bufferIndex += 1;
                    UpdateBuffer();
                }
                if (InputManager.KeyCodePressed(Keys.Down)) {
                    bufferIndex -= 1;
                    UpdateBuffer();
                }
            }
        }

        private static void UpdateBuffer()
        {
            bufferIndex = MathHelper.Clamp(bufferIndex, -1, commandLineBuffer.Count - 1);
            if (bufferIndex == -1) {
                consoleUI.TextInputField.SetValue("");
            } else {
                consoleUI.TextInputField.SetValue(commandLineBuffer[bufferIndex]);
            }
        }

        public static void AddCommand(ICommand command)
        {
            commands.Add(command.GetName(), command);
        }

        private static void Parse(string _input)
        {
            if (!commandLineBuffer.Contains(_input)) {
                commandLineBuffer.Insert(0, _input);
                if (commandLineBuffer.Count > 11) {
                    commandLineBuffer.RemoveAt(10);
                }
            }
            bufferIndex = -1;

            int parametersStart = -1;
            int parametersEnd = -1;
            string command = "";
            string[] parameters = new string[0];
            bool hasParams = false;
            char lastChar = ' ';
            try {
                lastChar = _input[_input.Length-1];
                if (lastChar == '?') {
                    _input = _input.Replace("?", "");
                }

                if(_input.Contains("(") && _input.Contains(")")) {
                    parametersStart = _input.IndexOf("(");
                    parametersEnd = _input.IndexOf(")");
                    if(parametersEnd - parametersStart > 1) {
                        hasParams = true;
                    }
                }

                if (hasParams) {
                    command = _input.Substring(0, parametersStart).ToLower();
                    string parameterBody = _input.Substring(parametersStart+1, parametersEnd-parametersStart-1).Replace(" ", "");
                    parameters = parameterBody.Split(',');
                } else {
                    command = _input.Replace("(", "").Replace(")", "").Replace(" ", "").ToLower();
                }
            } catch (Exception e) {
                WriteLine(e.Message);
            } finally {
                if (commands.TryGetValue(command, out ICommand c)) {
                    WriteLine(lastChar == '?' ? c.GetHelp() : c.Execute(parameters));
                }
            }
        }

        private static void WriteLine(string output, ConsoleColor? color = null)
        {
            ConsoleColor c = color ?? ConsoleColor.White;
            if (string.IsNullOrEmpty(output))
                return;

            tmpStrings.Clear();
            SplitLines(tmpStrings, output, 160);
            if (!Screen.IsFullHeadless && consoleUI != null && initialized) {
                foreach (string line in tmpStrings) {
                    consoleUI.Output(line, ConvertConsoleColor(c));
                }
            } else {
                System.Console.ForegroundColor = c;
                foreach (string line in tmpStrings) {
                    System.Console.WriteLine(line);
                }
                System.Console.ForegroundColor = ConsoleColor.White;
            }

            outputWriter?.WriteLine(output);
        }

        private static string GetExceptionInfo(Exception e)
        {
            if(PrintStackTrace) {
                return e.ToString();
            }
            string msg = e.Message;
            int recursion = 0;
            while (e.InnerException != null && recursion <= 5) {
                msg += "\n  -->" + e.InnerException.Message;
                e = e.InnerException;
                recursion++;
            }
            return msg;
        }

        public static void WriteLine(object output, ConsoleColor? color = null)
        {
            WriteLine(output.ToString(), color);
        }

        public static void LogError(Exception e, LogSource source = LogSource.Default)
        {
            if (UnhandledExceptions && source == LogSource.Network)
                throw e;
            if (LoggingLevel > LoggingLevel.Error)
                return;

            WriteLine(GetExceptionInfo(e), ConsoleColor.Red);
        }

        public static void LogError(string output, LogSource source = LogSource.Default)
        {
            if (UnhandledExceptions && source == LogSource.Network)
                throw new Exception(output);
            if (LoggingLevel > LoggingLevel.Error)
                return;

            WriteLine(output, ConsoleColor.Red);
        }

        public static void LogWarning(Exception e, LogSource source = LogSource.Default)
        {
            if (LoggingLevel > LoggingLevel.Warning)
                return;

            WriteLine(GetExceptionInfo(e), ConsoleColor.Yellow);
        }

        public static void LogWarning(string output, LogSource source = LogSource.Default)
        {
            if (LoggingLevel > LoggingLevel.Warning)
                return;

            WriteLine(output, ConsoleColor.Yellow);
        }

        public static void LogInfo(string output, bool important = false, LogSource source = LogSource.Default)
        {
            if (LoggingLevel > LoggingLevel.Info)
                return;

            ConsoleColor c = important ? ConsoleColor.Magenta : ConsoleColor.Cyan;
            WriteLine(output, c);
        }

        private static void Open()
        {
            active = true;
            if (!consoleUI.Enabled) {
                consoleUI.Enable();
            }
        }

        private static void Close()
        {
            active = false;
            if (consoleUI.Enabled) {
                consoleUI.Disable(true);
            }
        }

        public static void Clear()
        {
            outputLines.Clear();
        }

        public static void ToggleFps()
        {
            ShowFPS = !ShowFPS;
        }

        public static void ToggleGrid()
        {
            showGrid = !showGrid;
        }

        public static void ToggleNavGrid()
        {
            showNavGrid = !showNavGrid;
        }

        public static void DrawOverlay(Camera2D camera, SpriteBatch spriteBatch)
        {
            if (showNavGrid) {
                //NavigationManager.DebugDraw(camera, spriteBatch);
            }
            if (showfps) {
                statsUI.Draw();
            }
        }

        public static void DrawUnderlay(Camera2D camera, SpriteBatch spriteBatch)
        {
            Rendering.ChangeDrawCall(SpriteSortMode.Deferred, camera.ScaleMatrix);

            // Initialize the sprite texture if it's null.
            if (spriteTex.Texture == null)
                spriteTex = AssetManager.Get<SpriteTexture>(assetConsumerContext, "floor");

            if (showNavGrid) {
                //NavigationManager.DebugDraw(camera, spriteBatch);
            }
            if (showGrid) {
                SpatialPartitionManager.DrawBoundingRectangle(camera);
            }
            Rendering.EndDrawCall();
        }

        /// <summary>
        /// Converts a System.ConsoleColor into a MonoGame Color.
        /// </summary>
        /// <param name="color">ConsoleColor to convert.</param>
        /// <returns>MonoGame Color.</returns>
        public static Color ConvertConsoleColor(ConsoleColor color)
        {
            return colorTranslationTable.TryGetValue(color, out Color newColor) ? newColor : Color.White;
        }

        private static void SplitLines(List<string> existingList, string str, int maxChunkSize)
        {
            string[] tmp = str.Split("\n");
            for (int i = 0; i < tmp.Length; i++) {
                for (int ii = 0; ii < tmp[i].Length; ii += maxChunkSize) {
                    existingList.Add(tmp[i].Substring(ii, Math.Min(maxChunkSize, tmp[i].Length - ii)));
                }
            }
        }
    }

    public enum LoggingLevel
    {
        All,
        Info,
        Warning,
        Error,
        None
    }
}
