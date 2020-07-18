using System;
using SE.Input;
using Vector2 = System.Numerics.Vector2;

namespace SE.UI.Events
{
    public class MouseEventArgs : EventArgs
    {
        public Vector2? MousePoint { get; set; }
        public MouseButtons MouseButton { get; set; }

        public MouseEventArgs(MouseButtons? mouseButton, Vector2 mousePoint)
        {
            MouseButton = mouseButton ?? MouseButtons.None;
            MousePoint = mousePoint;
        }

        public MouseEventArgs(MouseButtons? mouseButton, Vector2? mousePoint)
        {
            MouseButton = mouseButton ?? MouseButtons.None;
            MousePoint = mousePoint;
        }

        public MouseEventArgs() { }

    }
}
