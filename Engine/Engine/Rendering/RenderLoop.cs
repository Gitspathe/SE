using System;
using System.Collections.Generic;
using SE.Components;
using SE.Core;
using Console = SE.Core.Console;

namespace SE.Rendering
{
    public static class RenderLoop
    {
        internal static SortedDictionary<int, Action<Camera2D>> Loop { get; } = new SortedDictionary<int, Action<Camera2D>>();
        internal static DefaultRenderer DefaultRender = new DefaultRenderer();

        internal static bool IsDirty;

        public static void Add(int order, Action<Camera2D> action)
        {
            Loop.Add(order, action);
            IsDirty = true;
        }

        public static void Add(LoopEnum order, Action<Camera2D> action)
        {
            Add((int)order, action);
        }

        public enum LoopEnum
        {
            PrepareFrame = 0,
            Culling = 100,
            GenerateRenderLists = 200,
            LightingStart = 300,
            DuringLighting = 1000,
            FinalizeParticles = 5000,
            RenderAlphaParticles = 5100,
            LightingEnd = 6000,
            RenderAlphaParticlesUnlit = 6100,
            RenderAdditiveParticles = 6200,
            AfterLighting = 10000,
            DrawUnderlay = 20000,
            DrawUI = 30000,
            DrawRenderTargets = 40000
        }

        static RenderLoop()
        {
            // Do not set rendering loop if fully headless.
            if(Screen.IsFullHeadless)
                return;

            // Headless mode.
            if (Screen.DisplayMode == DisplayMode.Headless) {
                Add(LoopEnum.PrepareFrame, DefaultRender.NewFrame);
                Add(LoopEnum.Culling, Core.Rendering.PerformCulling);
                Add(LoopEnum.DrawUI, DefaultRender.DrawUI);
                Add(LoopEnum.DrawRenderTargets, Core.Rendering.DrawRenderTargets);
                return;
            }

            // Full graphics mode.
            Add(LoopEnum.PrepareFrame - 100, Core.Lighting.Update);
            Add(LoopEnum.PrepareFrame, DefaultRender.NewFrame);
            Add(LoopEnum.Culling, Core.Rendering.PerformCulling);
            Add(LoopEnum.GenerateRenderLists, cam => DefaultRender.GenerateRenderLists());
            Add(LoopEnum.LightingStart, _ => DefaultRender.StartLighting());

            Add(LoopEnum.FinalizeParticles, _ => ParticleEngine.FinalizeThreads());

            Add(LoopEnum.RenderAlphaParticles, cam
                => DefaultRender.RenderAlphaParticles(cam, ParticleEngine.AlphaParticles));

            Add(LoopEnum.LightingEnd, _
                => DefaultRender.EndLighting());

            Add(LoopEnum.RenderAlphaParticlesUnlit, cam
                => DefaultRender.RenderAlphaParticles(cam, ParticleEngine.AlphaParticlesUnlit));

            Add(LoopEnum.RenderAdditiveParticles, cam
                => DefaultRender.RenderAdditiveParticles(cam, ParticleEngine.AdditiveParticles));

            Add(LoopEnum.DrawUnderlay, cam => Console.DrawUnderlay(cam, Core.Rendering.SpriteBatch));
            Add(LoopEnum.DrawUI, DefaultRender.DrawUI);
            Add(LoopEnum.DrawRenderTargets, Core.Rendering.DrawRenderTargets);
        }

        public new static string ToString()
        {
            string s = "";
            foreach (KeyValuePair<int, Action<Camera2D>> loop in Loop) {
                if (loop.Value.Method.ReflectedType != null) {
                    s += "  " + loop.Key + ", " + loop.Value.Method.ReflectedType.FullName + "." + loop.Value.Method.Name + "\n";
                }
            }
            return s;
        }
    }
}
