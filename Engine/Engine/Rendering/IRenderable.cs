using SE.Components;

namespace SE.Rendering
{
    public interface IRenderable
    {
        void Render(Camera2D camera, Space space);
    }

    public enum Space
    {
        World,
        Screen
    }
}
