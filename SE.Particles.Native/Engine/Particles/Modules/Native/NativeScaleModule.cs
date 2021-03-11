using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using SE.Utility;

namespace SE.Particles.Modules.Native
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "C++ interop")]
    public sealed unsafe class NativeScaleModule : NativeSubmodule
    {
        internal NativeScaleModule(NativeModule module) : base(module)
        {
            SubmodulePtr = nativeModule_ScaleModule_Ctor(ModulePtr);
        }

        public bool AbsoluteValue {
            get => nativeModule_ScaleModule_GetAbsoluteValue(SubmodulePtr);
            set => nativeModule_ScaleModule_SetAbsoluteValue(SubmodulePtr, value);
        }

        public void SetNone() => nativeModule_ScaleModule_SetNone(SubmodulePtr);
        public void SetLerp(float start, float end) => nativeModule_ScaleModule_SetLerp(SubmodulePtr, start, end);

        public void SetCurve(Curve curve)
        {
            nativeModule_ScaleModule_SetCurve(SubmodulePtr, NativeUtil.CopyCurveToNativeCurve(curve));
        }

        public void SetRandomCurve(Curve curve)
        {
            nativeModule_ScaleModule_SetRandomCurve(SubmodulePtr, NativeUtil.CopyCurveToNativeCurve(curve));
        }

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Submodule* nativeModule_ScaleModule_Ctor(Module* modulePtr);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool nativeModule_ScaleModule_GetAbsoluteValue(Submodule* modulePtr);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_ScaleModule_SetAbsoluteValue(Submodule* modulePtr, bool val);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_ScaleModule_SetNone(Submodule* modulePtr);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_ScaleModule_SetLerp(Submodule* modulePtr, float start, float end);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_ScaleModule_SetCurve(Submodule* modulePtr, NativeCurve* curvePtr);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_ScaleModule_SetRandomCurve(Submodule* modulePtr, NativeCurve* curvePtr);
    }
}
