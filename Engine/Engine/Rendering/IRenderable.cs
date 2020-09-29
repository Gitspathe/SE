using SE.Components;
using SE.Components.UI;
using SE.World.Partitioning;

namespace SE.Rendering
{
    /// <summary>
    /// Generic renderable interface.
    /// </summary>
    public interface IRenderable
    {
        void Render(Camera2D camera, Space space);
        RenderableData Data { get; }
    }

    /// <summary>
    /// Renderable which can be within a spatial partition.
    /// </summary>
    public interface IPartitionedRenderable : IRenderable, IPartitionObject<IPartitionedRenderable> { }

    public sealed class RenderableData
    {
        public Material Material;
        public RenderableTypeInfo TypeInfo;

        internal ILit Lit;
        internal IUISprite UISprite;

        public RenderableData(IRenderable renderable, Material material = null)
        {
            Material = material ?? new Material();
            TypeInfo = RenderableTypeLookup.Retrieve(renderable);
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
