using System;
using DeeZ.Core;
using DeeZ.Engine.Input;
using SE.Common;
using SE.Core;
using Vector2 = System.Numerics.Vector2;

namespace SE.Components
{
    public class NetworkedEntity : Component
    {
        //protected PhysicsObject PhysicsObject;

        public Players PlayerIndex;

        public override void OnFixedUpdate()
        {
            if (Owner.NetIdentity == null || !Owner.NetIdentity.IsOwner)
                return;
            //if(PhysicsObject == null)
                //PhysicsObject = Owner.GetComponent<PhysicsObject>();

            Vector2 velocity = Vector2.Zero;
            if (UIManager.IsKeyboardFree()) {
                velocity.Y = InputManager.AxisState(PlayerIndex, "PlayerVertical") * 256;
                velocity.X = InputManager.AxisState(PlayerIndex, "PlayerHorizontal") * 256;
            }
            if (MathF.Abs(velocity.Y) > 128f && MathF.Abs(velocity.X) > 128f) {
                velocity = Vector2.Normalize(velocity);
                velocity *= 256;
            }
            //PhysicsObject.Body.LinearVelocity = velocity;
            base.OnUpdate();
        }
    }
}
