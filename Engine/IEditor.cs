using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SE
{
    /// <summary>
    /// Interface used to inject the editor pipeline into a DeeZEngine game.
    /// </summary>
    public interface IEditor
    {
        public GraphicsDeviceManager EditorGraphicsDeviceManager { get; set; }
        public GraphicsDevice EditorGraphicsDevice { get; set; }

        public void OnUpdate(GraphicsDevice graphics, GameTime gameTime);
        public void OnInitialize(Game game);
        public void ChangeInstance(Game game);
    }
}
