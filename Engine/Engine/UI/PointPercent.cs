using System;

namespace SE.UI
{
    public struct PointPercent : IEquatable<PointPercent>
    {
        public float X;
        public float Y;

        public PointPercent(float x, float y)
        {
            X = x;
            Y = y;
        }

        public PointPercent Zero => new PointPercent(0, 0);
        public PointPercent Hundred => new PointPercent(100, 100);

        public static PointPercent operator +(PointPercent a, PointPercent b) => new PointPercent(a.X+b.X, a.Y+b.Y);

        public static PointPercent operator -(PointPercent a, PointPercent b) => new PointPercent(a.X - b.X, a.Y - b.Y);

        public static PointPercent operator *(PointPercent a, PointPercent b) => new PointPercent(a.X * b.X, a.Y * b.Y);

        public static PointPercent operator /(PointPercent a, PointPercent b) => new PointPercent(a.X / b.X, a.Y / b.Y);

        public static bool operator ==(PointPercent a, PointPercent b) => a.Equals(b);

        public static bool operator !=(PointPercent a, PointPercent b) => !a.Equals(b);

        public override bool Equals(object obj) => obj is PointPercent a && Equals(a);

        public bool Equals(PointPercent other) => X == other.X && Y == other.Y;

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public override string ToString() => $"{{X:{X} Y:{Y}}}";
    }

}