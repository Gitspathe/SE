using SE.Common;
using SE.Components;
using AetherContact = tainicom.Aether.Physics2D.Dynamics.Contacts.Contact;

namespace SE.Physics
{
    public struct Contact
    {
        internal AetherContact InternalContact;

        public Fixture FixtureA => InternalContact.FixtureA.DependencyFixture as Fixture;
        public Fixture FixtureB => InternalContact.FixtureB.DependencyFixture as Fixture;

        public PhysicsBody PhysicsBodyA => FixtureA?.PhysicsBody;
        public PhysicsBody PhysicsBodyB => FixtureB?.PhysicsBody;

        public PhysicsObject PhysicsObjectA => FixtureA?.PhysicsBody?.PhysicsObject;
        public PhysicsObject PhysicsObjectB => FixtureB?.PhysicsBody?.PhysicsObject;

        public GameObject GameObjectA => FixtureA?.PhysicsBody?.PhysicsObject?.Owner;
        public GameObject GameObjectB => FixtureB?.PhysicsBody?.PhysicsObject?.Owner;

        public float Friction {
            get => InternalContact.Friction;
            set => InternalContact.Friction = value;
        }

        public float Restitution {
            get => InternalContact.Restitution;
            set => InternalContact.Restitution = value;
        }

        /// Get or set the desired tangent speed for a conveyor belt behavior. In meters per second.
        public float TangentSpeed {
            get => InternalContact.TangentSpeed;
            set => InternalContact.TangentSpeed = value;
        }

        /// Enable/disable this contact. This can be used inside the pre-solve
        /// contact listener. The contact is only disabled for the current
        /// time step (or sub-step in continuous collisions).
        /// NOTE: If you are setting Enabled to a constant true or false,
        /// use the explicit Enable() or Disable() functions instead to 
        /// save the CPU from doing a branch operation.
        public bool Enabled {
            get => InternalContact.Enabled;
            set => InternalContact.Enabled = value;
        }

        /// <summary>
        /// Determines whether this contact is touching.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is touching; otherwise, <c>false</c>.
        /// </returns>
        public bool IsTouching {
            get => InternalContact.IsTouching; 
            set => InternalContact.IsTouching = value;
        }

        public void ResetRestitution() 
            => InternalContact.ResetRestitution();

        public void ResetFriction() 
            => InternalContact.ResetFriction();

        internal Contact(AetherContact internalContact)
        {
            InternalContact = internalContact;
        }
    }
}
