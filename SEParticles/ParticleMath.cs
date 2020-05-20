using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace SEParticles
{
    public static class ParticleMath
    {
        public static float PI = MathF.PI;
        public static float PIOver180 = MathF.PI / 180;
        public static float _180OverPI = 180 / MathF.PI;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float value1, float value2, float amount)
            => value1 + (value2 - value1) * amount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap(ref float x, ref float y)
        {
            float tmpX = x;
            x = y;
            y = tmpX;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Between(float min, float max, float ratio) 
            => (min + ((max - min) * ratio));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToRadians(float degrees)
            => PIOver180 * degrees;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToDegrees(float radians)
            => radians * _180OverPI;

        // Color helper methods are thanks to Jiagg.
        // https://github.com/Jjagg/MgMercury/blob/master/Core/ColorHelper.cs

        public static Vector4 ToRgba(this Vector4 hsl) 
            => ToRgba(hsl.X, hsl.Y, hsl.Z, hsl.W);

        private const float OneOverThree = 1.0f / 3.0f;
        private static Vector4 ToRgba(float h, float s, float l, float a)
        {
            if (s == 0)
                return new Vector4(l, l, l, a);

            h /= 360.0f;
            float max = l < 0.5f ? l * (1 + s) : (l + s) - (l * s);
            float min = 2f * l - max;

            return new Vector4(
                ComponentFromHue(min, max, h + OneOverThree),
                ComponentFromHue(min, max, h),
                ComponentFromHue(min, max, h - OneOverThree),
                a);
        }

        private const float TwoOverThree = 2.0f / 3.0f;
        private const float OneOverTwo = 1.0f / 2.0f;
        private const float OneOverSix = 1.0f / 6.0f;

        // https://github.com/craftworkgames/MonoGame.Extended/blob/develop/Source/MonoGame.Extended/ColorHelper.cs

        private static float ComponentFromHue(float p, float q, float t)
        {
            if (t < 0.0f) 
                t += 1.0f;
            if (t > 1.0f) 
                t -= 1.0f;

            if (t < OneOverSix) 
                return p + (q - p) * 6.0f * t;
            if (t < OneOverTwo) 
                return q;
            if (t < TwoOverThree) 
                return p + (q - p) * (TwoOverThree - t) * 6.0f;
            
            return p;
        }

        public static Vector4 ToHsl(this Vector4 rgba) 
            => ToHsl(rgba.X, rgba.Z, rgba.Y, rgba.W);

        private static Vector4 ToHsl(float r, float b, float g, float a)
        {
            r /= 255.0f;
            b /= 255.0f;
            g /= 255.0f;

            float max = MathF.Max(MathF.Max(r, g), b);
            float min = MathF.Min(MathF.Min(r, g), b);
            float chroma = max - min;
            float sum = max + min;

            float l = sum * 0.5f;

            if (chroma == 0) 
                return new Vector4(0f, 0f, l, a);

            float h;
            if (r == max)
                h = (60 * (g - b) / chroma + 360) % 360;
            else if (g == max)
                h = 60 * (b - r) / chroma + 120f;
            else
                h = 60 * (r - g) / chroma + 240f;

            float s = l <= 0.5f ? chroma / sum : chroma / (2f - sum);
            return new Vector4(h, s, l, a);
        }
    }
}
