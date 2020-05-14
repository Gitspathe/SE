using System;
using Microsoft.Xna.Framework.Input;
using SE.Attributes;
using SE.Common;
using SE.Components;
using SE.Core;
using Vector2 = System.Numerics.Vector2;

namespace SE
{
    [Components(typeof(Camera2D))]
    public class InternalCamera : GameObject
    {
        public override bool DestroyOnLoad => false;
        public override bool IsDynamic => true;
        public override bool IgnoreCulling => true;

        public Camera2D Camera { get; private set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Camera = GetComponent<Camera2D>();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            KeyboardState keyboardState = InputManager.KeyboardState;
            Vector2 movement = Vector2.Zero;
            if (UIManager.IsKeyboardFree()) {
                if (keyboardState.IsKeyDown(Keys.Up)) 
                    movement.Y -= (int)Math.Max(1600 * Time.DeltaTime, 1.0f);
                if (keyboardState.IsKeyDown(Keys.Down)) 
                    movement.Y += (int)Math.Max(1600 * Time.DeltaTime, 1.0f);
                if (keyboardState.IsKeyDown(Keys.Left)) 
                    movement.X -= (int)Math.Max(1600 * Time.DeltaTime, 1.0f);
                if (keyboardState.IsKeyDown(Keys.Right))
                    movement.X += (int)Math.Max(1600 * Time.DeltaTime, 1.0f);
            }

            if (UIManager.AssumedScrollWheelControl == null) {
                if (InputManager.MouseScrollValue == 1)
                    Camera.Zoom += 0.05f;
                else if (InputManager.MouseScrollValue == -1)
                    Camera.Zoom -= 0.05f;
            }
            Transform.Position += movement;
            Camera.Update();
        }

        public InternalCamera(Vector2 pos, float rot, Vector2 scale) : base(pos, rot, scale) { }
    }
}
