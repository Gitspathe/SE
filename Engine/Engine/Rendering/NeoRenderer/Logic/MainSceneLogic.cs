using System;
using System.Collections.Generic;
using DeferredEngine.Entities;
using DeferredEngine.Recources;
using DeferredEngine.Recources.Helper;
using DeferredEngine.Renderer.Helper;
using DeferredEngine.Renderer.Helper.HelperGeometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SE.Common;
using SE.Components;
using SE.Rendering;
using DirectionalLight = DeferredEngine.Entities.DirectionalLight;
using GameSettings = DeferredEngine.Recources.GameSettings;
using MaterialEffect = DeferredEngine.Recources.MaterialEffect;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace DeferredEngine.Logic
{
    public class MainSceneLogic
    {
        #region FIELDS

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        private Assets _assets;
        
        public Camera2D Camera;


        //mesh library, holds all the meshes and their materials
        public MeshMaterialLibrary MeshMaterialLibrary;

        public readonly List<ModelRenderer> BasicEntities = new List<ModelRenderer>();
        public readonly List<Decal> Decals = new List<Decal>();
        public readonly List<PointLight> PointLights = new List<PointLight>();
        public readonly List<DirectionalLight> DirectionalLights = new List<DirectionalLight>();
        public readonly List<DebugEntity> DebugEntities = new List<DebugEntity>();
        public EnvironmentSample EnvironmentSample;

        //Which render target are we currently displaying?
        private int _renderModeCycle;
        private Space _physicsSpace;

        //SDF

        private ModelRenderer testEntity;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //  MAIN FUNCTIONS
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //Done after Load
        public void Initialize(Assets assets, GraphicsDevice graphicsDevice)
        {
            _assets = assets;

            MeshMaterialLibrary = new MeshMaterialLibrary(graphicsDevice);

            SetUpEditorScene(graphicsDevice);
        }

        //Load our default setup!
        private void SetUpEditorScene(GraphicsDevice graphics)
        {
            GameObject editorCam = new GameObject();
            Camera = (Camera2D) editorCam.AddComponent(new Camera2D());

            EnvironmentSample = new EnvironmentSample(new Vector3(-45, -5, 5));

            AddPointLight(position: new Vector3(-61, 0, 107),
                        radius: 150,
                        color: new Color(104, 163, 223),
                        intensity: 20,
                        castShadows: false,
                        shadowResolution: 1024,
                        staticShadow: false,
                        isVolumetric: true);

            AddPointLight(position: new Vector3(15, 0, 107),
                        radius: 150,
                        color: new Color(104, 163, 223),
                        intensity: 30,
                        castShadows: false,
                        shadowResolution: 1024,
                        staticShadow: false,
                        isVolumetric: true);

            AddPointLight(position: new Vector3(66, 0, 40),
                radius: 120,
                color: new Color(255, 248, 232),
                intensity: 120,
                castShadows: true,
                shadowResolution: 1024,
                softShadowBlurAmount: 0,
                staticShadow: false,
                isVolumetric: false);
        }



        /// <summary>
        /// Main logic update function. Is called once per frame. Use this for all program logic and user inputs
        /// </summary>
        /// <param name="gameTime">Can use this to compute the delta between frames</param>
        /// <param name="isActive">The window status. If this is not the active window we shouldn't do anything</param>
        public void Update(GameTime gameTime, bool isActive)
        {
            if (!isActive) return;

            //Upd
            Input.Update(gameTime, Camera);

            //VolumeTexture.RotationMatrix = testEntity.WorldTransform.InverseWorld;
            //VolumeTexture.Scale = testEntity.WorldTransform.Scale;
            
            //Make the lights move up and down
            //for (var i = 2; i < PointLights.Count; i++)
            //{
            //    PointLight point = PointLights[i];
            //    point.Position = new Vector3(point.Position.X, point.Position.Y, (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 0.8f + i) * 10 - 13));
            //}

            //KeyInputs for specific tasks


            //If we are currently typing stuff into the console we should ignore the following keyboard inputs
            if (DebugScreen.ConsoleOpen) return;

            //Starts the "editor mode" where we can manipulate objects
            if (Input.WasKeyPressed(Keys.Space))
            {
                GameSettings.e_enableeditor = !GameSettings.e_enableeditor;
            }

            
            
            //Spawns a new light on the ground
            if (Input.keyboardState.IsKeyDown(Keys.L))
            {
                AddPointLight(position: new Vector3(FastRand.NextSingle() * 250 - 125, FastRand.NextSingle() * 50 - 25, FastRand.NextSingle() * 30 - 19), 
                    radius: 20, 
                    color: FastRand.NextColor(), 
                    intensity: 40, 
                    castShadows: false,
                    isVolumetric: true);
            }
            
            //Switch which rendertargets we show
            if (Input.WasKeyPressed(Keys.F1))
            {
                _renderModeCycle++;
                if (_renderModeCycle > Enum.GetNames(typeof(Renderer.Renderer.RenderModes)).Length - 1) _renderModeCycle = 0;

                GameSettings.g_rendermode = (Renderer.Renderer.RenderModes) _renderModeCycle;
            }
        }
        

        //Load content
        public void Load(ContentManager content)
        {
            //...
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  HELPER FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Spawn a directional light (omni light). This light covers everything and comes from a single point from infinte distance. 
        /// Good for something like a sun
        /// </summary>
        /// <param name="direction">The direction the light is facing in world space coordinates</param>
        /// <param name="intensity"></param>
        /// <param name="color"></param>
        /// <param name="position">The position is only relevant if drawing shadows</param>
        /// <param name="drawShadows"></param>
        /// <param name="shadowWorldSize">WorldSize is the width/height of the view projection for shadow mapping</param>
        /// <param name="shadowDepth">FarClip for shadow mapping</param>
        /// <param name="shadowResolution"></param>
        /// <param name="shadowFilteringFiltering"></param>
        /// <param name="screenspaceShadowBlur"></param>
        /// <param name="staticshadows">These shadows will not be updated once they are created, moving objects will be shadowed incorrectly</param>
        /// <returns></returns>
        private DirectionalLight AddDirectionalLight(Vector3 direction, int intensity, Color color, Vector3 position = default(Vector3), bool drawShadows = false, float shadowWorldSize = 100, float shadowDepth = 100, int shadowResolution = 512, DirectionalLight.ShadowFilteringTypes shadowFilteringFiltering = DirectionalLight.ShadowFilteringTypes.Poisson, bool screenspaceShadowBlur = false, bool staticshadows = false )
        {
            DirectionalLight light = new DirectionalLight(color: color, 
                intensity: intensity, 
                direction: direction, 
                position: position, 
                castShadows: drawShadows, 
                shadowSize: shadowWorldSize, 
                shadowDepth: shadowDepth, 
                shadowResolution: shadowResolution, 
                shadowFiltering: shadowFilteringFiltering, 
                screenspaceshadowblur: screenspaceShadowBlur, 
                staticshadows: staticshadows);
            DirectionalLights.Add(light);
            return light;
        }

        //The function to use for new pointlights
        /// <summary>
        /// Add a point light to the list of drawn point lights
        /// </summary>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        /// <param name="intensity"></param>
        /// <param name="castShadows">will render shadow maps</param>
        /// <param name="isVolumetric">does it have a fog volume?</param>
        /// <param name="volumetricDensity">How dense is the volume?</param>
        /// <param name="shadowResolution">shadow map resolution per face. Optional</param>
        /// <param name="staticShadow">if set to true the shadows will not update at all. Dynamic shadows in contrast update only when needed.</param>
        /// <returns></returns>
        private PointLight AddPointLight(Vector3 position, float radius, Color color, float intensity, bool castShadows, bool isVolumetric = false, float volumetricDensity = 1, int shadowResolution = 256, int softShadowBlurAmount = 0, bool staticShadow = false)
        {
            PointLight light = new PointLight(position, radius, color, intensity, castShadows, isVolumetric, shadowResolution, softShadowBlurAmount, staticShadow, volumetricDensity);
            PointLights.Add(light);
            return light;
        }

    }
}
