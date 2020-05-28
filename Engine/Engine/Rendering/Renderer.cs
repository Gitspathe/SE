using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Common;
using SE.Components;
using SE.Core;
using SE.Particles;
using SE.UI;
using SE.Utility;
using SE.World.Partitioning;
using SEParticles;
using Console = SE.Core.Console;
using static SE.Core.Rendering;
using Color = Microsoft.Xna.Framework.Color;
using Particle = SE.Particles.Particle;
using ParticleSystem = SE.Components.ParticleSystem;
using Vector2 = System.Numerics.Vector2;

namespace SE.Rendering
{
    public class Renderer : IRenderer
    {
        public RenderContainer RenderContainer = new RenderContainer();

        private RasterizerState rasterizerScissor = new RasterizerState { ScissorTestEnable = true };
        private int viewPortX;
        private int viewPortY;
        private Rectangle scissorRectBackup = new Rectangle(0, 0, 1920, 1080);

        private GraphicsDevice GraphicsDevice = Core.Rendering.GraphicsDevice;

        public static bool Multithreaded { get; set; } = false;
        public static int CullingThreshold { get; set; } = 128;

        public void NewFrame(Camera2D camera)
        {
            Prepare();
            GraphicsDevice.SetRenderTarget(SceneRender);
            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 0f, 0);
            ChangeDrawCall(SpriteSortMode.Deferred, camera.ScaleMatrix, BlendState.Opaque, SamplerState.PointClamp, DepthStencilGreater);

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
            GraphicsDevice.SetRenderTarget(UIRender);
            GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0.0f, 0);
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
            QuickList<IPartitionObject> renderedSprites = VisibleSprites;
            RenderContainer.Reset();

