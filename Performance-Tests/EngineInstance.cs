using System;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using DeeZ.Core;
using DeeZ.Editor.Debug.GameObjects;
using DeeZ.Engine.AssetManagement;
using DeeZ.Engine.Common;
using DeeZ.Engine.Components;
using DeeZ.Engine.Rendering;
using DeeZEngine_Demos.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = System.Numerics.Vector2;

namespace DeeZ.PerformanceTests
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class EngineInstance : GameEngine
    {
        private AssetConsumerContext assetContext = new AssetConsumerContext();
        internal static ContentLoader content;

        protected override void OnInitialize()
        {
            bool headless = DisplayMode == Mode.Decapitated || DisplayMode == Mode.Headless;

            // Initialize networking...
            Network.AddSpawnable("Player", typeof(NetPlayer));
            Network.AddSpawnable("bouncy", typeof(BouncyBall));

            if (!IsFullHeadless) {
                Lighting.Ambient = new Color(0.1f, 0.1f, 0.1f);
            }
            // Do performance tests.
            BenchmarkRunner.Run(typeof(EngineInstance).Assembly);

            Environment.Exit(0);
        }

        protected override void LoadAssets()
        {
            base.LoadAssets();
            content = new ContentLoader(Content.ServiceProvider, "Data/_MAIN/Content");

            AssetManager.Add("floor", new AssetBuilder<SpriteTexture>()
               .Create(() => new SpriteTexture(AssetManager.GetAsset<Texture2D>("tileset"), new Rectangle(0, 0, 64, 64)))
               .FromContent(content)
               .References(AssetManager.GetIAsset<Texture2D>("tileset"))
            );
            AssetManager.Add("wall_down", new AssetBuilder<SpriteTexture>()
               .Create(() => new SpriteTexture(AssetManager.GetAsset<Texture2D>("tileset"), new Rectangle(64, 48, 64, 16)))
               .FromContent(content)
               .References(AssetManager.GetIAsset<Texture2D>("tileset"))
            );
            AssetManager.Add("wall_left", new AssetBuilder<SpriteTexture>()
               .Create(() => new SpriteTexture(AssetManager.GetAsset<Texture2D>("tileset"), new Rectangle(128, 0, 16, 64)))
               .FromContent(content)
               .References(AssetManager.GetIAsset<Texture2D>("tileset"))
            );
            AssetManager.Add("wall_right", new AssetBuilder<SpriteTexture>()
               .Create(() => new SpriteTexture(AssetManager.GetAsset<Texture2D>("tileset"), new Rectangle(240, 0, 16, 64)))
               .FromContent(content)
               .References(AssetManager.GetIAsset<Texture2D>("tileset"))
            );
            AssetManager.Add("wall_up", new AssetBuilder<SpriteTexture>()
               .Create(() => new SpriteTexture(AssetManager.GetAsset<Texture2D>("tileset"), new Rectangle(256, 0, 64, 16)))
               .FromContent(content)
               .References(AssetManager.GetIAsset<Texture2D>("tileset"))
            );
            AssetManager.Add("player", new AssetBuilder<SpriteTexture>()
               .Create(() => new SpriteTexture(AssetManager.GetAsset<Texture2D>("tileset"), new Rectangle(0, 0, 52, 52)))
               .FromContent(content)
               .References(AssetManager.GetIAsset<Texture2D>("tileset"))
            );
            AssetManager.Add("circle", new AssetBuilder<SpriteTexture>()
               .Create(() => new SpriteTexture(AssetManager.GetAsset<Texture2D>("tileset"), new Rectangle(0, 128, 32, 32)))
               .FromContent(content)
               .References(AssetManager.GetIAsset<Texture2D>("tileset"))
            );

            if (!IsFullHeadless) {
                AssetManager.Add("assaultrifle", new AssetBuilder<SoundEffect>()
                   .Create(() => content.Load<SoundEffect>("assaultrifle"))
                   .FromContent(content)
                );
                AssetManager.Add("shader", new AssetBuilder<Effect>()
                   .Create(() => content.Load<Effect>("testshader"))
                   .FromContent(content)
                );
            }

            AssetManager.Add(0, new AssetBuilder<Func<Vector2, GameObject>>()
               .Create(() => pos => { 
                    Floor obj = new Floor(pos, 0f, Vector2.One); 
                    obj.GetComponent<Sprite>().Color = Color.White;
                    return obj;
                })
               .FromContent(content)
            );
            AssetManager.Add(1, new AssetBuilder<Func<Vector2, GameObject>>()
               .Create(() => pos => {
                    WallUp obj = new WallUp(pos, 0f, Vector2.One);
                    obj.GetComponent<Sprite>().Color = Color.Red; 
                    return obj;
                })
               .FromContent(content)
            );
            AssetManager.Add(2, new AssetBuilder<Func<Vector2, GameObject>>()
               .Create(() => pos => {
                    WallRight obj = new WallRight(pos, 0f, Vector2.One);
                    obj.GetComponent<Sprite>().Color = Color.Blue; 
                    return obj;
                })
               .FromContent(content)
            );
            AssetManager.Add(3, new AssetBuilder<Func<Vector2, GameObject>>()
               .Create(() => pos => {
                    WallDown obj = new WallDown(pos, 0f, Vector2.One);
                    obj.GetComponent<Sprite>().Color = Color.Orange; 
                    return obj;
                })
               .FromContent(content)
            );
            AssetManager.Add(4, new AssetBuilder<Func<Vector2, GameObject>>()
               .Create(() => pos => {
                    WallLeft obj = new WallLeft(pos, 0f, Vector2.One);
                    obj.GetComponent<Sprite>().Color = Color.Purple; 
                    return obj;
                })
               .FromContent(content)
            );
            AssetManager.Add(5, new AssetBuilder<Func<Vector2, GameObject>>()
               .Create(() => pos => { 
                    AnimatedFloor obj = new AnimatedFloor(pos, 0f, Vector2.One);
                    obj.GetComponent<Sprite>().Color = Color.White; 
                    return obj;
                })
               .FromContent(content)
            );
        }

        protected override void OnUpdate(GameTime gameTime) { }

        [STAThread]
        public static void Main(string[] args)
        {
            using (EngineInstance engineInstance = new EngineInstance(args)) {
                engineInstance.Run();
            }
        }

        public EngineInstance(string[] args) : base(args) { }
    }
}
