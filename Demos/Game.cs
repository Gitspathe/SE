using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SE;
using SE.AssetManagement;
using SE.AssetManagement.Processors;
using SE.Common;
using SE.Components;
using SE.Core;
using SE.Editor.Debug.GameObjects;
using SE.Engine.Networking;
using SE.Networking.Internal;
using SE.Rendering;
using SE.Core.Extensions;
using SE.Engine.Input;
using SE.Utility;
using SEDemos.GameObjects;
using SEDemos.GameObjects.UI;
using Vector2 = System.Numerics.Vector2;
using DisplayMode = SE.DisplayMode;
using SEParticles;

namespace SEDemos
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game : GameEngine
    {
        private AssetConsumerContext assetContext = new AssetConsumerContext();
        internal static ContentLoader content;

        private BackButton backToMenu;

        protected override void OnInitialize()
        {
            bool headless = Screen.DisplayMode == DisplayMode.Decapitated || Screen.DisplayMode == DisplayMode.Headless;

            // Initialize input for player 1...
            InputManager.AddAxisInputs("PlayerVertical", new [] { Players.One },
                AxisInput.FromKeyboard(Keys.S, Keys.W),
                AxisInput.FromThumbStick(ThumbSticks.Left, ThumbSticksAxis.Y, 0.1f, true));

            InputManager.AddAxisInputs("PlayerHorizontal", new[] { Players.One },
                AxisInput.FromKeyboard(Keys.D, Keys.A), 
                AxisInput.FromThumbStick(ThumbSticks.Left, ThumbSticksAxis.X, 0.1f));

            // Initialize input for player 2...
            InputManager.AddAxisInputs("PlayerVertical", new[] { Players.Two },
                AxisInput.FromKeyboard(Keys.G, Keys.T),
                AxisInput.FromThumbStick(ThumbSticks.Left, ThumbSticksAxis.Y, 0.1f, true));

            InputManager.AddAxisInputs("PlayerHorizontal", new[] { Players.Two },
                AxisInput.FromKeyboard(Keys.H, Keys.F),
                AxisInput.FromThumbStick(ThumbSticks.Left, ThumbSticksAxis.X, 0.1f));

            // Initialize networking...
            NetHelper.AddSpawnable("Player", typeof(NetPlayer));
            NetHelper.AddSpawnable("bouncy", typeof(BouncyBall));
            Network.OnServerStarted += SpawnStuff;
            Network.OnPeerConnected += peer => {
                if (Network.IsServer) {
                    NetHelper.Instantiate("Player", peer.GetUniqueID(), new Vector2(100, 100));
                    NetHelper.Instantiate("Player", peer.GetUniqueID(), new Vector2(100, 100));
                }
            };

            //SceneManager.SetCurrentScene("_DEMOS\\menu");
            SceneManager.SetCurrentScene("_DEMOS\\networktest");

            backToMenu = new BackButton();

            // Load the network test level, and start server if in fully-headless mode.
            if (headless) {
                SceneManager.SetCurrentScene("_DEMOS\\networktest");
                Network.StartServer(919, 920);
            }

            if (!Screen.IsFullHeadless) {
                Lighting.Ambient = new Color(0.1f, 0.1f, 0.1f);
            }

            new Cursor(Vector2.Zero, 0f, Vector2.One);

            if (!IsEditor) {
                GameObject camera = new InternalCamera(Vector2.Zero, 0f, Vector2.One);
            }
        }

        public static void SpawnStuff()
        {
            if(!Network.IsServer)
                return;

            for (int x = 0; x < 10; x++) {
                for (int y = 0; y < 10; y++) {
                    NetHelper.Instantiate("bouncy", "SERVER", new Vector2(128 + x * 64, 128 + y * 64));
                    //go.Destroy();
                }
            }
        }

        protected override void LoadAssets()
        {
            base.LoadAssets();
            content = new ContentLoader(Content.ServiceProvider, "Content1", "Data/_MAIN/Content");
            ContentLoader content2 = new ContentLoader(Content.ServiceProvider, "Content2", "Data/_MAIN/Content");

            if (!Screen.IsFullHeadless) {
                AssetManager.Add(new AssetBuilder<Texture2D>()
                   .ID("tileset2")
                   .Create(new GeneralContentProcessor<Texture2D>("Content1", "tileset2"))
                   .FromContent(content2)
                );
            }

            AssetManager.Add(new AssetBuilder<SpriteTexture>()
               .ID("unload_test")
               .Create(new SpriteTextureProcessor("tileset2", new Rectangle(0, 0, 64, 64)))
               .FromContent(content2)
               .References(AssetManager.GetIAsset<Texture2D>("tileset2"))
            );
            AssetManager.Add(new AssetBuilder<SpriteTexture>()
               .ID("floor")
               .Create(new SpriteTextureProcessor("tileset", new Rectangle(0, 0, 64, 64)))
               .FromContent(content)
               .References(AssetManager.GetIAsset<Texture2D>("tileset"))
            );
            AssetManager.Add(new AssetBuilder<SpriteTexture>()
               .ID("wall_down")
               .Create(new SpriteTextureProcessor("tileset", new Rectangle(64, 48, 64, 16)))
               .FromContent(content)
               .References(AssetManager.GetIAsset<Texture2D>("tileset"))
            );
            AssetManager.Add(new AssetBuilder<SpriteTexture>()
               .ID("wall_left")
               .Create(new SpriteTextureProcessor("tileset", new Rectangle(128, 0, 16, 64)))
               .FromContent(content)
               .References(AssetManager.GetIAsset<Texture2D>("tileset"))
            );
            AssetManager.Add(new AssetBuilder<SpriteTexture>()
               .ID("wall_right")
               .Create(new SpriteTextureProcessor("tileset", new Rectangle(240, 0, 16, 64)))
               .FromContent(content)
               .References(AssetManager.GetIAsset<Texture2D>("tileset"))
            );
            AssetManager.Add(new AssetBuilder<SpriteTexture>()
               .ID("wall_up")
               .Create(new SpriteTextureProcessor("tileset", new Rectangle(256, 0, 64, 16)))
               .FromContent(content)
               .References(AssetManager.GetIAsset<Texture2D>("tileset"))
            );
            AssetManager.Add(new AssetBuilder<SpriteTexture>()
               .ID("player")
               .Create(new SpriteTextureProcessor("tileset", new Rectangle(0, 0, 52, 52)))
               .FromContent(content)
               .References(AssetManager.GetIAsset<Texture2D>("tileset"))
            );
            AssetManager.Add(new AssetBuilder<SpriteTexture>()
               .ID("circle")
               .Create(new SpriteTextureProcessor("tileset", new Rectangle(0, 128, 32, 32)))
               .FromContent(content)
               .References(AssetManager.GetIAsset<Texture2D>("tileset"))
            );

            if (!Screen.IsFullHeadless) {
                AssetManager.Add(new AssetBuilder<SoundEffect>()
                   .ID("assaultrifle")
                   .Create(new GeneralContentProcessor<SoundEffect>("Content1", "assaultrifle"))
                   .FromContent(content)
                );
            }

            AssetManager.Add(new AssetBuilder<Func<Vector2, GameObject>>()
               .ID("0")
               .Create(new TileProcessor(pos => {
                    Floor obj = new Floor(pos, 0f, Vector2.One); 
                    obj.GetComponent<Sprite>().Color = Color.White;
                    return obj;
                }))
               .FromContent(content)
            );
            AssetManager.Add(new AssetBuilder<Func<Vector2, GameObject>>()
               .ID("1")
               .Create(new TileProcessor(pos => {
                    WallUp obj = new WallUp(pos, 0f, Vector2.One);
                    obj.GetComponent<Sprite>().Color = Color.Red; 
                    return obj;
                }))
               .FromContent(content)
            );
            AssetManager.Add(new AssetBuilder<Func<Vector2, GameObject>>()
               .ID("2")
               .Create(new TileProcessor(pos => {
                    WallRight obj = new WallRight(pos, 0f, Vector2.One);
                    obj.GetComponent<Sprite>().Color = Color.Blue; 
                    return obj;
                }))
               .FromContent(content)
            );
            AssetManager.Add(new AssetBuilder<Func<Vector2, GameObject>>()
               .ID("3")
               .Create(new TileProcessor(pos => {
                    WallDown obj = new WallDown(pos, 0f, Vector2.One);
                    obj.GetComponent<Sprite>().Color = Color.Orange; 
                    return obj;
                }))
               .FromContent(content)
            );
            AssetManager.Add(new AssetBuilder<Func<Vector2, GameObject>>()
               .ID("4")
               .Create(new TileProcessor(pos => {
                    WallLeft obj = new WallLeft(pos, 0f, Vector2.One);
                    obj.GetComponent<Sprite>().Color = Color.Purple; 
                    return obj;
                }))
               .FromContent(content)
            );
            AssetManager.Add(new AssetBuilder<Func<Vector2, GameObject>>()
               .ID("5")
               .Create(new TileProcessor(pos => {
                    AnimatedFloor obj = new AnimatedFloor(pos, 0f, Vector2.One);
                    obj.GetComponent<Sprite>().Color = Color.White; 
                    return obj;
                }))
               .FromContent(content)
            );

            //for (int i = 0; i < 50; i++) {
            //    Database.Add<int, Func<Vector2, GameObject>>(6+i, pos => {
            //        AnimatedFloor obj = new AnimatedFloor(pos, 0f, Vector2.One);
            //        //obj.GetComponent<Sprite>().Color = Color.White;
            //        return obj;
            //    });
            //}
        }

        private float timer = 1.0f;
        protected override void OnUpdate(GameTime gameTime)
        {
            timer -= Time.DeltaTime;
            if (timer <= 0.0f && Network.IsServer) {
                for (int i = 0; i < 20; i++) {
                    NetHelper.Instantiate("bouncy", "SERVER",
                        new Vector2(128 + SE.Utility.Random.Next(0.0f, 1024.0f), 128 + SE.Utility.Random.Next(0.0f, 1024.0f)));
                }
                timer = 0.05f;
            }

            if (IsEditor) {
                if (InputManager.KeyCodePressed(Keys.F3)) {
                    SceneManager.SetCurrentScene("_DEMOS\\networktest");
                    Network.StartServer(919, 920);
                }
            }

            if (SceneManager.CurrentScene.LevelName != "menu" && !backToMenu.Enabled) {
                backToMenu.Enable();
            } else if (SceneManager.CurrentScene.LevelName == "menu") {
                backToMenu.Disable(true);
            }
        }

        public Game(string[] args, IEditor editor = null, Microsoft.Xna.Framework.Game existingGame = null) : base(args, editor, existingGame) { }
    }
}
