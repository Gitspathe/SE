using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Core;
using SE.Rendering;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Quaternion = System.Numerics.Quaternion;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace SE.NeoRenderer
{
    // TODO: Allow this to be an Asset somehow?

    // Name this SpriteMaterial for now, to specify that it's ONLY for sprites.
    // Might change later.
    public sealed class SpriteMaterial : IDisposable
    {
        internal int RefCount = 0;

        public SpriteEffect Effect {
            get => effect;
            
            set {
                if (effect == value)
                    return;

                effect = value;
                SpriteMaterialHandler.MaterialPropertyChanged(this);
            }
        }
        private SpriteEffect effect;

        public Texture2D MainTexture {
            get => mainTexture ?? SpriteBatchManager.NullTexture;

            set {
                if (mainTexture == value)
                    return;

                mainTexture = value;
                SpriteMaterialHandler.MaterialPropertyChanged(this);
            }
        }
        private Texture2D mainTexture;

        public uint RenderQueue {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => RenderQueueInternal;

            set {
                if (value == RenderQueueInternal)
                    return;
                if (!useCustomRenderQueue)
                    return;

                RenderQueueInternal = value;
                SpriteMaterialHandler.MaterialPropertyChanged(this);
            }
        }
        internal uint RenderQueueInternal;

        public BlendMode BlendMode {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => BlendModeInternal;

            set {
                if (value == BlendModeInternal)
                    return;

                BlendModeInternal = value;
                SpecialRenderOrderingInternal = BlendModeInternal == BlendMode.Transparent || BlendMode == BlendMode.Additive;
                if (!useCustomRenderQueue) {
                    CalculateRenderQueue();
                }
                SpriteMaterialHandler.MaterialPropertyChanged(this);
            }
        }
        internal BlendMode BlendModeInternal = BlendMode.Opaque;

        public bool IgnoreLighting {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IgnoreLightingInternal;

            set {
                if (value == IgnoreLightingInternal)
                    return;

                IgnoreLightingInternal = value;
                if (!useCustomRenderQueue) {
                    CalculateRenderQueue();
                }
                SpriteMaterialHandler.MaterialPropertyChanged(this);
            }
        }
        internal bool IgnoreLightingInternal;

        public bool UseCustomRenderQueue {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => useCustomRenderQueue;

            set {
                if (value == useCustomRenderQueue)
                    return;

                useCustomRenderQueue = value;
                if (!useCustomRenderQueue) {
                    CalculateRenderQueue();
                }
                SpriteMaterialHandler.MaterialPropertyChanged(this);
            }
        }
        private bool useCustomRenderQueue;

        public bool SpecialRenderOrdering => SpecialRenderOrderingInternal;
        internal bool SpecialRenderOrderingInternal;

        internal static Texture2D NullTexture;
        
        private bool disposed;

        public SpriteMaterial()
        {
            CalculateRenderQueue();
            SpriteMaterialHandler.MaterialCreated(this);
        }

        private void CalculateRenderQueue()
        {
            uint renderIndex = (uint)(IgnoreLightingInternal ? RenderLoop.LoopEnum.AfterLighting : RenderLoop.LoopEnum.DuringLighting);
            switch (BlendModeInternal) {
                case BlendMode.Opaque:
                    renderIndex += 100;
                    break;
                case BlendMode.Transparent:
                    renderIndex += 1000;
                    break;
                case BlendMode.Additive:
                    renderIndex += 2000;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            RenderQueueInternal = renderIndex;
        }

        // DELETE THESE!
        // Doesn't follow the design vision.
        public void SetParameter(string parameter, bool value)
        {
            effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, int value)
        {
            effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, int[] value)
        {
            effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Matrix value)
        {
            effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Matrix[] value)
        {
            effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Matrix4x4 value)
        {
            effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Matrix4x4[] value)
        {
            effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, float value)
        {
            effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, float[] value)
        {
            effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Texture value)
        {
            effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Vector2 value)
        {
            effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Vector2[] value)
        {
            effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Vector3 value)
        {
            effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Vector3[] value)
        {
            effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Vector4 value)
        {
            effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Vector4[] value)
        {
            effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Quaternion value)
        {
            effect.Parameters[parameter].SetValue(value);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed) {
                if (disposing) {
                    SpriteMaterialHandler.MaterialDispose(this);
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        // TODO: MonoGame vector types?

    }
}
