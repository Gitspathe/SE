using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Common;
using SE.Components;
using SE.Core;
using SE.Particles;
using SE.UI;
using SE.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using static SE.Core.Rendering;
using Color = Microsoft.Xna.Framework.Color;
using Console = SE.Core.Console;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace SE.Rendering
{
    // TODO: World matrix instead of stupidly removing camera position from drawn position.
    public class Renderer : IRenderer
    {
        public RenderContainer RenderContainer = new RenderContainer();

        private RasterizerState rasterizerScissor = new RasterizerState { ScissorTestEnable = true };
        private int viewPortX;
        private int viewPortY;
        private Rectangle scissorRectBackup = new Rectangle(0, 0, 1920, 1080);
        private QuickList<Emitter> tmpEmitters = new QuickList<Emitter>();

        private GraphicsDevice graphicsDevice = Core.Rendering.GraphicsDevice;
        
        public static bool Multithreaded { get; set; } = false;
        public static int CullingThreshold { get; set; } = 128;

        public static BlendState Alpha = new BlendState {
            ColorSourceBlend = Blend.SourceAlpha,
            AlphaSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            AlphaDestinationBlend = Blend.InverseSourceAlpha,
        };

        public static BlendState AlphaSubtract = new BlendState {
            ColorBlendFunction = BlendFunction.ReverseSubtract,
            ColorSourceBlend = Blend.SourceAlpha,
            AlphaSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            AlphaDestinationBlend = Blend.InverseSourceAlpha,
            IndependentBlendEnable = true
        };

        public void NewFrame(Camera2D camera)
        {
            Prepare();
            graphicsDevice.SetRenderTarget(SceneRender);
            graphicsDevice.Clear(Color.Black);

            graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 0f, 0);
            ChangeDrawCall(SpriteSortMode.Deferred, camera.ViewMatrix, BlendState.Opaque, SamplerState.PointClamp, DepthStencilGreater);

            // Caching.
            viewPortX = (int)camera.Position.X;
            viewPortY = (int)camera.Position.Y;
        }

        public void EndLighting()
        {
            EndDrawCall();
            if (Core.Lighting.Enabled) {
                Core.Lighting.Penumbra?.Draw(Time.GameTime);
            }
        }

        public void StartLighting()
        {
            if (Core.Lighting.Enabled) {
                Core.Lighting.Penumbra?.BeginDraw();
            }
        }

        public void DrawUI(Camera2D camera)
        {
            graphicsDevice.SetRenderTarget(UIRender);
            graphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0.0f, 0);
            RenderUI();
            RenderUIGizmos(camera);

            ChangeDrawCall(SpriteSortMode.FrontToBack, Screen.ScreenScaleMatrix, BlendState.AlphaBlend, SamplerState.PointClamp, null, rasterizerScissor);
            Console.DrawOverlay(camera, Core.Rendering.SpriteBatch);
            EndDrawCall();
        }

        public void GenerateRenderLists(bool excludeUI = true)
        {
            if (Multithreaded) {
                GenerateRenderListsMultiThreaded(excludeUI);
            } else {
                GenerateRenderListsSingleThread(excludeUI);
            }
            RenderContainer.RegisterToRenderLoop();
        }

        public void GenerateRenderListsSingleThread(bool excludeUI)
        {
            QuickList<IPartitionedRenderable> renderedSprites = VisibleSprites;
            RenderContainer.Reset();

            IPartitionedRenderable[] spriteArray = renderedSprites.Array;
            for (int i = 0; i < renderedSprites.Count; i++) {
                IPartitionedRenderable renderObj = spriteArray[i];
                RenderableData info = renderObj.Data;
                if (excludeUI && info.TypeInfo.UISprite != null)
                    continue;

                RenderContainer.Add(renderObj, info);
            }
        }

        public void GenerateRenderListsMultiThreaded(bool excludeUI)
        {
            QuickList<IPartitionedRenderable> renderedSprites = VisibleSprites;
            RenderContainer.Reset();

            QuickParallel.ForEach(renderedSprites, (objects, count) => {
                for (int i = 0; i < count; i++) {
                    IPartitionedRenderable renderObj = objects[i];
                    RenderableData info = renderObj.Data;
                    if (excludeUI && info.TypeInfo.UISprite != null)
                        continue;

                    RenderContainer.Add(renderObj, info, true);
                }
            });
        }

        public void ProcessRenderList(Camera2D camera, RenderList renderList)
        {
            if (renderList == null || renderList.Data.Count < 1)
                return;

            for (int i = 0; i < renderList.Data.Count; i++) {
                ThreadSafeList<IRenderable> data = renderList.Data.Array[i];
                Effect effect = DrawCallDatabase.LookupArray.Array[i].Effect;
                switch (renderList.Mode) {
                    case BlendMode.Opaque:
                        ChangeDrawCall(
                            SpriteSortMode.Deferred,
                            camera.ViewMatrix,
                            BlendState.Opaque,
                            SamplerState.PointClamp,
                            DepthStencilGreater, 
                            null, 
                            effect);
                        break;
                    case BlendMode.Transparent:
                        data.Sort(new DepthComparer());
                        ChangeDrawCall(
                            SpriteSortMode.Deferred, 
                            camera.ViewMatrix, 
                            BlendState.NonPremultiplied, // Fix: was BlendState.AlphaBlend.
                            SamplerState.PointClamp, 
                            DepthStencilGreater, 
                            null, 
                            effect);
                        break;
                    case BlendMode.Additive:
                        data.Sort(new DepthComparer());
                        ChangeDrawCall(
                            SpriteSortMode.Deferred,
                            camera.ViewMatrix,
                            BlendState.Additive,
                            SamplerState.PointClamp,
                            DepthStencilGreater,
                            null,
                            effect);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(renderList.Mode), renderList.Mode, null);
                }

                IRenderable[] renderArray = data.Array;
                for (int z = 0; z < data.Count; z++) {
                    renderArray[z].Render(camera, Space.World);
                }
            }
        }

        public void RenderUI()
        {
            foreach (UIObject topLevelMenu in UIManager.OrderedTopLevelMenus) {
                if (!topLevelMenu.Enabled)
                    continue;

                graphicsDevice.ScissorRectangle = scissorRectBackup;
                Rectangle curScissorRect = topLevelMenu.ScissorRect 
                                           ?? graphicsDevice.ScissorRectangle;

                graphicsDevice.ScissorRectangle = ApplyScreenRectangleScaling(curScissorRect);
                ChangeDrawCall(SpriteSortMode.FrontToBack, Screen.ScreenScaleMatrix, null, SamplerState.PointClamp, null, rasterizerScissor);
                UIRenderIteration(topLevelMenu.Transform, curScissorRect);
            }
        }

        private void UIRenderIteration(Transform transform, Rectangle? originalScissorRect)
        {
            if (transform.GameObject is UIObject drawnUIObject) { 
                DrawUIElement(drawnUIObject, originalScissorRect);
            }

            for (int i = 0; i < transform.Children.Count; i++) {
                Transform nextTransform = transform.Children.Array[i];
                Rectangle? curScissorRect = originalScissorRect;
                UIObject nextUIObj = null;

                if (nextTransform.GameObject is UIObject uiObj) {
                    nextUIObj = uiObj;
                }

                if (nextUIObj?.ScissorRect != null) {
                    curScissorRect = nextUIObj.ScissorRect;
                }
                if (curScissorRect.HasValue && graphicsDevice.ScissorRectangle != curScissorRect.Value) {
                    EndDrawCall();
                    graphicsDevice.ScissorRectangle = ApplyScreenRectangleScaling(curScissorRect.Value);
                    ChangeDrawCall(SpriteSortMode.FrontToBack, Screen.ScreenScaleMatrix, null, SamplerState.PointClamp, null, rasterizerScissor);
                    UIRenderIteration(nextTransform, curScissorRect);
                } else {
                    UIRenderIteration(nextTransform, originalScissorRect);
                }
            }
        }

        private void DrawUIElement(UIObject uiObj, Rectangle? scissorRect)
        {
            int len = uiObj.Sprites.Count;
            for (int s = 0; s < len; s++) {
                SpriteBase sprite = uiObj.Sprites.Array[s];
                Rectangle bounds = sprite.Bounds;
                if (scissorRect != null && scissorRect.Value.Intersects(bounds)) {
                    sprite.Render(null, Space.Screen);
                }
            }
        }

        private void RenderUIGizmos(Camera2D camera)
        {
            Core.Rendering.SpriteBatch.GraphicsDevice.ScissorRectangle = scissorRectBackup;
            foreach (UIObject topLevelGizmo in UIManager.GetGizmos()) {
                if (!topLevelGizmo.Enabled)
                    continue;

                graphicsDevice.ScissorRectangle = scissorRectBackup;
                Rectangle curScissorRect = topLevelGizmo.ScissorRect 
                                           ?? graphicsDevice.ScissorRectangle;

                graphicsDevice.ScissorRectangle = curScissorRect;
                ChangeDrawCall(SpriteSortMode.FrontToBack, Screen.ScreenScaleMatrix, null, SamplerState.PointClamp, null, rasterizerScissor);
                UIGizmoRenderIteration(camera, topLevelGizmo.Transform, curScissorRect);
            }
        }

        private void UIGizmoRenderIteration(Camera2D camera, Transform transform, Rectangle? originalScissorRect)
        {
            if (transform.GameObject is UIObject drawableUIObj) {
                DrawUIGizmo(drawableUIObj, originalScissorRect, camera);
            }

            for (int i = 0; i < transform.Children.Count; i++) {
                Rectangle? curScissorRect = originalScissorRect;
                Transform nextTransform = transform.Children.Array[i];
                UIObject nextUIObj = null;

                if (nextTransform.GameObject is UIObject uiObj) {
                    nextUIObj = uiObj;
                }

                if (nextUIObj?.ScissorRect != null) {
                    curScissorRect = nextUIObj.ScissorRect;
                }
                if (curScissorRect.HasValue && graphicsDevice.ScissorRectangle != curScissorRect.Value) {
                    EndDrawCall();
                    Rectangle r = curScissorRect.Value;
                    r.X -= viewPortX;
                    r.Y -= viewPortY;
                    graphicsDevice.ScissorRectangle = r;
                    ChangeDrawCall(SpriteSortMode.FrontToBack, Screen.ScreenScaleMatrix, null, SamplerState.PointClamp, null, rasterizerScissor);
                    UIGizmoRenderIteration(camera, nextTransform, curScissorRect);
                } else {
                    UIGizmoRenderIteration(camera, nextTransform, originalScissorRect);
                }
            }
        }

        private void DrawUIGizmo(UIObject uiObj, Rectangle? scissorRect, Camera2D camera)
        {
            int len = uiObj.Sprites.Count;
            for (int s = 0; s < len; s++) {
                SpriteBase sprite = uiObj.Sprites.Array[s];
                Rectangle bounds = sprite.Bounds;
                bounds.X -= (int)camera.Position.X;
                bounds.Y -= (int)camera.Position.Y;
                if (scissorRect != null && scissorRect.Value.Intersects(bounds)) {
                    sprite.Render(camera, Space.World);
                }
            }
        }

        public void DrawParticles(Camera2D cam)
        {
            if (ParticleEngine.UseParticleRenderer) {
                DrawParticlesInstanced(cam);
            } else {
                DrawParticlesSpriteBatch(cam);
            }
        }

        private void DrawParticlesInstanced(Camera2D cam)
        {
            tmpEmitters.Clear();
            ParticleEngine.GetEmitters(Particles.BlendMode.Alpha, tmpEmitters, SearchFlags.Visible);
            if (tmpEmitters != null) {
                foreach (Emitter pEmitter in tmpEmitters) {
                    pEmitter.Draw(cam.ViewMatrix);
                }
            }

            tmpEmitters.Clear();
            ParticleEngine.GetEmitters(Particles.BlendMode.Additive, tmpEmitters, SearchFlags.Visible);
            if (tmpEmitters != null) {
                foreach (Emitter pEmitter in tmpEmitters) {
                    pEmitter.Draw(cam.ViewMatrix);
                }
            }
        }

        private void DrawParticlesSpriteBatch(Camera2D cam)
        {
            tmpEmitters.Clear();
            ParticleEngine.GetEmitters(Particles.BlendMode.Alpha, tmpEmitters, SearchFlags.Visible);
            if (tmpEmitters != null) {
                ChangeDrawCall(SpriteSortMode.Deferred, cam.ViewMatrix, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, ParticleShader);
                foreach (Emitter pEmitter in tmpEmitters) {
                    DrawNewParticleEmitter(cam, pEmitter);
                }
            }

            tmpEmitters.Clear();
            ParticleEngine.GetEmitters(Particles.BlendMode.Additive, tmpEmitters, SearchFlags.Visible);
            if (tmpEmitters != null) {
                ChangeDrawCall(SpriteSortMode.Deferred, cam.ViewMatrix, BlendState.Additive, SamplerState.PointClamp, null, null, ParticleShader);
                foreach (Emitter pEmitter in tmpEmitters) {
                    DrawNewParticleEmitter(cam, pEmitter);
                }
            }
        }

        private unsafe void DrawNewParticleEmitter(Camera2D cam, Emitter pEmitter)
        {
            Vector2 camPos = new Vector2(cam.Position.X, cam.Position.Y);
            Span<Particle> particles = pEmitter.ActiveParticles;
            Texture2D tex = pEmitter.Texture;
            fixed (Particle* ptr = particles) {
                int size = particles.Length;
                Particle* tail = ptr + size;
                for (Particle* particle = ptr; particle < tail; particle++) {
                    ref Int4 particleRect = ref particle->SourceRectangle;
                    ref Vector4 particleCol = ref particle->Color;

                    Rectangle sourceRect = new Rectangle(particleRect.X, particleRect.Y, particleRect.Width, particleRect.Height);
                    Vector2 origin = new Vector2(sourceRect.Width / 2.0f, sourceRect.Width / 2.0f);
                    Color color = new Color(particleCol.X / 360, particleCol.Y, particleCol.Z, particleCol.W);
                    Core.Rendering.SpriteBatch.Draw(tex, 
                        particle->Position,
                        sourceRect,
                        color,
                        particle->SpriteRotation,
                        origin,
                        particle->Scale,
                        particle->layerDepth);
                }
            }
        }

        public enum RenderModes
        {
            Deferred,
            Albedo,
            Normal,
            Depth,
            Diffuse,
            Specular,
            Volumetric,
            //Hologram,
            SSAO,
            SSBlur,
            //Emissive,
            SSR,
            HDR
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public struct DepthComparer : IComparer<IRenderable>
        {
            int IComparer<IRenderable>.Compare(IRenderable x, IRenderable y)
                => ((SpriteBase)x).LayerDepth.CompareTo(((SpriteBase)y).LayerDepth);
        }
    }

    public enum BlendMode
    {
        Opaque,
        Transparent,
        Additive
    }
}