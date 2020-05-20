using System;
using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;

namespace SE.Utility
{
    public struct RectangleF : IEquatable<RectangleF>
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public static RectangleF Empty => new RectangleF();

        public float Left => X;

        public float Right => (X + Width);

        public float Top => Y;

        public float Bottom => (Y + Height);

        public static bool operator ==(RectangleF a, RectangleF b) => 
            ((a.X == b.X) && (a.Y == b.Y) && (a.Width == b.Width) && (a.Height == b.Height));

        public bool Contains(int x, int y) => 
            ((((X <= x) && (x < (X + Width))) && (Y <= y)) && (y < (Y + Height)));

        public bool Contains(Vector2 value) => 
            ((((X <= value.X) && (value.X < (X + Width))) && (Y <= value.Y)) && (value.Y < (Y + Height)));

        public bool Contains(Point value) => 
            ((((X <= value.X) && (value.X < (X + Width))) && (Y <= value.Y)) && (value.Y < (Y + Height)));

        public bool Contains(RectangleF value) => 
            ((((X <= value.X) && ((value.X + value.Width) <= (X + Width))) && (Y <= value.Y)) && ((value.Y + value.Height) <= (Y + Height)));

        public bool Contains(Rectangle value) => 
            ((((X <= value.X) && ((value.X + value.Width) <= (X + Width))) && (Y <= value.Y)) && ((value.Y + value.Height) <= (Y + Height)));

        public static bool operator !=(RectangleF a, RectangleF b) => !(a == b);

        public static explicit operator Rectangle(RectangleF rect) => new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);

        public void Offset(Point offset)
        {
            X += offset.X;
            Y += offset.Y;
        }

        public void Offset(int offsetX, int offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        public Vector2 Center => new Vector2((X + Width) / 2, (Y + Height) / 2);

        public void Inflate(int horizontalValue, int verticalValue)
        {
            X -= horizontalValue;
            Y -= verticalValue;
            Width += horizontalValue * 2;
            Height += verticalValue * 2;
        }

        public bool IsEmpty => ((((Width == 0) && (Height == 0)) && (X == 0)) && (Y == 0));

        public bool Equals(RectangleF other) => this == other;

        public override bool Equals(object obj) => (obj is RectangleF f) && this == f;

        public override string ToString() => $"{{X:{X} Y:{Y} Width:{Width} Height:{Height}}}";

        public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

        public bool Intersects(RectangleF r2)
        {
            return !(r2.Left > Right
                     || r2.Right < Left
                     || r2.Top > Bottom
                     || r2.Bottom < Top
                    );
        }

        public bool Intersects(Rectangle r2)
        {
            return !(r2.Left > Right
                     || r2.Right < Left
                     || r2.Top > Bottom
                     || r2.Bottom < Top
                );
        }

        public void Intersects(ref RectangleF value, out bool result)
        {
            result = !(value.Left > Right
                     || value.Right < Left
                     || value.Top > Bottom
                     || value.Bottom < Top
                    );
        }

        public void Intersects(ref Rectangle value, out bool result)
        {
            result = !(value.Left > Right
                       || value.Right < Left
                       || value.Top > Bottom
                       || value.Bottom < Top
                );
        }

        public Vector2 GetIntersectionDepth(Rectangle rect)
        {
            // Calculate half sizes.
            float halfWidthA = Width / 2.0f;
            float halfHeightA = Height / 2.0f;
            float halfWidthB = rect.Width / 2.0f;
            float halfHeightB = rect.Height / 2.0f;

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
        #elif NETSTANDARD2_0
            if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
                return Vector2.Zero;
        #endif

            // Calculate and return intersection depths.
            float depthX = distanceX > 0.0f ? minDistanceX - distanceX : -minDistanceX - distanceX;
            float depthY = distanceY > 0.0f ? minDistanceY - distanceY : -minDistanceY - distanceY;
            return new Vector2(depthX, depthY);
        }

        public Vector2 GetIntersectionDepth(RectangleF rect)
        {
            // Calculate half sizes.
            float halfWidthA = Width / 2.0f;
            float halfHeightA = Height / 2.0f;
            float halfWidthB = rect.Width / 2.0f;
            float halfHeightB = rect.Height / 2.0f;

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
        #elif NETSTANDARD2_0
            if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
                return Vector2.Zero;
        #endif

            // Calculate and return intersection depths.
            float depthX = distanceX > 0.0f ? minDistanceX - distanceX : -minDistanceX - distanceX;
            float depthY = distanceY > 0.0f ? minDistanceY - distanceY : -minDistanceY - distanceY;
            return new Vector2(depthX, depthY);
        }

        public RectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
