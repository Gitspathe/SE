using System.Numerics;
using static SE.Core.Physics;
using AetherRaycastOutput = tainicom.Aether.Physics2D.Collision.RayCastOutput;

namespace SE.Physics
{
    public struct RayCastOutput
    {
        internal AetherRaycastOutput InternalOutput;

        /// <summary>
        /// The ray hits at p1 + fraction * (p2 - p1), where p1 and p2 come from RayCastInput.
        /// Contains the actual fraction of the ray where it has the intersection point.
        /// </summary>
        public float Fraction => InternalOutput.Fraction;

        /// <summary>
        /// The normal of the face of the shape the ray has hit.
        /// </summary>
        public Vector2 Normal => ToPixels(InternalOutput.Normal);

        public RayCastOutput(AetherRaycastOutput output)
            => InternalOutput = output;

    }
}
