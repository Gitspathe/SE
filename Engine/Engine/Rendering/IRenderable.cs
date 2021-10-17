using SE.Components;
using SE.World.Partitioning;

namespace SE.Rendering
{
    /// <summary>
    /// Renderable interface.
    /// </summary>
    public interface IRenderable
    {
        void Render(Camera2D camera, Space space);
        Material Material { get; }
    }

    /// <summary>
    /// Renderable which can be within a spatial partition.
    /// </summary>
    public interface IPartitionedRenderable : IRenderable, IPartitionObject<IPartitionedRenderable> { }

    public enum Space
    {
        World,
        Screen
    }
}
