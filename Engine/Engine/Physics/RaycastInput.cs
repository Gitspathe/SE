using System.Numerics;
using static SE.Core.Physics;
using AetherRaycastInput = tainicom.Aether.Physics2D.Collision.RayCastInput;

namespace SE.Physics
{
    public struct RayCastInput
    {
        internal AetherRaycastInput InternalInput;

        /// <summary>
        /// The ray extends from p1 to p1 + maxFraction * (p2 - p1).
        /// If you supply a max fraction of 1, the ray extends from p1 to p2.
        /// A max fraction of 0.5 makes the ray go from p1 and half way to p2.
        /// </summary>
        public float MaxFraction => InternalInput.MaxFraction;

        /// <summary>
        /// The starting point of the ray.
        /// </summary>
        public Vector2 Point1 => ToPixels(InternalInput.Point1);

        /// <summary>
        /// The ending point of the ray.
        /// </summary>
        public Vector2 Point2 => ToPixels(InternalInput.Point2);

        public RayCastInput(AetherRaycastInput input)
            => InternalInput = input;

    }
}
