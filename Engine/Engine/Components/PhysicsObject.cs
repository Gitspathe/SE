using Microsoft.Xna.Framework;
using SE.Common;
using SE.Physics;
using SE.Utility;
using System;
using BodyType = SE.Physics.BodyType;
using Vector2 = System.Numerics.Vector2;

namespace SE.Components
{
    public class PhysicsObject : Component, IPhysicsBodyProvider
    {
        /// <inheritdoc />
        public override int Queue => 50;

        public PhysicsBody Body {
            get => body;
            set {
                if (body != null || (value == null && body != null)) {
                    body.Provider = null;
                    body.Dispose();
                }
                body = value;
                if (body == null)
                    return;

                body.Provider = this;
                if (Owner != null) {
                    body.Position = Owner.Transform.GlobalPositionInternal;
                }
            }
        }
        private PhysicsBody body;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (Owner.PhysicsObject != null)
                throw new InvalidOperationException("Attempted to add an additional PhysicsObject. A GameObject may only contain a single PhysicsObject.");

            if (body != null) {
                body.Position = Owner.Transform.GlobalPositionInternal;
            }
            Owner.PhysicsObject = this;
        }

        public override void OnFixedUpdate()
        {
            base.OnUpdate();

            // Update position.
            if (Body != null)
                Owner.Transform.GlobalPosition = Body.Position;
        }

        internal void OverridePosition(Vector2 pos)
        {
            Body?.OverridePosition(pos);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Owner.PhysicsObject = null;
        }

        protected override void Dispose(bool disposing = true)
        {
            base.Dispose(disposing);
            Body = null;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (body != null) {
                Core.Physics.Remove(Body);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (body != null) {
                Core.Physics.Add(Body);
            }
        }

        /// <summary>
        /// Resets the dynamics of this body.
        /// Sets torque, force and linear/angular velocity to 0
        /// </summary>
        public void ResetDynamics()
            => Body.ResetDynamics();

        public PhysicsObject() { }

        public PhysicsObject(PhysicsBody body)
        {
            Body = body;
            Body.Provider = this;
        }

        public PhysicsObject(Rectangle rectangle, float density = 1.0f, BodyType bodyType = BodyType.Static)
        {
            Body = Core.Physics.CreateRectangle(rectangle, density, bodyType);
            Body.Provider = this;
        }

        public PhysicsObject(Circle circle, float density = 1.0f, BodyType bodyType = BodyType.Static)
        {
            Body = Core.Physics.CreateCircle(circle.Radius, density, new Vector2(circle.Radius), bodyType);
            Body.Provider = this;
        }
    }
}