using System;
using Vector2 = System.Numerics.Vector2;

namespace DeeZ.Core.Extensions
{
    public static class Vector2Extensions
    {
        public static float ToRotation(this Vector2 vector)
        {
#if NETSTANDARD2_1
            return MathF.Atan2(vector.X, -vector.Y);
#else
            return (float) Math.Atan2(vector.X, -vector.Y);
#endif
        }

        public static Microsoft.Xna.Framework.Vector2 ToMonoGameVector2(this Vector2 vector)
        {
            return new Microsoft.Xna.Framework.Vector2(vector.X, vector.Y);
        }

        public static Microsoft.Xna.Framework.Point ToPoint(this Vector2 vector)
        {
            return new Microsoft.Xna.Framework.Point((int)vector.X, (int)vector.Y);
        }

        public static Vector2 GetRotationVector(this float degrees, float length = 1.0f)
        {
#if NETSTANDARD2_1
            return new Vector2(MathF.Cos(degrees.ToRadians()) * length, MathF.Sin(degrees.ToRadians()) * length);
#else
            return new Vector2((float)Math.Cos(degrees.ToRadians()) * length, (float)Math.Sin(degrees.ToRadians()) * length);
#endif
        }

        public static float GetRotationFacing(this Vector2 srcPos, Vector2 destPos)
        {
            Vector2 vec = srcPos - destPos;
#if NETSTANDARD2_1
            return MathF.Atan2(vec.Y, vec.X);
#else
            return (float)Math.Atan2(vec.Y, vec.X);
#endif
        }
    }

}
