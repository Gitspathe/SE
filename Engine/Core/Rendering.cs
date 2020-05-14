using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Components;
using SE.Core.Exceptions;
using SE.Engine.Utility;
using SE.Rendering;
using SE.World.Partitioning;
using SE.Core.Extensions;
using Vector2 = System.Numerics.Vector2;

namespace SE.Core
{
    public static class Rendering
    {
        public static QuickList<IPartitionObject> VisibleSprites = new QuickList<IPartitionObject>(512);
        public static SpriteBatch SpriteBatch;

        public static DepthStencilState DepthStencilLess = new DepthStencilState {
            DepthBufferEnable = true,
            DepthBufferFunction = CompareFunction.Less
        };

        public static DepthStencilState DepthStencilGreater = new DepthStencilState {
            DepthBufferEnable = true,
            DepthBufferFunction = CompareFunction.GreaterEqual
        };

        public static RasterizerState RasterizerStateSolidNoCull = new RasterizerState {
            FillMode = FillMode.Solid,
            CullMode = CullMode.None
        };

        public static bool SpriteBatchActive { get; private set; }

        internal static bool DoFinalRender;

        private static Rectangle scissorRectBackup = new Rectangle(0,0,1920,1080);

        public static GraphicsDeviceManager GraphicsDeviceManager { get; private set; }
        public static GraphicsDevice GraphicsDevice { get; private set; }
        public static RenderTarget2D SceneRender { get; private set; }
        public static RenderTarget2D UIRender { get; private set; }
        public static RenderTarget2D FinalRender { get; private set; }
        internal static QuickList<Camera2D> cameras { get; } = new QuickList<Camera2D>();

        public static void Update()
        {
            if (Screen.IsFullHeadless)
                throw new HeadlessNotSupportedException("Cannot update rendering in fully headless display mode.");

            // Show message if there are no cameras being rendered.
            if (cameras.Count < 1) {
                DrawNoCamerasMessage();
            } else {
                foreach (Camera2D cam in cameras) {
                    foreach (Action<Camera2D> renderAction in RenderLoop.Loop.Values) {
                        renderAction.Invoke(cam);
                        if (RenderLoop.IsDirty) {
                            break;
                        }
                    }
                }
                RenderLoop.IsDirty = false;

                RenderCameras();
            }
        }

        public static void Initialize(GraphicsDeviceManager gdm, GraphicsDevice gd)
        {
            if (Screen.IsFullHeadless)
                throw new HeadlessNotSupportedException("Cannot initialize rendering in fully headless display mode.");

            DoFinalRender = GameEngine.IsEditor;
            GraphicsDeviceManager = gdm;
            GraphicsDevice = gd;
            SpriteBatch = new SpriteBatch(gd);
        }

        public static void ResetRenderTargets()
        {
            if (SceneRender != null) {
                SceneRender.Dispose();
                while (!SceneRender.IsDisposed) { } // Possible hard-lock.
            }
            if (UIRender != null) {
                UIRender.Dispose();
                while (!UIRender.IsDisposed) { } // Possible hard-lock.
            }
            SceneRender = new RenderTarget2D(GraphicsDevice, GraphicsDeviceManager.PreferredBackBufferWidth,
                GraphicsDeviceManager.PreferredBackBufferHeight, false,
                GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat,
                GraphicsDevice.PresentationParameters.MultiSampleCount, RenderTargetUsage.PreserveContents);
            UIRender = new RenderTarget2D(GraphicsDevice, GraphicsDeviceManager.PreferredBackBufferWidth, 
                GraphicsDeviceManager.PreferredBackBufferHeight, false,
                GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat, 
                GraphicsDevice.PresentationParameters.MultiSampleCount, RenderTargetUsage.PreserveContents);

            if (DoFinalRender) {
                FinalRender = new RenderTarget2D(GraphicsDevice, GraphicsDeviceManager.PreferredBackBufferWidth,
                    GraphicsDeviceManager.PreferredBackBufferHeight, false,
                    GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat,
                    GraphicsDevice.PresentationParameters.MultiSampleCount, RenderTargetUsage.DiscardContents);
            }

            Lighting.Reset();
            GC.Collect();
        }

