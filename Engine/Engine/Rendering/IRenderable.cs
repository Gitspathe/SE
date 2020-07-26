using SE.Components;
using SE.Components.UI;
using SE.World.Partitioning;

namespace SE.Rendering
{
    public interface IRenderable : IPartitionObject<IRenderable>
    {
        void Render(Camera2D camera, Space space);
        int DrawCallID { get; }

        // TODO: Organize these better. Maybe have a lookup dictionary or something.
        public IUISprite IUISprite { get; }
        public ILit ILit { get; }

        BlendMode BlendMode { get; }
    }

    public enum Space
    {
        World,
        Screen
    }
}
