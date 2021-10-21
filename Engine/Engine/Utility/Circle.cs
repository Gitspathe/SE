using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;
using Vector2 = System.Numerics.Vector2;

namespace SE.Utility
{
    /// <summary>
    /// Structure for a circle.
    /// </summary>
    public struct Circle : IEquatable<Circle>
    {
        /// <summary>Center position of the circle.</summary>
        public Vector2 Center { get; set; }

        /// <summary>Radius of the circle.</summary>
        public float Radius;

        public float Diameter => 2 * Radius;

        public float Circumference => MathHelper.TwoPi * Radius;

        public Circle(float radius)
        {
            this.Radius = radius;
            Center = Vector2.Zero;
        }

        public Circle(Vector2 center, float radius)
        {
            Center = center;
            this.Radius = radius;
        }

        public Circle(float centerX, float centerY, float radius)
        {
            Center = new Vector2(centerX, centerY);
            this.Radius = radius;
        }

        public Circle(Rectangle rect)
        {
            Center = new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            Radius = rect.Width > rect.Height ? rect.Width : rect.Height;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(Circle b)
        {
            return Intersects(ref this, ref b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(Circle b, out float depth, out Vector2 normal)
        {
            return Intersects(ref this, ref b, out depth, out normal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(Rectangle b)
        {
            return Intersects(ref this, ref b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(Rectangle b, out float depth, out Vector2 normal)
        {
            return Intersects(ref this, ref b, out depth, out normal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(RectangleF b)
        {
            return Intersects(ref this, ref b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(RectangleF b, out float depth, out Vector2 normal)
        {
            return Intersects(ref this, ref b, out depth, out normal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Intersects(Circle a, Circle b)
        {
            return Intersects(ref a, ref b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Intersects(Circle a, Circle b, out float depth, out Vector2 normal)
        {
            return Intersects(ref a, ref b, out depth, out normal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Intersects(Circle a, Rectangle b)
        {
            return Intersects(ref a, ref b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Intersects(Circle a, Rectangle b, out float depth, out Vector2 normal)
        {
            return Intersects(ref a, ref b, out depth, out normal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Intersects(ref Circle a, ref Circle b, out float depth, out Vector2 normal)
        {
            Vector2 distance = a.Center - b.Center;
            depth = distance.Length();
            if (float.IsNaN(depth)) {
                normal = new Vector2(0, 0);
                depth = 0f;
                return false;
            }
            float radiusSum = a.Radius > b.Radius ? b.Radius : a.Radius;
            normal = Vector2.Normalize(distance);
            return depth <= radiusSum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Intersects(ref Circle a, ref Circle b)
        {
            float depth = (a.Center - b.Center).Length();
            float radiusSum = a.Radius > b.Radius ? b.Radius : a.Radius;
            return depth <= radiusSum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Intersects(ref Circle a, ref Rectangle b, out float depth, out Vector2 normal)
        {
            normal = new Vector2(a.Center.X - (b.X + b.Width / 2.0f), a.Center.Y - (b.Y + b.Height / 2.0f));
#if NETSTANDARD2_1
            Vector2 distance = new Vector2(MathF.Abs(normal.X), MathF.Abs(normal.Y));
#else
            Vector2 distance = new Vector2(Math.Abs(normal.X), Math.Abs(normal.Y));
#endif
            normal = Vector2.Normalize(normal);
            depth = distance.Length();
            if (float.IsNaN(depth)) {
                normal = new Vector2(0, 0);
                depth = 0f;
                return false;
            }
            if (distance.X > b.Width / 2.0f + a.Radius / 2 || distance.Y > b.Height / 2.0f + a.Radius / 2)
                return false;
            if (distance.X <= b.Width / 2.0f || distance.Y <= b.Height / 2.0f)
                return true;

#if NETSTANDARD2_1
            float distanceSq = MathF.Pow(distance.X - b.Width / 2.0f, 2) + MathF.Pow(distance.Y - b.Height / 2.0f, 2);
            return distanceSq <= MathF.Pow(a.Radius, 2.0f);
#else
            double distanceSq = Math.Pow(distance.X - b.Width / 2.0f, 2) + Math.Pow(distance.Y - b.Height / 2.0f, 2);
            return distanceSq <= Math.Pow(a.Radius, 2.0f);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Intersects(ref Circle a, ref Rectangle b)
        {
#if NETSTANDARD2_1
            Vector2 distance = new Vector2(MathF.Abs(a.Center.X - (b.X + b.Width / 2)), MathF.Abs(a.Center.Y - (b.Y + b.Height / 2)));
#else
            Vector2 distance = new Vector2(Math.Abs(a.Center.X - (b.X + b.Width / 2)), Math.Abs(a.Center.Y - (b.Y + b.Height / 2)));
#endif
            if (distance.X > b.Width / 2.0f + a.Radius / 2 || distance.Y > b.Height / 2.0f + a.Radius / 2)
                return false;
            if (distance.X <= b.Width / 2.0f || distance.Y <= b.Height / 2.0f)
                return true;

#if NETSTANDARD2_1
            float distanceSq = MathF.Pow(distance.X - b.Width / 2.0f, 2) + MathF.Pow(distance.Y - b.Height / 2.0f, 2);
            return distanceSq <= MathF.Pow(a.Radius, 2);
#else
            double distanceSq = Math.Pow(distance.X - b.Width / 2.0f, 2) + Math.Pow(distance.Y - b.Height / 2.0f, 2);
            return distanceSq <= Math.Pow(a.Radius, 2);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Intersects(ref Circle a, ref RectangleF b, out float depth, out Vector2 normal)
        {
            normal = new Vector2(a.Center.X - (b.X + b.Width / 2), a.Center.Y - (b.Y + b.Height / 2));
#if NETSTANDARD2_1
            Vector2 distance = new Vector2(MathF.Abs(normal.X), MathF.Abs(normal.Y));
#else
            Vector2 distance = new Vector2(Math.Abs(normal.X), Math.Abs(normal.Y));
#endif
            normal = Vector2.Normalize(normal);
            depth = distance.Length();
            if (float.IsNaN(depth)) {
                normal = new Vector2(0, 0);
                depth = 0f;
                return false;
            }
            if (distance.X > b.Width / 2 + a.Radius / 2 || distance.Y > b.Height / 2 + a.Radius / 2)
                return false;
            if (distance.X <= b.Width / 2 || distance.Y <= b.Height / 2)
                return true;

#if NETSTANDARD2_1
            float distanceSq = MathF.Pow(distance.X - b.Width / 2, 2) + MathF.Pow(distance.Y - b.Height / 2, 2);
            return distanceSq <= MathF.Pow(a.Radius, 2);
#else
            double distanceSq = Math.Pow(distance.X - b.Width / 2, 2) + Math.Pow(distance.Y - b.Height / 2, 2);
            return distanceSq <= Math.Pow(a.Radius, 2);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Intersects(ref Circle a, ref RectangleF b)
        {
#if NETSTANDARD2_1
            Vector2 distance = new Vector2(MathF.Abs(a.Center.X - (b.X + b.Width / 2)), MathF.Abs(a.Center.Y - (b.Y + b.Height / 2)));
#else
            Vector2 distance = new Vector2(Math.Abs(a.Center.X - (b.X + b.Width / 2)), Math.Abs(a.Center.Y - (b.Y + b.Height / 2)));
#endif
            if (distance.X > b.Width / 2 + a.Radius / 2 || distance.Y > b.Height / 2 + a.Radius / 2)
                return false;
            if (distance.X <= b.Width / 2 || distance.Y <= b.Height / 2)
                return true;
#if NETSTANDARD2_1
            float distanceSq = MathF.Pow(distance.X - b.Width / 2, 2) + MathF.Pow(distance.Y - b.Height / 2, 2);
            return distanceSq <= MathF.Pow(a.Radius, 2);
#else
            double distanceSq = Math.Pow(distance.X - b.Width / 2, 2) + Math.Pow(distance.Y - b.Height / 2, 2);
            return distanceSq <= Math.Pow(a.Radius, 2);
#endif
        }

        public static bool operator ==(Circle a, Circle b) => a.Equals(b);

        public static bool operator !=(Circle a, Circle b) => !(a == b);

        public override bool Equals(object obj) => obj is Circle a && Equals(a);

        public bool Equals(Circle other)
        {
            return Center.Equals(other.Center) && Radius.Equals(other.Radius);
        }

        public override int GetHashCode() => HashCode.Combine(Radius, Center);

        public override string ToString()
        {
            return $"{{Center:{Center} Radius:{Radius} Diameter:{Diameter} Circumference:{Circumference}}}";
        }

    }

}
