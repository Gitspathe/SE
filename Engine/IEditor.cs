using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SE
{
    /// <summary>
    /// Interface used to inject the editor pipeline into an SE game.
    /// </summary>
    public interface IEditor
    {
        GraphicsDeviceManager EditorGraphicsDeviceManager { get; set; }
        GraphicsDevice EditorGraphicsDevice { get; set; }

        void OnUpdate(GraphicsDevice graphics, GameTime gameTime);
        void OnDraw(GraphicsDevice graphics, GameTime gameTime);
        void OnInitialize(Game game);
        void ChangeInstance(Game game);
    }
}
