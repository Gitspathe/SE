#if EDITOR
#endif
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SE.AssetManagement;
using SE.AssetManagement.Processors;
using SE.Common;
using SE.Components;
using SE.Core;
using SE.Core.Internal;
using SE.Rendering;
using SE.Core.Extensions;
using SE.Utility;
using static SE.Core.SceneManager;
using Console = SE.Core.Console;
using SE.Serialization;
using SE.GameLoop;
using SE.World.Partitioning;
using SE.Particles.Shapes;
using System.Numerics;
using Random = SE.Utility.Random;
using Vector2 = System.Numerics.Vector2;

[assembly: InternalsVisibleTo("SEEditor")]
namespace SE
{
    /// <summary>
    /// Entry point for SE. Override this class to use SE.
    /// </summary>
    public class GameEngine : Game
    {
        public static GameEngine Engine;
        public static Action Initalized;
        public static UpdateLoop UpdateLoop;
        public GraphicsDeviceManager GraphicsDeviceManager;
        
        internal static HashSet<GameObject> DynamicGameObjects = new HashSet<GameObject>();
        internal static HashSet<GameObject> AllGameObjects = new HashSet<GameObject>();
        internal GameObject Player;
        internal GameTime GameTime;

        private static bool gcFrame;

        private static object gameObjectHandlerLock = new object();

#if EDITOR
        public bool LevelEditMode = true;
#endif

        internal static ContentLoader EngineContent { get; private set; }

        public static IEditor Editor { get; set; }

        public static bool IsEditor => Editor != null;
        public static Camera2D EditorCamera { get; protected set; }

        private static bool isInitialized;

        /// <summary>
        /// Creates a new instance of SE.
        /// </summary>
        protected GameEngine(string[] args, IEditor editorEntry = null, Game existingGame = null) : base(existingGame)
        {
            Editor = editorEntry;
            if (IsEditor) {
                // Don't display 2nd window if editor! But pretend the Game() isn't headless.
                Screen.DisplayMode = DisplayMode.Normal;
                SetHeadless(true);
            } else {
                if (args.Contains("-full-headless")) {
                    Screen.DisplayMode = DisplayMode.Decapitated;
                    SetHeadless(true);
                } else if (args.Contains("-headless")) {
                    Screen.DisplayMode = DisplayMode.Headless;
                    SetHeadless(false);
                } else {
                    Screen.DisplayMode = DisplayMode.Normal;
                    SetHeadless(false);
                }
            }
            //Screen.DisplayMode = DisplayMode.Decapitated;
            //SetHeadless(true);

            Engine = this;

            if (!IsEditor) {
                GraphicsDeviceManager = new GraphicsDeviceManager(this) {
                    SynchronizeWithVerticalRetrace = false,
                    PreferredBackBufferFormat = SurfaceFormat.Color,
                    GraphicsProfile = GraphicsProfile.HiDef
                };
            } else {
                GraphicsDeviceManager = editorEntry.EditorGraphicsDeviceManager;
                Services.AddService(typeof(IGraphicsDeviceService), GraphicsDeviceManager);
            }

            // 3D test.
            //_screenManager = new ScreenManager();
        }

        /// <summary>
        /// Initializes the engine.
        /// </summary>
        protected sealed override void Initialize()
        {
            if (!isInitialized) {
                FileMarshal.Setup();
            }

            Content.RootDirectory = "Data";
            LoadEngineContent();
            LoadAssets();

            IsMouseVisible = true;
            GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = Screen.IsFullHeadless;
            TargetElapsedTime = TimeSpan.FromSeconds(1.0f / 60);
            InactiveSleepTime = TimeSpan.FromSeconds(1.0f / 60); // Alt-tabbed update rate.

            // If the engine ISN'T initialized (i.e, not in editor mode)...
            if (!isInitialized) {
                //ThreadPool.SetMaxThreads(Environment.ProcessorCount, Environment.ProcessorCount);
                //ThreadPool.SetMinThreads(Environment.ProcessorCount, Environment.ProcessorCount);

                Config.Initialize();
                InputManager.Initialize(this);
                Reflection.Initialize();
                ModLoader.Initialize();
                UpdateLoop = new UpdateLoop();

                ParticleEngine.UpdateMode = UpdateMode.ParallelAsynchronous;
                ParticleEngine.UseParticleRenderer = Config.Performance.UseParticleInstancing;
                ParticleEngine.AllocationMode = Config.Performance.UseArrayPoolParticles
                    ? ParticleAllocationMode.ArrayPool
                    : ParticleAllocationMode.Array;
                ParticleEngine.Initialize(this, GraphicsDeviceManager);

                UIManager.Initialize();
                if (!Screen.IsFullHeadless) 
                    Core.Lighting.Initialize();

                Core.Physics.Initialize();

                Network.OnLogInfo += (msg, important) => Console.LogInfo(msg, important, LogSource.Network);
                Network.OnLogWarning += (msg, exception) => {
                    if (!string.IsNullOrEmpty(msg))
                        Console.LogWarning(msg, LogSource.Network);
                    else
                        Console.LogWarning(exception, LogSource.Network);
                };
                Network.OnLogError += (msg, exception) => {
                    if (!string.IsNullOrEmpty(msg))
                        Console.LogError(msg, LogSource.Network);
                    else
                        Console.LogError(exception, LogSource.Network);
                };

                SetCurrentScene("_MAIN\\empty");
                if (!Screen.IsFullHeadless) {
                    Core.Rendering.Initialize(GraphicsDeviceManager, GraphicsDevice);
                    Console.InitializeStats(Core.Rendering.SpriteBatch);
                }

                // After core initialization. Console will work here.
                Console.Initialize();
                Console.LogInfo("Core engine loaded.", true);
                Console.LogInfo("Game loop loaded:", true);
                Console.LogInfo(UpdateLoop.ToString());

                Network.Initialize();
                Screen.Reset();
                Initalized?.Invoke();

                Console.LogInfo("Finished initialization.", true);
            }

            if (IsEditor) {
                Editor.ChangeInstance(this);
                Editor.OnInitialize(this);
            }

            OnInitialize();
            isInitialized = true;

            //_screenManager.Load(Content, GraphicsDevice);
            //_screenManager.Initialize(GraphicsDevice);
        }

