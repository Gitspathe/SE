using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using SE.Utility;

namespace SE.Particles.Modules.Native
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "C++ interop")]
    public sealed unsafe class NativeSpeedModule : NativeSubmodule
    {
        internal NativeSpeedModule(NativeModule module) : base(module)
        {
            SubmodulePtr = nativeModule_SpeedModule_Ctor(ModulePtr);
        }

        public bool AbsoluteValue {
            get => nativeModule_SpeedModule_GetAbsoluteValue(SubmodulePtr);
            set => nativeModule_SpeedModule_SetAbsoluteValue(SubmodulePtr, value);
        }

        public void SetNone() => nativeModule_SpeedModule_SetNone(SubmodulePtr);
        public void SetLerp(float start, float end) => nativeModule_SpeedModule_SetLerp(SubmodulePtr, start, end);

        public void SetCurve(Curve curve)
        {
            nativeModule_SpeedModule_SetCurve(SubmodulePtr, NativeUtil.CopyCurveToNativeCurve(curve));
        }

        public void SetRandomCurve(Curve curve)
        {
            nativeModule_SpeedModule_SetRandomCurve(SubmodulePtr, NativeUtil.CopyCurveToNativeCurve(curve));
        }

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Submodule* nativeModule_SpeedModule_Ctor(Module* modulePtr);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool nativeModule_SpeedModule_GetAbsoluteValue(Submodule* modulePtr);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_SpeedModule_SetAbsoluteValue(Submodule* modulePtr, bool val);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_SpeedModule_SetNone(Submodule* modulePtr);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_SpeedModule_SetLerp(Submodule* modulePtr, float start, float end);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_SpeedModule_SetCurve(Submodule* modulePtr, NativeCurve* curvePtr);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_SpeedModule_SetRandomCurve(Submodule* modulePtr, NativeCurve* curvePtr);
    }
}
