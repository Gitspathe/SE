using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Components;
using SE.Components.UI;
using SE.Core;
using SE.World.Partitioning;
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
    public interface IRenderable : IPartitionObject<IRenderable>
    {
        void Render(Camera2D camera, Space space);
        Material Material { get; }
    }

    // TODO: Allow this to be an Asset somehow?
    public sealed class Material
    {
        public RenderableTypeInfo RenderableTypeInfo;

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

        public BlendMode BlendMode;

        internal int DrawCallID;
        internal ILit Lit;
        internal IUISprite UISprite;

        public Material(IRenderable renderable)
        {
            RenderableTypeInfo = RenderableTypeLookup.Retrieve(renderable);
            Lit = renderable as ILit;
            UISprite = renderable as IUISprite;
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

    public enum Space
    {
        World,
        Screen
    }
}
