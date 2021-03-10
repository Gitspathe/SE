using System.Runtime.InteropServices;

namespace SE.Particles
{
    internal static unsafe class NativeUtil
    {
        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern NativeCurve* util_Curve_Ctor();

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void util_Curve_Add(NativeCurve* curvePtr, float position, float value);
    }
}
