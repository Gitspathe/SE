using SE.Components;
using SE.Components.UI;
using SE.World.Partitioning;

namespace SE.Rendering
{
    public interface IRenderable : IPartitionObject<IRenderable>
    {
        void Render(Camera2D camera, Space space);
        int DrawCallID { get; }
        RenderableTypeInfo RenderableTypeInfo { get; }
        BlendMode BlendMode { get; }
    }

    public enum Space
    {
        World,
        Screen
    }
}
