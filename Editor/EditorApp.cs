using System;
using DeeZ.Core;
using DeeZ.Editor.GUI;
using DeeZ.Editor.ImGUI;
using DeeZ.Engine;
using DeeZ.Engine.AssetManagement;
using DeeZ.Engine.Components;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Console = DeeZ.Core.Console;
using Vector2 = System.Numerics.Vector2;

namespace DeeZ
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class EditorApp : GameEngine, IEditor
    {
        public static ImGuiRenderer ImGuiRenderer { get; private set; }

        internal static ContentLoader content;

        private static Game game;

        public GraphicsDeviceManager EditorGraphicsDeviceManager { get; set; }
        public GraphicsDevice EditorGraphicsDevice { get; set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            InternalCamera cam = new InternalCamera(Vector2.Zero, 0f, Vector2.One);
            EditorCamera = cam.GetComponent<Camera2D>();
        }

        public void OnInitialize(Game game)
        {
            EditorGraphicsDeviceManager = GraphicsDeviceManager;
            EditorGraphicsDevice = GraphicsDeviceManager.GraphicsDevice;
            //ImGuiRenderer = new ImGuiRenderer(game);
            //ImGuiRenderer.RebuildFontAtlas();
            //EditorGUI.Initialize();
        }

        public void OnUpdate(GraphicsDevice gfxDevice, GameTime gameTime)
        {
            gfxDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 0.0f, 1);

            Rendering.ChangeDrawCall(SpriteSortMode.Immediate, null, BlendState.AlphaBlend);

            ImGuiRenderer.BeforeLayout(gameTime);
            EditorGUI.Paint();
            ImGuiRenderer.AfterLayout();

            // TEST: Create game instance.
            // TODO: Implement properly. Needs to dynamically open any type of DeeZEngine Game.
            if (InputManager.KeyCodePressed(Microsoft.Xna.Framework.Input.Keys.F2)) {
                SceneManager.CurrentScene.Unload();

                Game gameInstance = (Game) Activator.CreateInstance(typeof(DeeZEngine_Demos.Game), null, this, this);
                gameInstance.KeepWindowOnDipose = true;

                // Hook into the game instance. OnUpdate will be called from this instance,
                // until the game exited.
                gameInstance.Run();
                ChangeInstance(null);
                
                //Exit();
                // TODO: Exit and reopen program.
            }
        }

        public void ChangeInstance(Game game)
        {
            if(game == EditorApp.game)
                return;

            if (game == null) {
                game = this;
            } else {
                game.Focus();
                EditorApp.game = game;
            }

            ImGuiRenderer = new ImGuiRenderer(game);
            ImGuiRenderer.RebuildFontAtlas();
            EditorGUI.Initialize();
        }

        public EditorApp() : base(null)
        {
            Editor = this;
        }
    }
}
