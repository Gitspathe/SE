using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using SE.Utility;

namespace SE.Particles.Modules.Native
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "C++ interop")]
    public sealed unsafe class NativeSpriteRotationModule : NativeSubmodule
    {
        internal NativeSpriteRotationModule(NativeModule module) : base(module)
        {
            SubmodulePtr = nativeModule_SpriteRotationModule_Ctor(ModulePtr);
        }

        public void SetNone() => nativeModule_SpriteRotationModule_SetNone(SubmodulePtr);
        public void SetLerp(float start, float end) => nativeModule_SpriteRotationModule_SetLerp(SubmodulePtr, start, end);
        public void SetConstant(float val) => nativeModule_SpriteRotationModule_SetConstant(SubmodulePtr, val);
        public void SetRandomConstant(float min, float max) => nativeModule_SpriteRotationModule_SetRandomConstant(SubmodulePtr, min, max);

        public void SetCurve(Curve curve)
            => nativeModule_SpriteRotationModule_SetCurve(SubmodulePtr, NativeUtil.CopyCurveToNativeCurve(curve));
        public void SetRandomCurve(Curve curve) 
            => nativeModule_SpriteRotationModule_SetRandomCurve(SubmodulePtr, NativeUtil.CopyCurveToNativeCurve(curve));

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Submodule* nativeModule_SpriteRotationModule_Ctor(Module* modulePtr);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_SpriteRotationModule_SetNone(Submodule* modulePtr);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_SpriteRotationModule_SetConstant(Submodule* modulePtr, float val);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_SpriteRotationModule_SetLerp(Submodule* modulePtr, float start, float end);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_SpriteRotationModule_SetCurve(Submodule* modulePtr, NativeCurve* curvePtr);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_SpriteRotationModule_SetRandomConstant(Submodule* modulePtr, float min, float max);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_SpriteRotationModule_SetRandomCurve(Submodule* modulePtr, NativeCurve* curvePtr);
    }
}
