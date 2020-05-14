using System;
using System.Runtime.CompilerServices;
using Vector2 = System.Numerics.Vector2;

namespace SE.Core.Extensions
{

    public static class FloatExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToDirectionVector(this float angle)
        {
#if NETSTANDARD2_1
            return new Vector2(MathF.Sin(angle), -MathF.Cos(angle));
#else
            return new Vector2((float)Math.Sin(angle), (float)-Math.Cos(angle));
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToDirectionVector(this float angle, out Vector2 vector)
        {
#if NETSTANDARD21
            vector = new Vector2(MathF.Sin(angle), -MathF.Cos(angle));
#else
            vector = new Vector2((float)Math.Sin(angle), (float)-Math.Cos(angle));
#endif
        }

    }

}
