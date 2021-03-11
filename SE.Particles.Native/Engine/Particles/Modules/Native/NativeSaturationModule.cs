using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using SE.Utility;

namespace SE.Particles.Modules.Native
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "C++ interop")]
    public sealed unsafe class NativeSaturationModule : NativeSubmodule
    {
        internal NativeSaturationModule(NativeModule module) : base(module)
        {
            SubmodulePtr = nativeModule_SaturationModule_Ctor(ModulePtr);
        }

        public void SetNone() => nativeModule_SaturationModule_SetNone(SubmodulePtr);
        public void SetLerp(float end) => nativeModule_SaturationModule_SetLerp(SubmodulePtr, end);
        public void SetRandomLerp(float min, float max) => nativeModule_SaturationModule_SetRandomLerp(SubmodulePtr, min, max);

        public void SetCurve(Curve curve)
        {
            nativeModule_SaturationModule_SetCurve(SubmodulePtr, NativeUtil.CopyCurveToNativeCurve(curve));
        }

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Submodule* nativeModule_SaturationModule_Ctor(Module* modulePtr);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_SaturationModule_SetNone(Submodule* modulePtr);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_SaturationModule_SetLerp(Submodule* modulePtr, float end);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_SaturationModule_SetRandomLerp(Submodule* modulePtr, float min, float max);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_SaturationModule_SetCurve(Submodule* modulePtr, NativeCurve* curvePtr);
    }
}
