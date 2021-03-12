using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using SE.Utility;

namespace SE.Particles.Modules.Native
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "C++ interop")]
    public sealed unsafe class NativeTextureAnimationModule : NativeSubmodule
    {
        private int sheetRows = 1;
        private int sheetColumns = 1;

        internal NativeTextureAnimationModule(NativeModule module) : base(module)
        {
            SubmodulePtr = nativeModule_TextureAnimationModule_Ctor(ModulePtr);
        }

        protected internal override void PreUpdate()
        {
            Vector2 texSize = Emitter.TextureSize;
            nativeModule_TextureAnimationModule_SetTextureSize(SubmodulePtr, new NativeVector2(texSize.X, texSize.Y));
        }

        protected internal override void PreInitialize()
        {
            ApplyToEmitter();
        }

        private void ApplyToEmitter()
        {
            if (Emitter == null)
                return;

            Emitter.ParticleSize = new Int2(
                (int)Emitter.TextureSize.X / sheetColumns,
                (int)Emitter.TextureSize.Y / sheetRows);
        }

        public void SetOverLifetime(int sheetRows, int sheetColumns)
        {
            this.sheetRows = sheetRows;
            this.sheetColumns = sheetColumns;
            ApplyToEmitter();
            nativeModule_TextureAnimationModule_SetOverLifetime(SubmodulePtr, sheetRows, sheetColumns);
        }

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Submodule* nativeModule_TextureAnimationModule_Ctor(Module* modulePtr);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_TextureAnimationModule_SetTextureSize(Submodule* modulePtr, NativeVector2 textureSize);

        [DllImport("SE.Native", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nativeModule_TextureAnimationModule_SetOverLifetime(Submodule* modulePtr, int sheetRows, int sheetColumns);
    }
}
