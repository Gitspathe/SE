using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using SE.Engine.Utility;
using SE.Utility;

namespace SE.Particles
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "C++ interop")]
    internal static unsafe class NativeUtil
    {
        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern NativeCurve* util_Curve_Ctor();

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void util_Curve_Add(NativeCurve* curvePtr, float position, float value);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern NativeCurve2* util_Curve2_Ctor(NativeCurve* x, NativeCurve* y);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern NativeCurve3* util_Curve3_Ctor(NativeCurve* x, NativeCurve* y, NativeCurve* z);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern NativeCurve4* util_Curve4_Ctor(NativeCurve* x, NativeCurve* y, NativeCurve* z, NativeCurve* w);

        internal static NativeCurve* CopyCurveToNativeCurve(Curve managedCurve)
        {
            NativeCurve* nativeCurve = util_Curve_Ctor();
            foreach (CurveKey key in managedCurve.Keys) {
                util_Curve_Add(nativeCurve, key.Position, key.Value);
            }
            return nativeCurve;
        }

        internal static NativeCurve2* CopyCurve2ToNativeCurve2(Curve2 managedCurve)
        {
            NativeCurve* x = CopyCurveToNativeCurve(managedCurve.X);
            NativeCurve* y = CopyCurveToNativeCurve(managedCurve.Y);
            return util_Curve2_Ctor(x, y);
        }

        internal static NativeCurve3* CopyCurve3ToNativeCurve3(Curve3 managedCurve)
        {
            NativeCurve* x = CopyCurveToNativeCurve(managedCurve.X);
            NativeCurve* y = CopyCurveToNativeCurve(managedCurve.Y);
            NativeCurve* z = CopyCurveToNativeCurve(managedCurve.Z);
            return util_Curve3_Ctor(x, y, z);
        }

        internal static NativeCurve4* CopyCurve4ToNativeCurve4(Curve4 managedCurve)
        {
            NativeCurve* x = CopyCurveToNativeCurve(managedCurve.X);
            NativeCurve* y = CopyCurveToNativeCurve(managedCurve.Y);
            NativeCurve* z = CopyCurveToNativeCurve(managedCurve.Z);
            NativeCurve* w = CopyCurveToNativeCurve(managedCurve.W);
            return util_Curve4_Ctor(x, y, z, w);
        }
    }
}
