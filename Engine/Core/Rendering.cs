﻿using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement;
using SE.Components;
using SE.Core.Exceptions;
using SE.Rendering;
using SE.World.Partitioning;
using SE.Core.Extensions;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = System.Numerics.Vector4;
using SE.Particles;

namespace SE.Core
{
    // TODO: Material support. Check Trello for more info. https://trello.com/c/wFMnQrPG/62-material-system
    public static class Rendering
    {
        public static QuickList<IPartitionedRenderable> VisibleSprites = new QuickList<IPartitionedRenderable>(512);
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

        private static Rectangle scissorRectBackup = new Rectangle(0,0,1920,1080);

        public static GraphicsDeviceManager GraphicsDeviceManager { get; private set; }
        public static GraphicsDevice GraphicsDevice { get; private set; }
        public static RenderTarget2D SceneRender { get; set; }
        public static RenderTarget2D UIRender { get; set; }
        public static RenderTarget2D FinalRender { get; set; }
        internal static QuickList<Camera2D> Cameras { get; } = new QuickList<Camera2D>();
        
        public static Span<Vector4> CameraBounds {
            get {
                tmpCamBounds.Clear();
                for (int i = 0; i < Cameras.Count; i++) {
                    tmpCamBounds.Add(Cameras.Array[i].VisibleArea.ToVector4());
                }
                return new Span<Vector4>(tmpCamBounds.Array, 0, tmpCamBounds.Count);
            }
        }

        private static QuickList<Vector4> tmpCamBounds = new QuickList<Vector4>();
        public static Effect ParticleShader;

