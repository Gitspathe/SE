using System.Numerics;
using static SE.Core.Physics;
using AetherVelocityConstraint = tainicom.Aether.Physics2D.Dynamics.Contacts.ContactVelocityConstraint;

namespace SE.Physics
{
    public struct ContactVelocityConstraint
    {
        internal AetherVelocityConstraint InternalVelocityConstraint;

        public Vector2 Normal => ToPixels(InternalVelocityConstraint.normal);
        public float InvMassA => InternalVelocityConstraint.invMassA;
        public float InvMassB => InternalVelocityConstraint.invMassB;
        public float InvIa => InternalVelocityConstraint.invIA;
        public float InvIb => InternalVelocityConstraint.invIB;
        public float Friction => InternalVelocityConstraint.friction;
        public float Restitution => InternalVelocityConstraint.restitution;
        public float TangentSpeed => InternalVelocityConstraint.tangentSpeed;
        public int PointCount => InternalVelocityConstraint.pointCount;
        public int ContactIndex => InternalVelocityConstraint.contactIndex;

        internal ContactVelocityConstraint(AetherVelocityConstraint internalConstraint)
        {
            InternalVelocityConstraint = internalConstraint;
        }
    }
}
