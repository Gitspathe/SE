using SE.Components;
using SE.Components.UI;
using SE.World.Partitioning;

namespace SE.Rendering
{
    public interface IRenderable : IPartitionObject<IRenderable>
    {
        void Render(Camera2D camera, Space space);
        RenderableInfo Info { get; }
    }

    public sealed class RenderableInfo
    {
        public RenderableTypeInfo RenderableTypeInfo;
        public int DrawCallID;
        public BlendMode BlendMode;

        internal ILit Lit;
        internal IUISprite UISprite;

        public RenderableInfo(IRenderable renderable)
        {
            RenderableTypeInfo = RenderableTypeLookup.Retrieve(renderable);
            Lit = renderable as ILit;
            UISprite = renderable as IUISprite;
        }
    }

    public enum Space
    {
        World,
        Screen
    }
}
