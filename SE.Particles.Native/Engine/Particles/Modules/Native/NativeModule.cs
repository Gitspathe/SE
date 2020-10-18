using System;
using System.Runtime.InteropServices;

namespace SE.Particles.Modules.Native
{
    // TODO.
    // TODO: Native module builder.
    public unsafe class NativeModule : ParticleModule, IDisposable
    {
        internal Module* NativeModulePtr;

        private bool isDisposed;

        public NativeModule()
        {
            NativeModulePtr = (Module*)nativeModule_Create();
            
            NativeAlphaModule alpha = new NativeAlphaModule(this);
            alpha.SetLerp(0.0f);
        }

        public override void OnParticlesActivated(Span<int> particlesIndex)
        {
            fixed (int* indexPtr = particlesIndex) {
                nativeModule_OnParticlesActivated(NativeModulePtr, indexPtr, Emitter.GetParticlePointer(), particlesIndex.Length);
            }
        }

        public override void OnUpdate(float deltaTime, Particle* arrayPtr, int length)
        {
            nativeModule_OnUpdate(NativeModulePtr, deltaTime, arrayPtr, length);
        }

        public override void OnInitialize()
        {
            nativeModule_OnInitialize(NativeModulePtr, Emitter.ParticlesLength);
        }

        public override ParticleModule DeepCopy()
        {
            return null;
        }

        ~NativeModule()
        {
            Dispose();
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            nativeModule_Delete(NativeModulePtr);
            isDisposed = true;
        }

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void* nativeModule_Create();

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_AddSubmodule(Module* modulePtr, Submodule* submodulePtr);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_RemoveSubmodule(Module* modulePtr, Submodule* submodulePtr);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_OnInitialize(Module* modulePtr, int particleArrayLength);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_OnParticlesActivated(Module* modulePtr, int* particleIndexArr, Particle* particleArrPtr, int length);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_OnUpdate(Module* modulePtr, float deltaTime, Particle* particleArrPtr, int length);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_Delete(Module* modulePtr);
    }

    internal struct Module { /* Used for naming only! */ }
    internal struct Submodule { /* Used for naming only! */ }

    public unsafe class NativeSubmodule
    {
        internal Module* ModulePtr;
        internal Submodule* SubmodulePtr;

        internal NativeSubmodule(NativeModule module) => ModulePtr = module.NativeModulePtr;
    }
}
