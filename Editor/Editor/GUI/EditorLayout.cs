﻿using System;
using System.Collections.Generic;
using System.Text;
using SE.Core;
using SE.Editor.GUI.Windows;

namespace SE.Editor.GUI
{
    public static class EditorLayoutManager
    {
        public static void SwapToLayout(EditorLayout layout)
        {
            // Clear all windows...
            EditorGUI.guiObjects.Clear();
            EditorGUI.ShowMainMenuBar = layout.ShowMainMenuBar;
            layout.SwapTo();
        }
    }

    public abstract class EditorLayout
    {
        public abstract bool ShowMainMenuBar { get; }
        public abstract void SwapTo();
    }

    public class EditorLayoutUser : EditorLayout
    {
        public sealed override bool ShowMainMenuBar => true;

        public override void SwapTo()
        {
            throw new NotImplementedException();
        }
    }

    public class IntroLayout : EditorLayout
    {
        public sealed override bool ShowMainMenuBar => false;

        public override void SwapTo()
        {
            Screen.SetScreenSize(1000, 600);
            EditorGUI.guiObjects.Add(new IntroWindow());
        }
    }
}
