using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using DeeZ.Editor.GUI.Viewport;
using DeeZ.Engine.Utility;
using ImGuiNET;

namespace DeeZ.Editor.GUI
{
    public static class EditorGUI
    {
        public static View View { get; private set; }
        public static Properties.Properties Properties { get; private set; }
        public static QuickList<GUIObject> guiObjects { get; } = new QuickList<GUIObject>();

        public static void Initialize()
        {
            guiObjects.Clear();
            InitializeStyle();

            View = new View(Core.Rendering.FinalRender, new Vector2(1920 - 1477, 18),
                new Vector2(1920 / 1.3f, 1080 / 1.3f));

            Properties = new Properties.Properties();

            guiObjects.Add(new Hierarchy.Hierarchy());
            guiObjects.Add(View);
            guiObjects.Add(Properties);
        }

        public static void Paint()
        {
            GUI.BeginMainMenuBar();
            GUI.MenuItem("File");
            for (int i = 0; i < guiObjects.Count; i++) {
                guiObjects.Array[i].OnPaint();
            }
            ImGui.ShowDemoWindow();
        }

        public static void InitializeStyle()
        {
            ImGuiStylePtr style = ImGui.GetStyle();
            style.WindowPadding = new Vector2(6.0f, 6.0f);
            style.IndentSpacing = 16.0f;
            style.ItemSpacing = new Vector2(6.0f, 4.0f);
            style.GrabMinSize = 14.0f;

            style.WindowRounding = 0.0f;
            style.FrameRounding = 0.0f;
            style.ScrollbarRounding = 0.0f;
            style.ScrollbarSize = 16.0f;
            style.GrabRounding = 2.0f;

            style.WindowBorderSize = 0.0f;
            style.ChildBorderSize = 0.0f;
            style.FrameBorderSize = 0.0f;

            style.Colors[(int) ImGuiCol.WindowBg] = new Vector4(0.050f, 0.050f, 0.050f, 1.000f);
            style.Colors[(int) ImGuiCol.FrameBg] = new Vector4(0.254f, 0.254f, 0.254f, 0.540f);
            style.Colors[(int) ImGuiCol.TitleBg] = new Vector4(0.031f, 0.031f, 0.031f, 1.000f);
            style.Colors[(int) ImGuiCol.ScrollbarBg] = new Vector4(0.031f, 0.031f, 0.031f, 1.000f);
            style.Colors[(int) ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.200f, 0.430f, 0.710f, 1.000f);
            style.Colors[(int) ImGuiCol.ScrollbarGrabActive] = new Vector4(0.260f, 0.590f, 0.980f, 1.000f);
        }

        public static class Colors
        {
            public static Vector4 TextDisabled { get; set; } = new Vector4(0.65f, 0.65f, 0.65f, 1.0f);
            public static Vector4 FrameBgDisabled { get; set; } = new Vector4(0.11f, 0.11f, 0.11f, 1.0f);
        }
    }
}
