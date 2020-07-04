using System;
using System.Collections.Generic;
using SE.Components;
using SE.Core;
using Console = SE.Core.Console;

namespace SE.Rendering
{
    // TODO: Allow multiple render actions under a single enum position.
    public static class RenderLoop
    {
        internal static SortedDictionary<uint, Action<Camera2D>> Loop { get; } = new SortedDictionary<uint, Action<Camera2D>>();
        internal static Renderer Render = new Renderer();

        internal static bool IsDirty;

        public static void Add(uint order, Action<Camera2D> action)
        {
            Loop.Add(order, action);
            IsDirty = true;
        }

        public static void Add(LoopEnum order, Action<Camera2D> action)
        {
            Add((uint)order, action);
        }

        public enum LoopEnum : uint
        {
            NewFrame = 0,
            PrepareFrame = 1,
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
                Add(LoopEnum.PrepareFrame, Render.NewFrame);
                Add(LoopEnum.Culling, Core.Rendering.PerformCulling);
                Add(LoopEnum.DrawUI, Render.DrawUI);
                Add(LoopEnum.DrawRenderTargets, Core.Rendering.DrawRenderTargets);
                return;
            }

            // Full graphics mode.
            Add(LoopEnum.NewFrame, Core.Lighting.Update);
            Add(LoopEnum.PrepareFrame, Render.NewFrame);
            Add(LoopEnum.Culling, Core.Rendering.PerformCulling);
            Add(LoopEnum.GenerateRenderLists, cam => Render.GenerateRenderLists());
            Add(LoopEnum.LightingStart, _ => Render.StartLighting());

            Add(LoopEnum.FinalizeParticles, _ => ParticleEngine.WaitForThreads());

            Add(LoopEnum.LightingEnd, _
                => Render.EndLighting());

            Add(LoopEnum.RenderAlphaParticlesUnlit, cam => Render.DrawNewParticles(cam));

            Add(LoopEnum.DrawUnderlay, cam => Console.DrawUnderlay(cam, Core.Rendering.SpriteBatch));
            Add(LoopEnum.DrawUI, Render.DrawUI);
            Add(LoopEnum.DrawRenderTargets, Core.Rendering.DrawRenderTargets);
        }

        public new static string ToString()
        {
            string s = "";
            foreach (KeyValuePair<uint, Action<Camera2D>> loop in Loop) {
                if (loop.Value.Method.ReflectedType != null) {
                    s += "  " + loop.Key + ", " + loop.Value.Method.ReflectedType.FullName + "." + loop.Value.Method.Name + "\n";
                }
            }
            return s;
        }
    }
}
