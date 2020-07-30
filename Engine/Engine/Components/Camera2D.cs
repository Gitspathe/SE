using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Common;
using SE.Core;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;
using MonoGameVector3 = Microsoft.Xna.Framework.Vector3;
using System.Collections.Generic;

namespace SE.Components
{
    public class Camera2D : Component
    {
        internal Matrix ScaleMatrix;
        internal RenderTarget2D renderTarget;

        public uint Priority { get; set; }

        public float Zoom {
            get => zoom;
            set {
                if (value < 0.10f)
                    value = 0.10f;
                else if (value > 8.0f)
                    value = 8.0f;

                zoom = value;
            }
        }
        private float zoom = 1.0f;

        public RectangleF RenderRegion {
            get => renderRegion;
            set {
                value.X = Math.Clamp(value.X, 0.0f, 1.0f);
                value.Y = Math.Clamp(value.Y, 0.0f, 1.0f);
                value.Width = Math.Clamp(value.Width, 0.0f, 1.0f);
                value.Height = Math.Clamp(value.Height, 0.0f, 1.0f);
                renderRegion = value;
            }
        }
        private RectangleF renderRegion = new RectangleF(0.0f, 0.0f, 1.0f, 1.0f);

        public Vector2 Position { get; private set; } = Vector2.Zero;
        public Rectangle ViewBounds { get; private set; }

        public static Camera2D Main => Core.Rendering.Cameras.Array[0];

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Position = Owner.Transform.GlobalPosition;
            Core.Rendering.AddCamera(this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Core.Rendering.AddCamera(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Core.Rendering.RemoveCamera(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Core.Rendering.RemoveCamera(this);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            Position = Owner.Transform.GlobalPosition;
            CalculateBounds();
            CalculateScaleMatrix();
        }

        private void CalculateBounds()
        {
            ViewBounds = new Rectangle((int)Position.X, (int)Position.Y,
                Convert.ToInt32(Screen._BASE_RES_X / Zoom / renderRegion.Width),
                Convert.ToInt32(Screen._BASE_RES_Y / Zoom / renderRegion.Height));
        }

        private void CalculateScaleMatrix()
        {
            ScaleMatrix = Matrix.CreateScale(new MonoGameVector3(Zoom, Zoom, 1));
        }

        public Vector2? MouseToWorldPoint() 
            => ScreenToWorldPoint(Screen.MousePoint);

        public Vector2? ScreenToWorldPoint(Vector2? screenPoint)
        {
            if (screenPoint.HasValue) {
                return new Vector2(
                    ((screenPoint.Value.X - (renderRegion.X * Screen._BASE_RES_X)) / zoom / renderRegion.Width) + ViewBounds.X,
                    ((screenPoint.Value.Y - (renderRegion.Y * Screen._BASE_RES_Y)) / zoom / renderRegion.Height) + ViewBounds.Y);
            }
            return null;
        }

        public Vector2? WorldToCameraPoint(Vector2? worldPoint)
        {
            if (worldPoint.HasValue) {
                return new Vector2(
                    ((worldPoint.Value.X + (renderRegion.X * Screen._BASE_RES_X)) / zoom / renderRegion.Width) - ViewBounds.X,
                    ((worldPoint.Value.Y + (renderRegion.Y * Screen._BASE_RES_Y)) / zoom / renderRegion.Height) - ViewBounds.Y);
            }
            return null;
        }

        public Camera2D()
        {
            if (!Screen.IsFullHeadless) {
                renderTarget = new RenderTarget2D(GameEngine.Engine.GraphicsDevice,
                    GameEngine.Engine.GraphicsDeviceManager.PreferredBackBufferWidth,
                    GameEngine.Engine.GraphicsDeviceManager.PreferredBackBufferHeight,
                    false,
                    GameEngine.Engine.GraphicsDevice.PresentationParameters.BackBufferFormat,
                    GameEngine.Engine.GraphicsDevice.PresentationParameters.DepthStencilFormat,
                    GameEngine.Engine.GraphicsDevice.PresentationParameters.MultiSampleCount,
                    RenderTargetUsage.PreserveContents);
            }
        }
    }
}
