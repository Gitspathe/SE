using System;
using Microsoft.Xna.Framework.Input;
using SE.Attributes;
using SE.Common;
using SE.Components;
using SE.Core;
using Vector2 = System.Numerics.Vector2;

namespace SE
{
    [Components(typeof(Camera2D), typeof(SimpleCameraController))]
    public class InternalCamera : GameObject
    {
        public override bool DestroyOnLoad => false;
        public override bool IsDynamic => true;

        public Camera2D Camera { get; private set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Camera = GetComponent<Camera2D>();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
        }

        public InternalCamera(Vector2 pos, float rot, Vector2 scale) : base(pos, rot, scale) { }
    }

    public class SimpleCameraController : Component 
    {
        private Camera2D camera;

        protected override void OnInitialize()
        {
            camera = Owner.GetComponent<Camera2D>();
        }

        protected override void OnUpdate()
        {
            if (UIManager.AssumedScrollWheelControl == null) {
                if (InputManager.MouseScrollValue == 1)
                    camera.Zoom *= 1.1f;
                else if (InputManager.MouseScrollValue == -1)
                    camera.Zoom *= 0.9f;
            }
        }

        public override void OnFixedUpdate()
        {
            KeyboardState keyboardState = InputManager.KeyboardState;
            Vector2 movement = Vector2.Zero;
            if (UIManager.IsKeyboardFree()) {
                float zoomMod = 1.0f;
                if (camera.Zoom > 1.0f) {
                    zoomMod = 1.0f - (0.1f * camera.Zoom);
                } else if (camera.Zoom < 1.0f) {
                    zoomMod = 5.0f - (4.0f * (camera.Zoom));
                }

                if (keyboardState.IsKeyDown(Keys.Up))
                    movement.Y -= (int)Math.Max(1200 * zoomMod * Time.FixedTimestep, 1.0f);
                if (keyboardState.IsKeyDown(Keys.Down))
                    movement.Y += (int)Math.Max(1200 * zoomMod * Time.FixedTimestep, 1.0f);
                if (keyboardState.IsKeyDown(Keys.Left))
                    movement.X -= (int)Math.Max(1200 * zoomMod * Time.FixedTimestep, 1.0f);
                if (keyboardState.IsKeyDown(Keys.Right))
                    movement.X += (int)Math.Max(1200 * zoomMod * Time.FixedTimestep, 1.0f);
            }
            //Transform.Rotation += 0.5f * Time.FixedTimestep;

            Transform.Position += movement;
        }
    }
}
