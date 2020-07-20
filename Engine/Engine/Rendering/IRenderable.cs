using SE.Components;
using SE.World.Partitioning;

namespace SE.Rendering
{
    public interface IRenderable : IPartitionObjectExtended<IRenderable>
    {
        void Render(Camera2D camera, Space space);
        int DrawCallID { get; }
        BlendMode BlendMode { get; }
    }

    public enum Space
    {
        World,
        Screen
    }
}
