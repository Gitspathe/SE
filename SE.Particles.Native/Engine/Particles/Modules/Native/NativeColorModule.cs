using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using SE.Engine.Utility;
using SE.Utility;

namespace SE.Particles.Modules.Native
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "C++ interop")]
    public sealed unsafe class NativeColorModule : NativeSubmodule
    {
        internal NativeColorModule(NativeModule module) : base(module)
        {
            SubmodulePtr = nativeModule_ColorModule_Ctor(ModulePtr);
        }

        public void SetNone() => nativeModule_ColorModule_SetNone(SubmodulePtr);

        public void SetLerp(Vector4 end) 
            => nativeModule_ColorModule_SetLerp(SubmodulePtr, 
                new NativeVector4(end.X, end.Y, end.Z, end.W));

        public void SetRandomLerp(Vector4 min, Vector4 max) 
            => nativeModule_ColorModule_SetRandomLerp(SubmodulePtr, 
                new NativeVector4(min.X, min.Y, min.Z, min.W), 
                new NativeVector4(max.X, max.Y, max.Z, max.W));

        public void SetCurve(Curve4 curve)
        {
            nativeModule_ColorModule_SetCurve(SubmodulePtr, NativeUtil.CopyCurve4ToNativeCurve4(curve));
        }

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Submodule* nativeModule_ColorModule_Ctor(Module* modulePtr);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_ColorModule_SetNone(Submodule* modulePtr);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_ColorModule_SetLerp(Submodule* modulePtr, NativeVector4 end);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_ColorModule_SetRandomLerp(Submodule* modulePtr, NativeVector4 min, NativeVector4 max);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_ColorModule_SetCurve(Submodule* modulePtr, NativeCurve4* curvePtr);
    }
}
