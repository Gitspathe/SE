using SE.Core;
using SE.Utility;
using System.Collections.Generic;

namespace SE.Rendering
{
    public static class RenderLoop
    {
        internal static SortedDictionary<uint, QuickList<IRenderLoopAction>> Loop { get; } = new SortedDictionary<uint, QuickList<IRenderLoopAction>>();
        internal static Renderer Render = new Renderer();

        internal static bool IsDirty;

        public static void Add(uint order, IRenderLoopAction action)
        {
            if (Loop.TryGetValue(order, out QuickList<IRenderLoopAction> loopSpot)) {
                if (!loopSpot.Contains(action)) {
                    loopSpot.Add(action);
                }
            } else {
                Loop.Add(order, new QuickList<IRenderLoopAction> { action });
            }

            IsDirty = true;
        }

        public static void Add(LoopEnum order, IRenderLoopAction action)
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
            LightingEnd = 6000,
            RenderUnlitParticles = 6100,
            AfterLighting = 10000,
            DrawUnderlay = 20000,
            DrawUI = 30000,
            DrawRenderTargets = 40000
        }

        static RenderLoop()
        {
            // Do not set rendering loop if fully headless.
            if (Screen.IsFullHeadless)
                return;

            // Headless mode.
            if (Screen.DisplayMode == DisplayMode.Headless) {
                Add(LoopEnum.PrepareFrame, new LoopPrepareFrame(Render));
                Add(LoopEnum.Culling, new LoopCulling());
                Add(LoopEnum.DrawUI, new LoopUI(Render));
                Add(LoopEnum.DrawRenderTargets, new LoopRenderTargets());
                return;
            }

            // Full graphics mode.
            Add(LoopEnum.NewFrame, new LoopLighting());
            Add(LoopEnum.PrepareFrame, new LoopPrepareFrame(Render));
            Add(LoopEnum.Culling, new LoopCulling());
            Add(LoopEnum.GenerateRenderLists, new LoopGenerateRenderLists(Render));
            Add(LoopEnum.LightingStart, new LoopStartLighting(Render));
            Add(LoopEnum.LightingStart + 1, new LoopTileMaps(Render));

            Add(LoopEnum.FinalizeParticles, new LoopFinalizeParticles());

            Add(LoopEnum.LightingEnd, new LoopEndLighting(Render));

            Add(LoopEnum.FinalizeParticles+1, new NewRender(Render));

            Add(LoopEnum.RenderUnlitParticles, new LoopParticles(Render));

            Add(LoopEnum.DrawUnderlay, new LoopConsoleUnderlay());
            Add(LoopEnum.DrawUI, new LoopUI(Render));
            Add(LoopEnum.DrawRenderTargets, new LoopRenderTargets());
        }

        public new static string ToString()
        {
            string s = "";
            foreach ((uint key, QuickList<IRenderLoopAction> value) in Loop) {
                s += "  " + key + ": ";
                for (int i = 0; i < value.Count; i++) {
                    s += value.Array[i].Name;
                    if (i + 1 < value.Count) {
                        s += ", ";
                    }
                }
            }
            return s;
        }
    }
}
