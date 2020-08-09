using System;
using System.Diagnostics;
using System.Net.Mime;
using System.Reflection;
using System.Threading;
using DeeZ.Editor.GUI;
using DeeZ.Editor.ImGUI;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement;
using SE.Components;
using SE.Core;
using Console = SE.Core.Console;
using Vector2 = System.Numerics.Vector2;

namespace SE
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

            Core.Rendering.ChangeDrawCall(SpriteSortMode.Immediate, null, BlendState.AlphaBlend);

            ImGuiRenderer.BeforeLayout(gameTime);
            EditorGUI.Paint();
            ImGuiRenderer.AfterLayout();

            // TEST: Create game instance.
            // TODO: Implement properly. Needs to dynamically open any type of SE Game.
            if (InputManager.KeyCodePressed(Microsoft.Xna.Framework.Input.Keys.F2)) {
                SceneManager.CurrentScene.Unload();

                Game gameInstance = (Game) Activator.CreateInstance(typeof(SEDemos.Game), null, this, this);
                gameInstance.KeepWindowOnDipose = true;

                // Hook into the game instance. OnUpdate will be called from this instance,
                // until the game exited.
                gameInstance.Run();
                ChangeInstance(null);
                
                //Exit();
                // TODO: Exit and reopen program.
            }

            // Test reload.
            // This is how hot reloading of code will work. Even though it isn't hot reloading.
            // Detect change -> Save editor state -> Close -> Reopen -> Restore editor state.
            if (InputManager.KeyCodePressed(Microsoft.Xna.Framework.Input.Keys.F9)) {
                Process.Start(AppDomain.CurrentDomain.BaseDirectory + "/SEEditor.exe");
                Thread.Sleep(2000);
                Environment.Exit(-1);
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
