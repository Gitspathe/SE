using System;

namespace SE.Core.Extensions
{
    public static class NumericExtensions
    {
#if NETSTANDARD2_1
        public const float _PI_OVER180 = MathF.PI / 180;
#else
        public const float _PI_OVER180 = (float)Math.PI / 180;
#endif

        public static double ToRadians(this double val)
        {
            return _PI_OVER180 * val;
        }

        public static float ToRadians(this float val)
        {
            return _PI_OVER180 * val;
        }

        public static int Round(this int i, int increment)
        {
            return ((int)(i / (float)increment) * increment);
        }
    }
}
