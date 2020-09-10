using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Common;
using SE.Core;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;
using MonoGameVector3 = Microsoft.Xna.Framework.Vector3;
using System.Collections.Generic;

using MGVector2 = Microsoft.Xna.Framework.Vector2;

namespace SE.Components
{
    public class Camera2D : Component
    {
        internal Matrix TransformMatrix;
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

        public Rectangle VisibleArea { get; private set; }
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
                Convert.ToInt32(Screen._BASE_RES_X / renderRegion.Width),
                Convert.ToInt32(Screen._BASE_RES_Y / renderRegion.Height));
        }

        private void CalculateScaleMatrix()
        {
            Vector2 pos = Transform.GlobalPosition;

            Matrix translation = Matrix.CreateTranslation(-pos.X, -pos.Y, 0.0f);
            Matrix rotation = Matrix.CreateRotationZ(Transform.GlobalRotation);
            Matrix scale = Matrix.CreateScale(Zoom, Zoom, 1);
            Matrix origin = Matrix.CreateTranslation(ViewBounds.Width * 0.5f, ViewBounds.Height * 0.5f, 0);

            TransformMatrix = Matrix.Identity * translation * rotation * origin * scale;

            // Update visible area.
            Matrix inverseViewMatrix = Matrix.Invert(TransformMatrix);

            MGVector2 tl = MGVector2.Transform(MGVector2.Zero, inverseViewMatrix);
            MGVector2 tr = MGVector2.Transform(new MGVector2(ViewBounds.Width, 0.0f), inverseViewMatrix);
            MGVector2 bl = MGVector2.Transform(new MGVector2(0.0f, ViewBounds.Height), inverseViewMatrix);
            MGVector2 br = MGVector2.Transform(new MGVector2(ViewBounds.Width, ViewBounds.Height), inverseViewMatrix);

            Vector2 min = new Vector2(
                MathHelper.Min(tl.X, MathHelper.Min(tr.X, MathHelper.Min(bl.X, br.X))),
                MathHelper.Min(tl.Y, MathHelper.Min(tr.Y, MathHelper.Min(bl.Y, br.Y))));
            Vector2 max = new Vector2(
                MathHelper.Max(tl.X, MathHelper.Max(tr.X, MathHelper.Max(bl.X, br.X))),
                MathHelper.Max(tl.Y, MathHelper.Max(tr.Y, MathHelper.Max(bl.Y, br.Y))));
            VisibleArea = new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
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