            IPartitionObject[] spriteArray = renderedSprites.Array;
            for (int i = 0; i < renderedSprites.Count; i++) {
                SpriteBase sprite = (SpriteBase)spriteArray[i];
                if (excludeUI && sprite.IsUISprite)
                    continue;

                RenderContainer.Add(sprite);
            }
        }

        public void GenerateRenderListsMultiThreaded(bool excludeUI)
        {
            QuickList<IPartitionObject> renderedSprites = VisibleSprites;
            RenderContainer.Reset();

            OrderablePartitioner<IPartitionObject> partitioner = Partitioner.Create(renderedSprites);
            Parallel.ForEach(partitioner, (obj, loopstate) => {
                SpriteBase sprite = (SpriteBase)obj;
                if (excludeUI && sprite.IsUISprite)
                    return;

                RenderContainer.Add(sprite, true);
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
                            camera.ScaleMatrix,
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
                            camera.ScaleMatrix, 
                            BlendState.AlphaBlend, 
                            SamplerState.PointClamp, 
                            DepthStencilGreater, 
                            null, 
                            effect);
                        break;
                    case BlendMode.Additive:
                        data.Sort(new DepthComparer());
                        ChangeDrawCall(
                            SpriteSortMode.Deferred,
                            camera.ScaleMatrix,
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

                GraphicsDevice.ScissorRectangle = scissorRectBackup;
                Rectangle curScissorRect = topLevelMenu.ScissorRect 
                                           ?? GraphicsDevice.ScissorRectangle;

                GraphicsDevice.ScissorRectangle = ApplyScreenRectangleScaling(curScissorRect);
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
                Transform nextTransform = transform.Children[i];
                Rectangle? curScissorRect = originalScissorRect;
                UIObject nextUIObj = null;

                if (nextTransform.GameObject is UIObject uiObj) {
                    nextUIObj = uiObj;
                }

                if (nextUIObj?.ScissorRect != null) {
                    curScissorRect = nextUIObj.ScissorRect;
                }
                if (curScissorRect.HasValue && GraphicsDevice.ScissorRectangle != curScissorRect.Value) {
                    EndDrawCall();
                    GraphicsDevice.ScissorRectangle = ApplyScreenRectangleScaling(curScissorRect.Value);
                    ChangeDrawCall(SpriteSortMode.FrontToBack, Screen.ScreenScaleMatrix, null, SamplerState.PointClamp, null, rasterizerScissor);
                    UIRenderIteration(nextTransform, curScissorRect);
                } else {
                    UIRenderIteration(nextTransform, originalScissorRect);
                }
            }
        }

        private void DrawUIElement(UIObject uiObj, Rectangle? scissorRect)
        {
            int len = uiObj.Sprites.Length;
            for (int s = 0; s < len; s++) {
                SpriteBase sprite = uiObj.Sprites[s];
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

                GraphicsDevice.ScissorRectangle = scissorRectBackup;
                Rectangle curScissorRect = topLevelGizmo.ScissorRect 
                                           ?? GraphicsDevice.ScissorRectangle;

                GraphicsDevice.ScissorRectangle = curScissorRect;
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
                Transform nextTransform = transform.Children[i];
                UIObject nextUIObj = null;

                if (nextTransform.GameObject is UIObject uiObj) {
                    nextUIObj = uiObj;
                }

                if (nextUIObj?.ScissorRect != null) {
                    curScissorRect = nextUIObj.ScissorRect;
                }
                if (curScissorRect.HasValue && GraphicsDevice.ScissorRectangle != curScissorRect.Value) {
                    EndDrawCall();
                    Rectangle r = curScissorRect.Value;
                    r.X -= viewPortX;
                    r.Y -= viewPortY;
                    GraphicsDevice.ScissorRectangle = r;
                    ChangeDrawCall(SpriteSortMode.FrontToBack, Screen.ScreenScaleMatrix, null, SamplerState.PointClamp, null, rasterizerScissor);
                    UIGizmoRenderIteration(camera, nextTransform, curScissorRect);
                } else {
                    UIGizmoRenderIteration(camera, nextTransform, originalScissorRect);
                }
            }
        }

        private void DrawUIGizmo(UIObject uiObj, Rectangle? scissorRect, Camera2D camera)
        {
            int len = uiObj.Sprites.Length;
            for (int s = 0; s < len; s++) {
                SpriteBase sprite = uiObj.Sprites[s];
                Rectangle bounds = sprite.Bounds;
                bounds.X -= (int)camera.Position.X;
                bounds.Y -= (int)camera.Position.Y;
                if (scissorRect != null && scissorRect.Value.Intersects(bounds)) {
                    sprite.Render(camera, Space.World);
                }
            }
        }

        public void RenderAlphaParticles(Camera2D cam, Dictionary<int, ParticleRendererContainer> particleData)
        {
            if (particleData.Count < 1)
                return;

            foreach ((int drawIndex, ParticleRendererContainer particles) in particleData) {
                DrawCall drawCall = DrawCallDatabase.LookupArray.Array[drawIndex];

                ChangeDrawCall(SpriteSortMode.Deferred, cam.ScaleMatrix, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilGreater, null, drawCall.Effect);
                Vector2 camPos = cam.Position;
                for (int i = 0; i < particles.Length; i++) {
                    for (int ii = 0; ii < particles.ParticlesRenderData[i].Count; ii++) {
                        Particle p = particles.ParticlesRenderData[i].Array[ii];
                        Core.Rendering.SpriteBatch.Draw(drawCall.Texture, p.GlobalPosition - camPos, p.sourceRect, p.CurrentColor, p.CurrentRotation, p.Origin, p.CurrentScale, p.LayerDepth);
                    }
                }
            }
        }

        public void RenderAdditiveParticles(Camera2D cam, Dictionary<int, ParticleRendererContainer> particleData)
        {
            if (particleData.Count < 1)
                return;

            foreach ((int drawIndex, ParticleRendererContainer particles) in particleData) {
                DrawCall drawCall = DrawCallDatabase.LookupArray.Array[drawIndex];

                ChangeDrawCall(SpriteSortMode.Deferred, cam.ScaleMatrix, BlendState.Additive, SamplerState.PointClamp, DepthStencilGreater, null, drawCall.Effect);
                Vector2 camPos = cam.Position;
                for (int i = 0; i < particles.Length; i++) {
                    for (int ii = 0; ii < particles.ParticlesRenderData[i].Count; ii++) {
                        Particle p = particles.ParticlesRenderData[i].Array[ii];
                        Core.Rendering.SpriteBatch.Draw(drawCall.Texture, p.GlobalPosition - camPos, p.sourceRect, p.CurrentColor, p.CurrentRotation, p.Origin, p.CurrentScale, p.LayerDepth);
                    }
                }
            }
        }

        public static BlendState Alpha => new BlendState
        {
            ColorSourceBlend = Blend.SourceAlpha,
            AlphaSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            AlphaDestinationBlend = Blend.InverseSourceAlpha,
        };

        public static BlendState AlphaSubtract => new BlendState
        {
            ColorBlendFunction = BlendFunction.ReverseSubtract,
            ColorSourceBlend = Blend.SourceAlpha,
            AlphaSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            AlphaDestinationBlend = Blend.InverseSourceAlpha
        };

        public unsafe void DrawNewParticles(Camera2D cam)
        {
            AlphaSubtract.IndependentBlendEnable = true;
            ChangeDrawCall(SpriteSortMode.Deferred, cam.ScaleMatrix, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilGreater, null, TestEffect);
            Vector2 camPos = cam.Position;

            foreach (Emitter pEmitter in SEParticles.ParticleEngine.Emitters) {
                Span<SEParticles.Particle> particles = pEmitter.ActiveParticles;
                
                Texture2D tex = pEmitter.Texture;

                fixed (SEParticles.Particle* ptr = particles) {
                    int size = particles.Length;
                    SEParticles.Particle* tail = ptr + size;

                    for (SEParticles.Particle* particle = ptr; particle < tail; particle++) {
                        Rectangle sourceRect = particle->SourceRectangle;
                        Vector2 origin = new Vector2(
                            sourceRect.Width / 2.0f,
                            sourceRect.Width / 2.0f);

                        System.Numerics.Vector4 particleC = particle->Color;
                        Color color = new Color(particleC.X / 360, particleC.Y, particleC.Z, particleC.W);

                        Core.Rendering.SpriteBatch.Draw(tex,
                            particle->Position - camPos,
                            sourceRect,
                            color,
                            particle->Rotation,
                            origin,
                            particle->Scale,
                            1.0f);
                    }
                }
            }
            EndDrawCall();
        }

        public struct DepthComparer : IComparer<IRenderable>
        {
            int IComparer<IRenderable>.Compare(IRenderable x, IRenderable y) {
                SpriteBase xBase = (SpriteBase)x;
                SpriteBase yBase = (SpriteBase)y;
                return xBase.LayerDepth.CompareTo(yBase.LayerDepth);
            }
        }
    }

    public enum BlendMode
    {
        Opaque,
        Transparent,
        Additive
    }
}