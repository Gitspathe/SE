using System;
using DeeZ.Engine.Utility;
using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;

namespace DeeZ.Core.Extensions
{
    public static class RectangleExtensions
    {

        public static Vector2 GetIntersectionDepth(this Rectangle rectA, Rectangle rectB)
        {
            // Calculate half sizes.
            float halfWidthA = rectA.Width / 2.0f;
            float halfHeightA = rectA.Height / 2.0f;
            float halfWidthB = rectB.Width / 2.0f;
            float halfHeightB = rectB.Height / 2.0f;

            // Calculate centers.
            Vector2 centerA = new Vector2(halfWidthA, halfHeightA);
            Vector2 centerB = new Vector2(halfWidthB, halfHeightB);

            // Calculate current and minimum-non-intersecting distances between centers.
            float distanceX = centerA.X - centerB.X;
            float distanceY = centerA.Y - centerB.Y;
            float minDistanceX = halfWidthA;
            float minDistanceY = halfHeightA;

            // If we are not intersecting at all, return (0, 0).
#if NETSTANDARD2_1
            if (MathF.Abs(distanceX) >= minDistanceX || MathF.Abs(distanceY) >= minDistanceY)
                return Vector2.Zero;
#else
            if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
                return Vector2.Zero;
#endif

            // Calculate and return intersection depths.
            float depthX = distanceX > 0.0f ? minDistanceX - distanceX : -minDistanceX - distanceX;
            float depthY = distanceY > 0.0f ? minDistanceY - distanceY : -minDistanceY - distanceY;
            return new Vector2(depthX, depthY);
        }

        public static Vector2 GetIntersectionDepth(this Rectangle rectA, RectangleF rectB)
        {
            // Calculate half sizes.
            float halfWidthA = rectA.Width / 2.0f;
            float halfHeightA = rectA.Height / 2.0f;
            float halfWidthB = rectB.Width / 2.0f;
            float halfHeightB = rectB.Height / 2.0f;

            // Calculate centers.
            Vector2 centerA = new Vector2(halfWidthA, halfHeightA);
            Vector2 centerB = new Vector2(halfWidthB, halfHeightB);

            // Calculate current and minimum-non-intersecting distances between centers.
            float distanceX = centerA.X - centerB.X;
            float distanceY = centerA.Y - centerB.Y;
            float minDistanceX = halfWidthA;
            float minDistanceY = halfHeightA;

            // If we are not intersecting at all, return (0, 0).
#if NETSTANDARD2_1
            if (MathF.Abs(distanceX) >= minDistanceX || MathF.Abs(distanceY) >= minDistanceY)
                return Vector2.Zero;
#else
            if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
                return Vector2.Zero;
#endif

            // Calculate and return intersection depths.
            float depthX = distanceX > 0.0f ? minDistanceX - distanceX : -minDistanceX - distanceX;
            float depthY = distanceY > 0.0f ? minDistanceY - distanceY : -minDistanceY - distanceY;
            return new Vector2(depthX, depthY);
        }

        public static bool Contains(this Rectangle rect, RectangleF value)
        {
            return ((((rect.X <= value.X) && ((value.X + value.Width) <= (rect.X + rect.Width))) && (rect.Y <= value.Y)) && ((value.Y + value.Height) <= (rect.Y + rect.Height)));
        }

        public static bool Intersects(this Rectangle rect, RectangleF r2)
        {
            return !(r2.Left > rect.Right
                     || r2.Right < rect.Left
                     || r2.Top > rect.Bottom
                     || r2.Bottom < rect.Top
                );
        }

        public static void Intersects(this Rectangle rect, ref RectangleF value, out bool result)
        {
            result = !(value.Left > rect.Right
                       || value.Right < rect.Left
                       || value.Top > rect.Bottom
                       || value.Bottom < rect.Top
                );
        }

    }
}
