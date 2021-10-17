using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement;
using SE.Core;
using System;

namespace SE
{

    /// <summary>
    /// Controller for the loading screen menu.
    /// </summary>
    public static class LoadingScreen
    {
        private static GraphicsDevice graphicsDevice;
        private static SpriteBatch spriteBatch;
        private static SpriteFont loadingFont;
        private static string loadingMessage = "";
        private static Vector2 loadingPos = new Vector2(100, 980);
        private static AssetConsumerContext assetConsumerContext = new AssetConsumerContext();

        /// <summary>True if the loading screen is active due to Initialize() being called.</summary>
        public static bool CurrentlyLoading { get; private set; }

        /// <summary>True if the loading screen is displayed.</summary>
        public static bool ShowProgress { get; set; }

        /// <summary>Current loading progress, from 0 to 1.</summary>
        public static float Progress { get; set; }

        /// <summary>
        /// Initialize the loading screen.
        /// </summary>
        /// <param name="showProgress">Whether to display the loading screen graphic or not.</param>
        public static void Initialize(bool showProgress = true)
        {
            if (CurrentlyLoading)
                throw new Exception("Something is already loading!");

            Progress = 0;
            ShowProgress = showProgress;
            graphicsDevice = Core.Rendering.GraphicsDevice;
            spriteBatch = Core.Rendering.SpriteBatch;
            loadingFont = AssetManager.Get<SpriteFont>(assetConsumerContext, "editor_heading");
            CurrentlyLoading = true;
            Draw();
        }

        /// <summary>
        /// Changes the current progress for the loading screen.
        /// </summary>
        /// <param name="loadMessage">String giving basic feedback to the player.</param>
        /// <param name="progress">Optional progress from 0 to 1. If set, a percentage will appear next to the loading message string while loading.</param>
        public static void SetProgress(string loadMessage, float progress = 0f)
        {
            if (!string.IsNullOrEmpty(loadMessage)) {
                loadingMessage = loadMessage;
            }
            Progress = progress;
            if (ShowProgress)
                Draw();
        }

        /// <summary>
        /// Finalize the current loading screen process. Should be called when the desired loading sequence is finished.
        /// </summary>
        public static void Finished()
        {
            CurrentlyLoading = false;
        }

        private static void Draw()
        {
            if (spriteBatch == null)
                return;

            Core.Rendering.ChangeDrawCall(SpriteSortMode.FrontToBack, null, null, SamplerState.PointClamp);
            graphicsDevice.Clear(Color.Black);
            if (Progress.Equals(0)) {
                spriteBatch.DrawString(loadingFont, loadingMessage, loadingPos, Color.White);
            } else {
                spriteBatch.DrawString(loadingFont, loadingMessage + $" {Progress * 100:0}%", loadingPos, Color.White);
            }
            Core.Rendering.EndDrawCall();
            graphicsDevice.Present();
        }

    }

}
