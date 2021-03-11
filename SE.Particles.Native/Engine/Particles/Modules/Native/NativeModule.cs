using System;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using SE.Utility;
using System.Numerics;
using SE.Engine.Utility;

namespace SE.Particles.Modules.Native
{
    // TODO.
    // TODO: Native module builder.
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "C++ interop")]
    public unsafe class NativeModule : ParticleModule, IDisposable
    {
        internal Module* NativeModulePtr;

        private bool isDisposed;

        public NativeModule()
        {
            NativeModulePtr = (Module*)nativeModule_Create();

            Curve alphaCurve = new Curve();
            alphaCurve.Keys.Add(0.0f, 0.0f);
            alphaCurve.Keys.Add(0.1f, 1.0f);
            alphaCurve.Keys.Add(0.667f, 1.0f);
            alphaCurve.Keys.Add(1.0f, 0.0f);
            //new NativeAlphaModule(this).SetCurve(alphaCurve);

            Curve4 colorCurve = new Curve4();
            colorCurve.Add(0.0f, new Vector4(0.0f, 1.0f, 0.5f, 1.0f));
            colorCurve.Add(1.0f, new Vector4(360.0f, 1.0f, 0.5f, 1.0f));

            for (int i = 0; i < 50; i++) {
                new NativeColorModule(this).SetCurve(colorCurve);
            }
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

    public unsafe class NativeSubmodule
    {
        internal Module* ModulePtr;
        internal Submodule* SubmodulePtr;

        internal NativeSubmodule(NativeModule module) => ModulePtr = module.NativeModulePtr;
    }
}
