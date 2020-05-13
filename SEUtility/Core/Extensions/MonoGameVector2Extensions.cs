using System;
using Microsoft.Xna.Framework;

namespace DeeZ.Core.Extensions
{
    public static class MonoGameVector2Extensions
    {

        public static float ToRotation(this Vector2 vector)
        {
#if NETSTANDARD2_1
            float angle = MathF.Atan2(vector.X, -vector.Y);
#else
            float angle = (float)Math.Atan2(vector.X, -vector.Y);
#endif
            return angle;
        }

        public static System.Numerics.Vector2 ToNumericsVector2(this Vector2 vector)
        {
            return new System.Numerics.Vector2(vector.X, vector.Y);
        }

        public static System.Numerics.Vector2 ToNumericsVector2(this Point point)
        {
            return new System.Numerics.Vector2(point.X, point.Y);
        }

    }

}