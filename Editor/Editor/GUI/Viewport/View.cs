using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE;
using SE.Core;
using Vector2 = System.Numerics.Vector2;

namespace DeeZ.Editor.GUI.Viewport
{
    public class View : GUIObject
    {
        private Texture2D texture;
        private IntPtr boundTexture;

        private Vector2 pos;
        private Vector2 size;

        public Vector2 ViewPos { get; private set; }
        public Vector2 ViewSize { get; private set; }
        public Rectangle ViewBounds { get; private set; }

        int x = 0;

        public override void OnPaint()
        {
            GUI.Begin("View", 
                GUIWindowFlags.NoScrollbar
                | GUIWindowFlags.NoCollapse
                | GUIWindowFlags.NoResize
                //| ImGuiWindowFlags.NoMove
                | GUIWindowFlags.NoScrollWithMouse
                | GUIWindowFlags.NoTitleBar);

            //ImGui.SetWindowSize(size);
            //ImGui.SetWindowPos(pos);

            ViewPos = GUI.GetWindowPos();
            ViewSize = new Vector2(GUI.GetWindowWidth() - 12, GUI.GetWindowHeight() - 16);

            // Magical numbers to fix the viewport scaling.
            ViewBounds = new Rectangle((int)ViewPos.X + 7, (int)ViewPos.Y + 7, (int)ViewSize.X, (int)ViewSize.Y);
            Screen.EditorViewBounds = ViewBounds;

            GUI.Image(boundTexture, ViewSize, Vector2.Zero, Vector2.One);
            GUI.End();
        }

        public View(Texture2D texture, Vector2 position, Vector2 size)
        {
            this.texture = texture;
            this.size = size;
            pos = position;

            boundTexture = EditorApp.ImGuiRenderer.BindTexture(texture);
        }

    }
}
