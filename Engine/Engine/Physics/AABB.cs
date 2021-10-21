using SE.Core;
using tainicom.Aether.Physics2D;
using tainicom.Aether.Physics2D.Common;
using static SE.Core.Physics;
using AetherAABB = tainicom.Aether.Physics2D.Collision.AABB;
using Vector2 = System.Numerics.Vector2;

namespace SE.Physics
{
    public struct AABB
    {
        private AetherAABB internalAabb;

        /// <summary>
        /// The lower vertex
        /// </summary>
        public Vector2 LowerBound {
            get => ToPixels(internalAabb.LowerBound);
            set => internalAabb.LowerBound = ToMeters(value);
        }

        /// <summary>
        /// The upper vertex
        /// </summary>
        public Vector2 UpperBound {
            get => ToPixels(internalAabb.UpperBound);
            set => internalAabb.UpperBound = ToMeters(value);
        }

        public float Width => UpperBound.X - LowerBound.X;

        public float Height => UpperBound.Y - LowerBound.Y;

        /// <summary>
        /// Get the center of the AABB.
        /// </summary>
        public Vector2 Center => 0.5f * (LowerBound + UpperBound);

        /// <summary>
        /// Get the extents of the AABB (half-widths).
        /// </summary>
        public Vector2 Extents => 0.5f * (UpperBound - LowerBound);

        /// <summary>
        /// Get the perimeter length
        /// </summary>
        public float Perimeter {
            get {
                float wx = UpperBound.X - LowerBound.X;
                float wy = UpperBound.Y - LowerBound.Y;
                return 2.0f * (wx + wy);
            }
        }

        /// <summary>
        /// First quadrant
        /// </summary>
        public AABB Q1 => new AABB(Center, UpperBound);

        /// <summary>
        /// Second quadrant
        /// </summary>
        public AABB Q2 => new AABB(new Vector2(LowerBound.X, Center.Y), new Vector2(Center.X, UpperBound.Y));

        /// <summary>
        /// Third quadrant
        /// </summary>
        public AABB Q3 => new AABB(LowerBound, Center);

        /// <summary>
        /// Forth quadrant
        /// </summary>
        public AABB Q4 => new AABB(new Vector2(Center.X, LowerBound.Y), new Vector2(UpperBound.X, Center.Y));

        /// <summary>
        /// Verify that the bounds are sorted. And the bounds are valid numbers (not NaN).
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValid()
        {
            Vector2 d = UpperBound - LowerBound;
            bool valid = d.X >= 0.0f && d.Y >= 0.0f;
            valid = valid && LowerBound.ToMonoGameVector2().IsValid() && UpperBound.ToMonoGameVector2().IsValid();
            return valid;
        }

        /// <summary>
        /// Combine an AABB into this one.
        /// </summary>
        /// <param name="aabb">The aabb.</param>
        public void Combine(ref AABB aabb)
        {
            LowerBound = Vector2.Min(LowerBound, aabb.LowerBound);
            UpperBound = Vector2.Max(UpperBound, aabb.UpperBound);
        }

        /// <summary>
        /// Combine two AABBs into this one.
        /// </summary>
        /// <param name="aabb1">The aabb1.</param>
        /// <param name="aabb2">The aabb2.</param>
        public void Combine(ref AABB aabb1, ref AABB aabb2)
        {
            LowerBound = Vector2.Min(aabb1.LowerBound, aabb2.LowerBound);
            UpperBound = Vector2.Max(aabb1.UpperBound, aabb2.UpperBound);
        }

        /// <summary>
        /// Does this aabb contain the provided AABB.
        /// </summary>
        /// <param name="aabb">The aabb.</param>
        /// <returns>
        /// 	<c>true</c> if it contains the specified aabb; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(ref AABB aabb)
        {
            bool result = LowerBound.X <= aabb.LowerBound.X;
            result = result && LowerBound.Y <= aabb.LowerBound.Y;
            result = result && aabb.UpperBound.X <= UpperBound.X;
            result = result && aabb.UpperBound.Y <= UpperBound.Y;
            return result;
        }

        /// <summary>
        /// Determines whether the AAABB contains the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>
        /// 	<c>true</c> if it contains the specified point; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(ref Vector2 point)
        {
            //using epsilon to try and gaurd against float rounding errors.
            return (point.X > (LowerBound.X + Settings.Epsilon) && point.X < (UpperBound.X - Settings.Epsilon) &&
                   (point.Y > (LowerBound.Y + Settings.Epsilon) && point.Y < (UpperBound.Y - Settings.Epsilon)));
        }

        /// <summary>
        /// Test if the two AABBs overlap.
        /// </summary>
        /// <param name="a">The first AABB.</param>
        /// <param name="b">The second AABB.</param>
        /// <returns>True if they are overlapping.</returns>
        public static bool TestOverlap(ref AABB a, ref AABB b)
        {
            if (b.LowerBound.X > a.UpperBound.X || b.LowerBound.Y > a.UpperBound.Y)
                return false;
            if (a.LowerBound.X > b.UpperBound.X || a.LowerBound.Y > b.UpperBound.Y)
                return false;

            return true;
        }

        /// <summary>
        /// Raycast against this AABB using the specificed points and maxfraction (found in input)
        /// </summary>
        /// <param name="output">The results of the raycast.</param>
        /// <param name="input">The parameters for the raycast.</param>
        /// <param name="doInteriorCheck">Should the raycast hit objects it is inside of?</param>
        /// <returns>True if the ray intersects the AABB</returns>
        public bool RayCast(out RayCastOutput output, ref RayCastInput input, bool doInteriorCheck = true)
            => internalAabb.RayCast(out output.InternalOutput, ref input.InternalInput, doInteriorCheck);

        internal AABB(Vector2 min, Vector2 max) : this(ref min, ref max)
            => internalAabb = new AetherAABB(ToMeters(min), ToMeters(max));

        internal AABB(ref Vector2 min, ref Vector2 max)
            => internalAabb = new AetherAABB(ToMeters(min), ToMeters(max));

        internal AABB(Vector2 center, float width, float height)
            => internalAabb = new AetherAABB(ToMeters(center), ToMeters(width), ToMeters(height));

        internal AABB(AetherAABB internalAabb)
            => this.internalAabb = internalAabb;

    }
}
