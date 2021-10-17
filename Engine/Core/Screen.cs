using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using MGVector2 = Microsoft.Xna.Framework.Vector3;
using Vector2 = System.Numerics.Vector2;

namespace SE.Core
{
    /// <summary>
    /// Static class which handles screen logic.
    /// </summary>
    public static class Screen
    {
        //internal const int _BASE_RES_X = 1920;
        //internal const int _BASE_RES_Y = 1080;

        public static int SizeX { get; private set; } = 1920;
        public static int SizeY { get; private set; } = 1080;

        internal static Matrix ScreenScaleMatrix;
        internal static float SizeRatio = 1.0f;

        /// <summary>Where the mouse pointer is located.</summary>
        private static Vector2 screenMousePoint = Vector2.Zero;

        private static GraphicsDeviceManager graphics;

        /// <value>Gets the mouse point in screen coordinates.
        ///        Returns null if the mouse isn't within the game window.</value>
        public static Vector2? MousePoint => MouseInWindow ? (Vector2?)screenMousePoint : null;

        /// <summary>Where the viewport is located in the editor. Valid if running through the editor.</summary>
        public static Rectangle EditorViewBounds { get; set; }

        /// <summary>Viewport size.</summary>
        public static Vector2 ViewSize => new Vector2(ViewBounds.Width, ViewBounds.Height);

        /// <summary>Viewport bounds.</summary>
        internal static Rectangle ViewBounds { get; private set; } = Rectangle.Empty;

        /// <summary>True if the mouse is within the game window.</summary>
        public static bool MouseInWindow { get; private set; }

        public static DisplayMode DisplayMode { get; internal set; } = DisplayMode.Normal;

        // TODO: Tidy this up. Maybe have stuff like Capabilities.HasGPU, etc. Then I can have headless with GPU but no window.
        public static bool IsFullHeadless => DisplayMode == DisplayMode.Decapitated;

        public static void SetScreenSize(int sizeX, int sizeY)
        {
            if (SizeX < 1 || SizeY < 1)
                throw new InvalidOperationException();

            SizeX = sizeX;
            SizeY = sizeY;
            Reset();
        }

        internal static void CalculateScreenScale()
        {
            ScreenScaleMatrix = Matrix.CreateScale(new MGVector2(SizeRatio, SizeRatio, 1));
        }

        internal static void Update()
        {
            MouseState mouseState = InputManager.MouseState;

            // Calculate the mouse point.
            if (GameEngine.IsEditor) {
                Vector2 scale = new Vector2((float)EditorViewBounds.Width / SizeX, (float)EditorViewBounds.Height / SizeY);
                Vector2 mousePos = new Vector2(
                    (mouseState.X / SizeRatio - EditorViewBounds.X) / scale.X,
                    (mouseState.Y / SizeRatio - EditorViewBounds.Y) / scale.Y);

                screenMousePoint.X = mousePos.X;
                screenMousePoint.Y = mousePos.Y;
            } else {
                screenMousePoint.X = mouseState.X / SizeRatio;
                screenMousePoint.Y = mouseState.Y / SizeRatio;
            }

            // Determine whether or not the mouse point is contained within the game window.
            if (screenMousePoint.X < 0 || screenMousePoint.X > SizeX || screenMousePoint.Y < 0 || screenMousePoint.Y > SizeY)
                MouseInWindow = false;
            else
                MouseInWindow = true;

            CalculateScreenScale();

            ViewBounds = new Rectangle(0, 0,
                Convert.ToInt32(graphics.PreferredBackBufferWidth),
                Convert.ToInt32(graphics.PreferredBackBufferHeight));
        }

        public static void Reset()
        {
            if (IsFullHeadless)
                return;

            // Have to go into full screen, then exit, to fix weird .net core issue.
            // TODO: Proper supports for switching between windowed, borderless fullscreen, and fullscreen.
            graphics = Rendering.GraphicsDeviceManager;
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = (int)(SizeX * SizeRatio);
            graphics.PreferredBackBufferHeight = (int)(SizeY * SizeRatio);
            graphics.ApplyChanges();
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Rendering.ResetRenderTargets();

            //graphics.HardwareModeSwitch = false;
            //graphics.IsFullScreen = false;
            //graphics.ApplyChanges();

            CalculateScreenScale();
        }
    }
}