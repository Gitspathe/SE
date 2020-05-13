using System;
using Vector2 = System.Numerics.Vector2;

namespace SE.UI.Events
{
    public class MouseDragEventArgs : EventArgs
    {
        public Vector2? DragPos { get; private set; }
        public Vector2? StartDragPos { get; private set; }

        public MouseDragEventArgs(Vector2? dragPos, Vector2? startDragPos)
        {
            DragPos = dragPos;
            StartDragPos = startDragPos;
        }
    }
}