        private void LoadEngineContent()
        {
            EngineContent = new ContentLoader(Content.ServiceProvider, "EngineContent", "Data/_MAIN/Content/");
            if (Screen.IsFullHeadless) 
                return;

            // Particle shader.
            AssetManager.Add(new AssetBuilder<Effect>()
               .ID("testshader")
               .Create(new GeneralContentProcessor<Effect>("EngineContent", "shader"))
               .FromContent(EngineContent)
            );

            // Texture2Ds
            AssetManager.Add(new AssetBuilder<Texture2D>()
               .ID("tileset")
               .Create(new GeneralContentProcessor<Texture2D>("EngineContent", "Images/tileset"))
               .FromContent(EngineContent)
            );
            AssetManager.Add(new AssetBuilder<Texture2D>()
               .ID("UI")
               .Create(new GeneralContentProcessor<Texture2D>("EngineContent", "UI"))
               .FromContent(EngineContent)
            );
            AssetManager.Add(new AssetBuilder<Texture2D>()
               .ID("EditorUI")
               .Create(new GeneralContentProcessor<Texture2D>("EngineContent", "EditorUI"))
               .FromContent(EngineContent)
            );

            // Sprite Textures
            AssetManager.Add(new AssetBuilder<SpriteTexture>()
               .ID("uiRect")
               .Create(new SpriteTextureProcessor("tileset", new Rectangle(0, 0, 64, 64)))
               .FromContent(EngineContent)
            );
            AssetManager.Add(new AssetBuilder<SpriteTexture>()
               .ID("leveleditorcursor")
               .Create(new SpriteTextureProcessor("tileset", new Rectangle(0, 64, 64, 64)))
               .FromContent(EngineContent)
            );
            AssetManager.Add(new AssetBuilder<SpriteTexture>()
               .ID("debugsmallsquare")
               .Create(new SpriteTextureProcessor("tileset", new Rectangle(0, 128, 16, 16)))
               .FromContent(EngineContent)
            );

            // Sprite Fonts
            AssetManager.Add(new AssetBuilder<SpriteFont>()
               .ID("editor")
               .Create(new GeneralContentProcessor<SpriteFont>("EngineContent", "fonts/editor"))
               .FromContent(EngineContent)
            );
            AssetManager.Add(new AssetBuilder<SpriteFont>()
               .ID("editor_subheading")
               .Create(new GeneralContentProcessor<SpriteFont>("EngineContent", "fonts/editor"))
               .FromContent(EngineContent)
            );
            AssetManager.Add(new AssetBuilder<SpriteFont>()
               .ID("editor_heading")
               .Create(new GeneralContentProcessor<SpriteFont>("EngineContent", "fonts/editor_heading"))
               .FromContent(EngineContent)
            );
        }

        /// <summary>
        /// Main update loop, the majority of both the engine's and the game's logic are processed here.
        /// </summary>
        /// <param name="gameTime">Helper class containing timing methods.</param>
        protected sealed override void Update(GameTime gameTime)
        {
            DevelopmentTests();
            if (!Config.Initialized)
                throw new InvalidOperationException("Config was not initialized!");

            Config.Update();
            AssetManager.Update(Time.DeltaTime);

            GameTime = gameTime;
            UpdateLoop.Invoke();

            OnUpdate(gameTime);
            Editor?.OnUpdate(GraphicsDeviceManager.GraphicsDevice, gameTime);
            if (gcFrame) {
                GC.Collect();
                gcFrame = false;
            }

            //_screenManager.Update(gameTime, true);
        }

