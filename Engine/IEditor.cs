using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SE
{
    /// <summary>
    /// Interface used to inject the editor pipeline into an SE game.
    /// </summary>
    public interface IEditor
    {
        public GraphicsDeviceManager EditorGraphicsDeviceManager { get; set; }
        public GraphicsDevice EditorGraphicsDevice { get; set; }

        public void OnUpdate(GraphicsDevice graphics, GameTime gameTime);
        public void OnDraw(GraphicsDevice graphics, GameTime gameTime);
        public void OnInitialize(Game game);
        public void ChangeInstance(Game game);
    }
}
