using System;
using System.Numerics;

namespace SE.Utility
{
    public static class Random
    {
        private static System.Random random = new System.Random();

        public static float Next(float max)
        {
            return (float)(random.NextDouble() * max);
        }

        public static int Next(int max)
        {
            return random.Next(max);
        }

        public static float Next(float min, float max)
        {
            return (float)random.NextDouble() * (max - min) + min;
        }

        public static int Next(int min, int max)
        {
            return random.Next(min, max);
        }

        public static float NextAngle()
        {
#if NETSTANDARD2_1
            return Next(-MathF.PI, MathF.PI);
#else
            return Next((float)-Math.PI, (float)Math.PI);
#endif
        }

        public static void NextUnitVector(out Vector2 vector)
        {
            float angle = NextAngle();
#if NETSTANDARD2_1
            vector = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
#else
            vector = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
#endif
        }
    }
}
