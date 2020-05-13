using System;
using System.Collections.Generic;
using DeeZ.Core;
using DeeZ.Engine.Utility;
using Microsoft.Xna.Framework;
using SE.Common;
using SE.Core;
using SE.UI.Events;
using Vector2 = System.Numerics.Vector2;

namespace SE.UI
{
    /// <summary>
    /// Panels act as the backgrounds of buttons, windows and other UI elements.
    /// </summary>
    public class ScrollView : UIObject
    {
        public ScrollBar ScrollBar;

        private Point scrollPortSize;
        private float oldScrollValue;

        /// <summary>If set to true, UIObjects which are outside of the currently visible scroll-view bounds will be disabled.
        /// And enabled if they enter the scroll-view bounds. Improves performance at the cost of flexibility. Defaults to false.</summary>
        public bool DisableHidden { get; set; } = false;

        public Point ScrollPortSize {
            get => scrollPortSize;
            set {
                scrollPortSize = value;
                if (ScissorRect.HasValue) {
                    Rectangle scissorRectValue = ScissorRect.Value;
                    if (value.X < scissorRectValue.Width) {
                        scissorRectValue.Width = value.X;
                    }
                    if (value.Y < scissorRectValue.Height) {
                        scissorRectValue.Height = value.Y;
                    }
                    ScissorRect = scissorRectValue;

                    Bounds = new RectangleF(Transform.GlobalPositionInternal.X, Transform.GlobalPositionInternal.Y, value.X + ScissorRect.Value.Width, value.Y + ScissorRect.Value.Height);
                    scrollPortSize = new Point(Math.Max(value.X - ScissorRect.Value.Width, 0), Math.Max(value.Y - ScissorRect.Value.Height, 0));
                } else {
                    Bounds = new RectangleF(Transform.GlobalPositionInternal.X, Transform.GlobalPositionInternal.Y, value.X, value.Y);
                    scrollPortSize = new Point((int)MathF.Max(value.X - Bounds.Width, 0), (int)MathF.Max(value.Y - Bounds.Height, 0));
                    ProcessHidden();
                }
            }
        }

        protected override void OnEnable(bool enableAllChildren = false)
        {
            base.OnEnable(enableAllChildren);
            ProcessHidden();
        }

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (MouseIn) {
                if (UIManager.AssumedScrollWheelControl == null) {
                    UIManager.AssumedScrollWheelControl = this;
                }
                if (UIManager.AssumedScrollWheelControl == this && InputManager.MouseScrollValue != 0) {
                    ScrollBar?.Scroll(-(ScrollBar.Handle.Bounds.Width * 0.1f) * InputManager.MouseScrollValue);
                    ProcessHidden();
                }
            } else {
                if (UIManager.AssumedScrollWheelControl == this) {
                    UIManager.AssumedScrollWheelControl = null;
                }
            }
            if (!oldScrollValue.Equals(ScrollBar.Value)) {
                Transform.Position = new Vector2(Transform.Position.X, ScrollPortSize.Y * -ScrollBar.Value);
                ProcessHidden();
            }
            oldScrollValue = ScrollBar.Value;
        }

        private void ProcessHidden()
        {
            if(!DisableHidden)
                return;

            List<Transform> children = Transform.Children;
            for (int i = 0; i < children.Count; i++) {
                Transform child = children[i];
                if (child.GameObject.Enabled) {
                    if (!child.GameObject.Bounds.Intersects(ScissorRect.Value)) {
                        child.GameObject.Disable();
                    }
                } else {
                    child.GameObject.RecalculateBounds();
                    if (child.GameObject.Bounds.Intersects(ScissorRect.Value)) {
                        child.GameObject.Enable();
                    }
                }
            }
        }

        /// <inheritdoc />
        protected sealed override void OnInitialize()
        {
            base.OnInitialize();
            Transform.ChildAdded += t => ProcessHidden();
            Transform.ChildRemoved += t => ProcessHidden();
        }

        /// <inheritdoc />
        public override void OnSelected(MouseEventArgs mouseEventArgs)
        {
            base.OnSelected(mouseEventArgs);
        }

        /// <inheritdoc />
        public override void OnDeselected(MouseEventArgs mouseEventArgs)
        {
            base.OnDeselected(mouseEventArgs);
            if (UIManager.AssumedScrollWheelControl == this) {
                UIManager.AssumedScrollWheelControl = null;
            }
        }

        /// <inheritdoc />
        public override void OnClick(MouseEventArgs mouseEventArgs)
        {
            base.OnClick(mouseEventArgs);
        }

        public ScrollView(Vector2 pos, Point size, Rectangle? scissorRect = null) : base(pos, size)
        {
            ScissorRect = scissorRect;
            ScrollPortSize = size;
            Interactable = true;
        }
    }
}