        public static void DrawRenderTargets(Camera2D camera)
        {
            GraphicsDevice.SetRenderTarget(camera.renderTarget);

            ChangeDrawCall(SpriteSortMode.Deferred, null, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone);
            SpriteBatch.Draw(SceneRender, new Rectangle(0, 0, GraphicsDeviceManager.PreferredBackBufferWidth, GraphicsDeviceManager.PreferredBackBufferHeight), Color.White);
            SpriteBatch.Draw(UIRender, new Rectangle(0, 0, GraphicsDeviceManager.PreferredBackBufferWidth, GraphicsDeviceManager.PreferredBackBufferHeight), Color.White);
            EndDrawCall();

            //GraphicsDevice.SetRenderTarget(null);
        }

        public static void RenderCameras()
        {
            // If doFinalRender is true, draw to an intermediate render target. Otherwise, draw to the back buffer.
            GraphicsDevice.SetRenderTarget(DoFinalRender ? FinalRender : null);

            int screenWidth = GraphicsDeviceManager.PreferredBackBufferWidth;
            int screenHeight = GraphicsDeviceManager.PreferredBackBufferHeight;

            cameras.Sort(new CameraQueueComparer());
            ChangeDrawCall(SpriteSortMode.Deferred, null, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone);
            foreach (Camera2D camera in cameras) {
                Rectangle renderRegion = new Rectangle((int)(camera.RenderRegion.X * screenWidth), 
                    (int)(camera.RenderRegion.Y * screenHeight), 
                    (int)(camera.RenderRegion.Width * screenWidth), 
                    (int)(camera.RenderRegion.Height * screenHeight));

                SpriteBatch.Draw(camera.renderTarget, renderRegion, Color.White);
            }
            EndDrawCall();

            if (DoFinalRender)
                GraphicsDevice.SetRenderTarget(null);
        }

        public static void ChangeDrawCall(SpriteSortMode sortMode, Matrix? transformMatrix, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null)
        {
            if (SpriteBatchActive) {
                SpriteBatch.End();
            }

            SpriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
            SpriteBatchActive = true;
        }

        public static void EndDrawCall()
        {
            if (SpriteBatchActive) {
                SpriteBatch.End();
            }
            SpriteBatchActive = false;
        }

        public static Rectangle ApplyScreenRectangleScaling(Rectangle rectangle)
        {
            return new Rectangle((int)MathF.Round(rectangle.X * Screen.SizeRatio), (int)MathF.Round(rectangle.Y * Screen.SizeRatio),
                (int)MathF.Round(rectangle.Width * Screen.SizeRatio), (int)MathF.Round(rectangle.Height * Screen.SizeRatio));
        }

        public static void PerformCulling(Camera2D camera)
        {
            VisibleSprites.Clear();
            //Rectangle viewRect = camera.Zoom > 1.0f ? camera.UnscaledViewBounds : camera.Bounds;
            Rectangle viewRect = camera.ViewBounds;
            SpatialPartitionManager.GetFromRegionRaw<SpriteBase>(VisibleSprites, viewRect);
        }

        public static void Prepare()
        {
            // Cleanup for next render frame.
            GraphicsDevice.ScissorRectangle = scissorRectBackup;
        }

        public static void AddCamera(Camera2D camera)
        {
            if (!cameras.Contains(camera))
                cameras.Add(camera);
        }

        public static void RemoveCamera(Camera2D camera)
        {
            cameras.Remove(camera);
        }

        private static void DrawNoCamerasMessage()
        {
            const string str = "No cameras.";
            Vector2 strSize = UIManager.DefaultFont.MeasureString(str).ToNumericsVector2();

            GraphicsDevice.SetRenderTarget(DoFinalRender ? FinalRender : null);
            GraphicsDevice.Clear(Color.Black);
            ChangeDrawCall(SpriteSortMode.Deferred, Screen.ScreenScaleMatrix);
            SpriteBatch.DrawString(UIManager.DefaultFont, str, Screen.ViewSize / 2.0f, Color.Red, 0f, strSize / 2.0f, Vector2.One, SpriteEffects.None, 0.0f);
            EndDrawCall();
            
            if (DoFinalRender)
                GraphicsDevice.SetRenderTarget(null);
        }

        private struct CameraQueueComparer : IComparer<Camera2D>
        {
            int IComparer<Camera2D>.Compare(Camera2D x, Camera2D y) => y.Priority.CompareTo(x.Priority);
        }
    }
}