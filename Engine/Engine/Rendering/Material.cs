using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Components.UI;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;
using Quaternion = System.Numerics.Quaternion;

using MGVector2 = Microsoft.Xna.Framework.Vector2;
using MGVector3 = Microsoft.Xna.Framework.Vector3;
using MGVector4 = Microsoft.Xna.Framework.Vector4;
using MGQuaternion = Microsoft.Xna.Framework.Quaternion;

namespace SE.Rendering
{
    // TODO: Allow this to be an Asset somehow?
    // TODO: What about Materials without textures?? (i.e non SpriteBatch stuff.)
    public class Material
    {
        public Texture2D Texture {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => texture;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                texture = value;
                RegenerateDrawCall();
            }
        }
        private Texture2D texture;

        public Effect Effect {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => effect;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                effect = value;
                RegenerateDrawCall();
            }
        }
        private Effect effect;
        private bool isSharedEffect = true;

        public uint RenderQueue {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => renderQueue;

            set {
                if (value == renderQueue)
                    return;
                if (!useCustomRenderQueue)
                    return;

                renderQueue = value;
                RegenerateDrawCall();
            }
        }
        private uint renderQueue;

        public BlendMode BlendMode {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => blendMode;

            set {
                if (value == blendMode)
                    return;

                blendMode = value;
                if (!useCustomRenderQueue)
                {
                    CalculateRenderQueue();
                }
            }
        }
        private BlendMode blendMode = BlendMode.Opaque;

        public bool IgnoreLighting
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ignoreLighting;

            set {
                if (value == ignoreLighting)
                    return;

                ignoreLighting = value;
                if (!useCustomRenderQueue) {
                    CalculateRenderQueue();
                }
            }
        }
        private bool ignoreLighting;

        public bool UseCustomRenderQueue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => useCustomRenderQueue;

            set {
                if (value == useCustomRenderQueue)
                    return;

                useCustomRenderQueue = value;
                if (!useCustomRenderQueue) {
                    CalculateRenderQueue();
                }
            }
        }
        private bool useCustomRenderQueue;

        public bool RequiresUnordered
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => BlendMode == BlendMode.Transparent || BlendMode == BlendMode.Additive;
        }

        internal int DrawCallID;

        public Material()
        {
            // TODO: Empty/invalid material. Need a freaky looking texture to scare developers.
            CalculateRenderQueue();
        }

        public Material(Texture2D texture, Effect effect = null)
        {
            this.texture = texture;
            this.effect = effect;
            CalculateRenderQueue();
            RegenerateDrawCall();
        }

        private void CalculateRenderQueue()
        {
            uint renderIndex = (uint)(ignoreLighting ? RenderLoop.LoopEnum.AfterLighting : RenderLoop.LoopEnum.DuringLighting);
            switch (blendMode) {
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
            renderQueue = renderIndex;

            RegenerateDrawCall();
        }

        private void CloneEffectIfNeeded()
        {
            if(isSharedEffect)
                return;

            isSharedEffect = false;
            Effect = Effect.Clone();
        }

        public void SetParameter(string parameter, bool value)
        {
            CloneEffectIfNeeded();
            Effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, int value)
        {
            CloneEffectIfNeeded();
            Effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, int[] value)
        {
            CloneEffectIfNeeded();
            Effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Matrix value)
        {
            CloneEffectIfNeeded();
            Effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Matrix[] value)
        {
            CloneEffectIfNeeded();
            Effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Matrix4x4 value)
        {
            CloneEffectIfNeeded();
            Effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Matrix4x4[] value)
        {
            CloneEffectIfNeeded();
            Effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, float value)
        {
            CloneEffectIfNeeded();
            Effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, float[] value)
        {
            CloneEffectIfNeeded();
            Effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Texture value)
        {
            CloneEffectIfNeeded();
            Effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Vector2 value)
        {
            CloneEffectIfNeeded();
            Effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Vector2[] value)
        {
            CloneEffectIfNeeded();
            Effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Vector3 value)
        {
            CloneEffectIfNeeded();
            Effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Vector3[] value)
        {
            CloneEffectIfNeeded();
            Effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Vector4 value)
        {
            CloneEffectIfNeeded();
            Effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Vector4[] value)
        {
            CloneEffectIfNeeded();
            Effect.Parameters[parameter].SetValue(value);
        }

        public void SetParameter(string parameter, Quaternion value)
        {
            CloneEffectIfNeeded();
            Effect.Parameters[parameter].SetValue(value);
        }

        // TODO: MonoGame vector types?

        public void RegenerateDrawCall()
        {
            if(Texture != null)
                DrawCallID = DrawCallDatabase.TryGetID(new DrawCall(texture, Effect));
        }
    }
}