        protected sealed override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if(Screen.IsFullHeadless)
                return;

            Core.Rendering.Update();
            Editor?.OnDraw(GraphicsDeviceManager.GraphicsDevice, gameTime);
            EngineUtility.TransformHierarchyDirty = false;

            //_screenManager.Draw(gameTime);
        }

        internal void GraphicsPresent()
        {
            DoDraw(GameTime);
            //Draw(GameTime);
            //EndDraw();
        }

        private float time = 2.0f;
        private void DevelopmentTests()
        {
            // GC test. TODO: MAKE SURE THIS IS COMMENTED OUT WHEN NOT NEEDED!!!!!!!!!!!!!!!!!!
            //if (InputManager.KeyCodePressed(Keys.A)) {
            //    GC.Collect();
            //}

            time -= Time.DeltaTime;
            if (time <= 0.0f) {
                time = 2.0f;
                Console.WriteLine("NEW: " + ParticleEngine.ParticleCount + ", EMITTERS #: " + ParticleEngine.EmitterCount);
            }

            // Multithreaded render test.
            if (InputManager.KeyCodePressed(Keys.F)) {
                Renderer.Multithreaded = !Renderer.Multithreaded;
                Console.WriteLine("Multithreaded: " + (Renderer.Multithreaded ? "on" : "off"));
            }

            if (InputManager.KeyCodePressed(Keys.I)) {
                ParticleEngine.UseParticleRenderer = !ParticleEngine.UseParticleRenderer;
                Console.WriteLine("Instanced: " + (ParticleEngine.UseParticleRenderer ? "on" : "off"));
            }

            if (InputManager.KeyCodePressed(Keys.L)) {
                ParticleEngine.UpdateMode = ParticleEngine.UpdateMode == UpdateMode.ParallelAsynchronous 
                    ? UpdateMode.Synchronous 
                    : UpdateMode.ParallelAsynchronous;

                Console.WriteLine("Multithreaded particles: " + (ParticleEngine.UpdateMode == UpdateMode.ParallelAsynchronous ? "on" : "off"));
            }

            //Debug.WriteLine(SpatialPartitionManager.EntitiesCount);
        }

        public static void RequestGC()
        {
            if (gcFrame) {
                GC.Collect();
            }
            gcFrame = true;
        }

        /// <summary>
        /// Removes and re-adds a GameObject to the engine. Used when a GameObject is changed from dynamic
        /// to static, etc.
        /// </summary>
        /// <param name="go">GameObject to update.</param>
        internal static void UpdateGameObjectState(GameObject go)
        {
            RemoveGameObject(go);
            AddGameObject(go);
        }

        internal static void GameObjectConstructorCallback(GameObject go)
        {
            // If the GameObject isn't networked, initialize it immediately after creation.
            // Otherwise, wait for the network manager to set up the GameObject.
            lock (gameObjectHandlerLock) {
                if (go.NetIdentity == null) {
                    go.Initialize();
                }
            }
        }

        /// <summary>
        /// Registers a GameObject with the engine.
        /// </summary>
        /// <param name="go">GameObject which was created.</param>
        internal static void AddGameObject(GameObject go)
        {
            lock (gameObjectHandlerLock) {
                if (AllGameObjects.Contains(go))
                    RemoveGameObject(go);

                AllGameObjects.Add(go);
                CurrentScene.GameObjectsToRemove.Remove(go);
                
                if (go.DestroyOnLoad) {
                    CurrentScene.AttachedGameObjects.Add(go);
                }

                if (go.IsDynamic) {
                    DynamicGameObjects.Add(go);
                }
                EngineUtility.TransformHierarchyDirty = true;
            }
        }

        /// <summary>
        /// Unregisters a GameObject from the engine.
        /// </summary>
        /// <param name="go">GameObject which was destroyed.</param>
        /// <param name="destroyed">If the GameObject was destroyed.</param>
        internal static void RemoveGameObject(GameObject go, bool destroyed = false)
        {
            lock (gameObjectHandlerLock) {
                DynamicGameObjects.Remove(go);
                CurrentScene.GameObjectsToRemove.Add(go);
                if (destroyed) {
                    AllGameObjects.Remove(go);
                }
                EngineUtility.TransformHierarchyDirty = true;
            }
        }

        // SE methods.
        protected virtual void LoadAssets() { }

        /// <summary>
        /// Update loop.
        /// </summary>
        /// <param name="gameTime">Helper class containing timing methods.</param>
        protected virtual void OnUpdate(GameTime gameTime) { }

        /// <summary>
        /// Function called on initialization.
        /// </summary>
        protected virtual void OnInitialize() { }

        // Disable some MonoGame methods.
        protected sealed override void UnloadContent() => base.UnloadContent();
        protected sealed override void LoadContent() => throw new NotImplementedException();
    }
}