        public static void Update()
        {
            if (Screen.IsFullHeadless)
                throw new HeadlessNotSupportedException("Cannot update rendering in fully headless display mode.");

            // Show message if there are no cameras being rendered.
            if (Cameras.Count < 1) {
                DrawNoCamerasMessage();
            } else {
                foreach (Camera2D cam in Cameras) {
                    foreach (QuickList<IRenderLoopAction> renderActions in RenderLoop.Loop.Values) {
                        for (int i = 0; i < renderActions.Count; i++) {
                            renderActions.Array[i].Invoke(cam);
                            if (RenderLoop.IsDirty)
                                break;
                        }
                        if(RenderLoop.IsDirty)
                            break;
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

            GraphicsDeviceManager = gdm;
            GraphicsDevice = gd;
            SpriteBatch = new SpriteBatch(gd);
            ParticleShader = GameEngine.EngineContent.Load<Effect>("shader");
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

            // TODO: Settings config file?
            //GraphicsDeviceManager.PreferMultiSampling = true;
            //GraphicsDevice.PresentationParameters.MultiSampleCount = 16;

            SceneRender = new RenderTarget2D(GraphicsDevice, GraphicsDeviceManager.PreferredBackBufferWidth,
                GraphicsDeviceManager.PreferredBackBufferHeight, false,
                GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat,
                GraphicsDevice.PresentationParameters.MultiSampleCount, RenderTargetUsage.PreserveContents);
            UIRender = new RenderTarget2D(GraphicsDevice, GraphicsDeviceManager.PreferredBackBufferWidth, 
                GraphicsDeviceManager.PreferredBackBufferHeight, false,
                GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat, 
                GraphicsDevice.PresentationParameters.MultiSampleCount, RenderTargetUsage.PreserveContents);
            
            FinalRender = GameEngine.IsEditor 
                ? new RenderTarget2D(GraphicsDevice, GraphicsDeviceManager.PreferredBackBufferWidth, 
                    GraphicsDeviceManager.PreferredBackBufferHeight, false, 
                    GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat, 
                    GraphicsDevice.PresentationParameters.MultiSampleCount, RenderTargetUsage.DiscardContents) 
                : null;
            
            Lighting.Reset();
            GC.Collect();
        }

        public static void DrawRenderTargets(Camera2D camera)
        {
            GraphicsDevice.SetRenderTarget(camera.RenderTarget);

            ChangeDrawCall(SpriteSortMode.Deferred, null, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone);
            SpriteBatch.Draw(SceneRender, new Rectangle(0, 0, GraphicsDeviceManager.PreferredBackBufferWidth, GraphicsDeviceManager.PreferredBackBufferHeight), Color.White);
            SpriteBatch.Draw(UIRender, new Rectangle(0, 0, GraphicsDeviceManager.PreferredBackBufferWidth, GraphicsDeviceManager.PreferredBackBufferHeight), Color.White);
            EndDrawCall();

            //GraphicsDevice.SetRenderTarget(null);
        }

        public static void RenderCameras()
        {
            GraphicsDevice.SetRenderTarget(FinalRender);

            int screenWidth = GraphicsDeviceManager.PreferredBackBufferWidth;
            int screenHeight = GraphicsDeviceManager.PreferredBackBufferHeight;

            Cameras.Sort(new CameraQueueComparer());
            ChangeDrawCall(SpriteSortMode.Deferred, null, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone);
            foreach (Camera2D camera in Cameras) {
                Rectangle renderRegion = new Rectangle((int)(camera.RenderRegion.X * screenWidth), 
                    (int)(camera.RenderRegion.Y * screenHeight), 
                    (int)(camera.RenderRegion.Width * screenWidth), 
                    (int)(camera.RenderRegion.Height * screenHeight));

                SpriteBatch.Draw(camera.RenderTarget, renderRegion, Color.White);
            }
            EndDrawCall();
            GraphicsDevice.SetRenderTarget(null);

            DrawModel(ModelDefinition.TESTMODEL, Cameras[0]);

        }

        public static void DrawModel(ModelDefinition model, Camera2D camera)
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.DepthStencilState = DepthStencilGreater;

            model.Transform.Rotation += System.Numerics.Quaternion.CreateFromYawPitchRoll(0.1f, 0.0f, 0.0f) * Time.DeltaTime;

            //model.Transform.Position = new System.Numerics.Vector3(0.0f, 0.0f, 0.0f);
            //model.Transform.Scale = new System.Numerics.Vector3(1.0f, 1.0f, 1.0f);
            foreach (ModelMesh mesh in model.Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = model.Transform.WorldTransformation.ToMonoGameMatrix();
                    
                    // TODO: Replace with camera view.
                    effect.View = Matrix.CreateLookAt(new Vector3(0, 0, 100), Vector3.Zero, Vector3.Up);
                    
                    effect.Projection = camera.ProjectionMatrix;

                    //effect.EnableDefaultLighting();
                    //effect.World = Matrix.CreateTranslation(new Vector3(0, 0, 0));
                    //effect.View = Matrix.CreateLookAt(new Vector3(0, 0, 100), Vector3.Zero, Vector3.Up);
                    //effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), 1.6f, 0.1f, 10000.0f);
                }

                mesh.Draw();
            }
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
            Rectangle viewRect = camera.VisibleArea;
            SpatialPartitionManager<IPartitionedRenderable>.GetFromRegion(VisibleSprites, viewRect);
        }

        public static void Prepare()
        {
            // Cleanup for next render frame.
            GraphicsDevice.ScissorRectangle = scissorRectBackup;
        }

        public static void AddCamera(Camera2D camera)
        {
            if (!Cameras.Contains(camera))
                Cameras.Add(camera);
        }

        public static void RemoveCamera(Camera2D camera)
        {
            Cameras.Remove(camera);
        }

        private static void DrawNoCamerasMessage()
        {
            const string str = "No cameras.";
            Vector2 strSize = UIManager.DefaultFont.MeasureString(str).ToNumericsVector2();

            GraphicsDevice.SetRenderTarget(FinalRender);
            GraphicsDevice.Clear(Color.Black);
            ChangeDrawCall(SpriteSortMode.Deferred, Screen.ScreenScaleMatrix);
            SpriteBatch.DrawString(UIManager.DefaultFont, str, Screen.ViewSize / 2.0f, Color.Red, 0f, strSize / 2.0f, Vector2.One, SpriteEffects.None, 0.0f);
            EndDrawCall();
            
            GraphicsDevice.SetRenderTarget(null);
        }

        private struct CameraQueueComparer : IComparer<Camera2D>
        {
            int IComparer<Camera2D>.Compare(Camera2D x, Camera2D y) => y.Priority.CompareTo(x.Priority);
        }
    }
}