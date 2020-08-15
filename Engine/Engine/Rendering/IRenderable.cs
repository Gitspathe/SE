using System;
using Microsoft.Xna.Framework.Graphics;
using SE.Components;
using SE.Components.UI;
using SE.Core;
using SE.World.Partitioning;

namespace SE.Rendering
{
    public interface IRenderable : IPartitionObject<IRenderable>
    {
        void Render(Camera2D camera, Space space);
        Material Material { get; }
    }

    // TODO: Allow to clone effects when shader parameters are changed.
    public sealed class Material
    {
        public RenderableTypeInfo RenderableTypeInfo;

        public Texture2D Texture {
            get => texture;
            set {
                texture = value;
                RegenerateDrawCall();
            }
        }
        private Texture2D texture;

        public Effect Effect {
            get => effect;
            set {
                effect = value;
                RegenerateDrawCall();
            }
        }
        private Effect effect;

        public int DrawCallID;
        public BlendMode BlendMode;

        internal ILit Lit;
        internal IUISprite UISprite;

        public Material(IRenderable renderable)
        {
            RenderableTypeInfo = RenderableTypeLookup.Retrieve(renderable);
            Lit = renderable as ILit;
            UISprite = renderable as IUISprite;
        }

        public void RegenerateDrawCall()
        {
            if(Texture != null)
                DrawCallID = DrawCallDatabase.TryGetID(new DrawCall(Texture, Effect));
        }
    }

    public enum Space
    {
        World,
        Screen
    }
}
