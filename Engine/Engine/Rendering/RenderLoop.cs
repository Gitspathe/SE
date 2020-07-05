﻿using System;
using System.Collections.Generic;
using SE.Components;
using SE.Core;
using Console = SE.Core.Console;

namespace SE.Rendering
{
    // TODO: Allow multiple render actions under a single enum position.
    public static class RenderLoop
    {
        internal static SortedDictionary<uint, IRenderLoopAction> Loop { get; } = new SortedDictionary<uint, IRenderLoopAction>();
        internal static Renderer Render = new Renderer();

        internal static bool IsDirty;

        public static void Add(uint order, IRenderLoopAction action)
        {
            Loop.Add(order, action);
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

            Add(LoopEnum.FinalizeParticles, new LoopFinalizeParticles());

            Add(LoopEnum.LightingEnd, new LoopEndLighting(Render));

            Add(LoopEnum.RenderAlphaParticlesUnlit, new LoopParticles(Render));

            Add(LoopEnum.DrawUnderlay, new LoopConsoleUnderlay());
            Add(LoopEnum.DrawUI, new LoopUI(Render));
            Add(LoopEnum.DrawRenderTargets, new LoopRenderTargets());
        }

        public new static string ToString()
        {
            string s = "";
            foreach (KeyValuePair<uint, IRenderLoopAction> loop in Loop) {
                s += "  " + loop.Key + ", " + loop.Value.Name + ".\n";
            }
            return s;
        }
    }
}
