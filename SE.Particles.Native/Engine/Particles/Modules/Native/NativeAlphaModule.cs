using System.Runtime.InteropServices;

namespace SE.Particles.Modules.Native
{
    public sealed unsafe class NativeAlphaModule : NativeSubmodule
    {
        internal NativeAlphaModule(NativeModule module) : base(module)
        {
            SubmodulePtr = nativeModule_AlphaModule_Ctor(ModulePtr);
        }

        public void SetNone() => nativeModule_AlphaModule_SetNone(SubmodulePtr);
        public void SetLerp(float end) => nativeModule_AlphaModule_SetLerp(SubmodulePtr, end);
        public void SetRandomLerp(float min, float max) => nativeModule_AlphaModule_SetRandomLerp(SubmodulePtr, min, max);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Submodule* nativeModule_AlphaModule_Ctor(Module* modulePtr);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_AlphaModule_SetNone(Submodule* modulePtr);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_AlphaModule_SetLerp(Submodule* modulePtr, float end);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_AlphaModule_SetRandomLerp(Submodule* modulePtr, float min, float max);
    }
}
