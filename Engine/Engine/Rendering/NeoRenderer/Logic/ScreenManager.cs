using System;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Logic
{
    /// <summary>
    /// Manages our different screens and passes information accordingly
    /// </summary>
    public class ScreenManager : IDisposable
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private Renderer.Renderer _renderer;
        private MainSceneLogic _sceneLogic;
        private Assets _assets;
        private ShaderManager _shaderManager;
        private DebugScreen _debug;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _renderer.Initialize(graphicsDevice, _assets);
            _sceneLogic.Initialize(_assets, graphicsDevice);
            _debug.Initialize(graphicsDevice);
        }

        //Update per frame
        public void Update(GameTime gameTime, bool isActive)
        {
#if DEBUG
            _shaderManager.CheckForChanges();
#endif

            _sceneLogic.Update(gameTime, isActive);
            _renderer.Update(gameTime, isActive, _sceneLogic.BasicEntities);
            
            _debug.Update(gameTime);
        }

        //Load content
        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _renderer = new Renderer.Renderer();
            _sceneLogic = new MainSceneLogic();
            _assets = new Assets();
            _debug = new DebugScreen();

            Globals.content = content;
            Shaders.Load(content);
            _shaderManager = new ShaderManager(content, graphicsDevice);
            _assets.Load(content, graphicsDevice);
            _renderer.Load(content, _shaderManager);
            _sceneLogic.Load(content);
            _debug.LoadContent(content);
        }

        public void Unload(ContentManager content)
        {
            content.Dispose();
        }
        
        public void Draw(GameTime gameTime)
        {
            //Our renderer gives us information on what id is currently hovered over so we can update / manipulate objects in the logic functions
            _renderer.Draw(_sceneLogic.Camera, 
                _sceneLogic.MeshMaterialLibrary, 
                _sceneLogic.BasicEntities,
                pointLights: _sceneLogic.PointLights,
                directionalLights: _sceneLogic.DirectionalLights, 
                envSample: _sceneLogic.EnvironmentSample,
                gameTime: gameTime);

            _debug.Draw(gameTime);
        }

        public void UpdateResolution()
        {
            //if(_renderer == null)
            //    return;

            _renderer.UpdateResolution();
        }

        public void Dispose()
        {
        }
    }
}
