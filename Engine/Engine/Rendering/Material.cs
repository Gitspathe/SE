using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Core;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Quaternion = System.Numerics.Quaternion;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace SE.Rendering
{
    // TODO: Allow this to be an Asset somehow?
    // TODO: What about Materials without textures?? (i.e non SpriteBatch stuff.)
    // TODO: Renderers should hold SharedMaterial and Material, like Unity. (How will this work with SpriteBatch?)

    // TODO: Get rid of the draw call ID stuff and just use the Material itself as the ID. Cloning should not be done automatically!
    public sealed class Material
    {
        public Texture2D Texture {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => TextureInternal;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                TextureInternal = value;
                RegenerateDrawCall();
            }
        }
        internal Texture2D TextureInternal;

        public Effect Effect {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => EffectInternal;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                EffectInternal = value;
                RegenerateDrawCall();
            }
        }
        internal Effect EffectInternal;
        private bool isSharedEffect = true;

        public uint RenderQueue {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => RenderQueueInternal;

            set {
                if (value == RenderQueueInternal)
                    return;
                if (!useCustomRenderQueue)
                    return;

                RenderQueueInternal = value;
                RegenerateDrawCall();
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
                RequiresUnorderedInternal = BlendModeInternal == BlendMode.Transparent || BlendMode == BlendMode.Additive;
                if (!useCustomRenderQueue) {
                    CalculateRenderQueue();
                }
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
            }
        }
        private bool useCustomRenderQueue;

        // TODO: Might be a good idea to move TextureSourceRectangle from Sprite to Material.

        public bool RequiresUnordered => RequiresUnorderedInternal;
        internal bool RequiresUnorderedInternal;

        internal int DrawCallID;

        internal static Texture2D NullTexture;

        public Material()
        {
            // TODO: Empty/invalid material. Need a freaky looking texture to scare developers.
            if (!Screen.IsFullHeadless && NullTexture == null) {
                NullTexture = new Texture2D(GameEngine.Engine.GraphicsDevice, 1, 1);
                NullTexture.SetData(new Color[] { Color.Purple });
            }
            TextureInternal = NullTexture;
            CalculateRenderQueue();
        }

        public Material(Texture2D texture, Effect effect = null)
        {
            if (texture == null) {
                texture = NullTexture;
            }
            TextureInternal = texture;
            EffectInternal = effect;
            CalculateRenderQueue();
            RegenerateDrawCall();
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

            RegenerateDrawCall();
        }

        private void CloneEffectIfNeeded()
        {
            if (isSharedEffect)
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
            if (Texture != null) {
                DrawCallID = DrawCallDatabase.TryGetID(new DrawCall(TextureInternal, Effect));
            }
        }

    }
}
