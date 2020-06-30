using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SE.Common;
using SE.Components.UI;
using SE.Core;
using SE.UI.Events;
using SE.Core.Extensions;
using SE.Engine.Input;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;

namespace SE.UI
{
    public class UIObject : GameObject
    {
        /// <summary>Draw and event order for the UI object.</summary>
        public int Priority { get; protected set; }

        public override bool SerializeToScene => false;

        public UIObject RootUIObject { get; private set; }

        public virtual bool IsRootUIMenu { get; } = false;

        public virtual bool BlocksSelection { get; set; } = false;

        public virtual string RootUIName { get; } = null;

        public virtual int RootUIPriority { get; } = 0;

        public override bool DestroyOnLoad => false;

        /// <inheritdoc />
        public sealed override bool IsDynamic => true;

        /// <inheritdoc />
        public sealed override bool IgnoreCulling => true;

        public override bool AutoBounds => false;

        public new UITransform Transform => (UITransform)TransformProp;

        protected sealed override Transform TransformProp { get; set; }

        /// <summary>Can the UIObject be clicked?</summary>
        public virtual bool IsInteractable { get; protected set; }

        /// <summary>Called when the cursor enters the UIObject's bounds.</summary>
        public event EventHandler<MouseEventArgs> Selected;

        /// <summary>Called when the cursor leaves the UIObject's bounds.</summary>
        public event EventHandler<MouseEventArgs> Deselected;

        /// <summary>Called when the UIObject is clicked.</summary>
        public event EventHandler<MouseEventArgs> Clicked;

        /// <summary>Called when the mouse is clicked, but away from the UIObject.</summary>
        public event EventHandler<MouseEventArgs> ClickedAway;

        /// <summary>Called when the mouse is released while over the UIObject.</summary>
        public event EventHandler<MouseEventArgs> ClickReleased;

        /// <summary>Called when the UIObject is dragged.</summary>
        public event EventHandler<MouseDragEventArgs> Dragged;

        public Transform Parent {
            get => Transform.Parent;
            set {
                Transform.SetParent(value);
                RootUIObject = (UIObject)Transform.Root.GameObject;
            }
        }

        private Color color;
        public Color SpriteColor {
            get => color;
            set {
                color = value;
                for (int i = 0; i < Sprites.Count; i++) {
                    Sprites.Array[i].Color = color;
                }
            }
        }

        public bool Interactable {
            get => IsInteractable;
            set {
                IsInteractable = value;
                for (int i = 0; i < Transform.Children.Count; i++) {
                    UIObject o = (UIObject)Transform.Children[i].GameObject;
                    o.IsInteractable = value;
                }
            }
        }

        public Alignment Align {
            get => Transform.Align;
            set => Transform.Align = value;
        }

        public Rectangle? ScissorRect;
        public Rectangle? ParentScissorRect;

        protected bool MouseIn;
        protected bool DragListen;
        private MouseButtons dragOldMouseButton; 

        private Vector2 dragOldMousePoint;
        private Vector2 dragOldPos;

        protected bool MouseOver;

        // Caching...
        private List<UIComponent> tmpUIComponents = new List<UIComponent>();

        internal void SetPriority(int priority)
        {
            if (Transform.Parent != null) {
                Priority = priority + 1;
            } else {
                Priority = priority;
            }
            for (int i = 0; i < Transform.Children.Count; i++) {
                Transform child = Transform.Children.Array[i];
                if (child.GameObject is UIObject uiObj) {
                    uiObj.SetPriority(Priority);
                } else {
                    StepIntoChild(child);
                }
            }
            for (int i = 0; i < Sprites.Count; i++) {
                Sprites.Array[i].LayerDepth = Priority * 0.00001f;
            }
        }

        private void StepIntoChild(Transform child)
        {
            for (int i = 0; i < child.Children.Count; i++) {
                Transform c = child.Children.Array[i];
                if (c.GameObject is UIObject uiObj) {
                    uiObj.SetPriority(Priority);
                } else {
                    StepIntoChild(c);
                }
            }
        }

        internal void RefreshPriority()
        {
            SetPriority(Priority);
        }

        internal void UpdateChildScissorRect(Rectangle? sr)
        {
            if (ScissorRect.HasValue) {
                sr = ScissorRect.Value;
            }
            if (sr.HasValue) {
                ParentScissorRect = sr;
            }
            QuickList<Transform> children = Transform.Children;
            for (int i = 0; i < children.Count; i++) {
                UIObject child = (UIObject)children.Array[i].GameObject;
                if (child.Enabled) {
                    child.UpdateChildScissorRect(sr);
                }
            }
        }

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            if (DestroyOnLoad && !IsRootUIMenu) {
                DestroyOnLoad = false;
            }
            base.OnInitialize();
            for (int i = 0; i < Transform.Children.Count; i++) {
                UIObject o = (UIObject)Transform.Children.Array[i].GameObject;
                o.IsInteractable = IsInteractable;
            }
            if (IsRootUIMenu) {
                UIManager.RegisterMenu(RootUIName, this, RootUIPriority);
            }
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            if (Destroyed)
                throw new InvalidOperationException("GameObject is already destroyed.");

            RootUIObject = null;
            if (IsRootUIMenu) {
                UIManager.UnregisterMenu(RootUIName);
            }
            if (UIManager.AssumedScrollWheelControl == this) {
                UIManager.AssumedScrollWheelControl = null;
            }
            UIManager.RemoveAssumedKeyboardControl(this);
            UIManager.RemoveGizmo(this);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (Selected != null) {
                foreach (Delegate d in Selected.GetInvocationList()) {
                    Selected -= d as EventHandler<MouseEventArgs>;
                }
            }
            if (Deselected != null) {
                foreach (Delegate d in Deselected.GetInvocationList()) {
                    Deselected -= d as EventHandler<MouseEventArgs>;
                }
            }
            if (Clicked != null) {
                foreach (Delegate d in Clicked.GetInvocationList()) {
                    Clicked -= d as EventHandler<MouseEventArgs>;
                }
            }
            if (ClickedAway != null) {
                foreach (Delegate d in ClickedAway.GetInvocationList()) {
                    ClickedAway -= d as EventHandler<MouseEventArgs>;
                }
            }
            if (ClickReleased != null) {
                foreach (Delegate d in ClickReleased.GetInvocationList()) {
                    ClickReleased -= d as EventHandler<MouseEventArgs>;
                }
            }
            if (Dragged != null) {
                foreach (Delegate d in Dragged.GetInvocationList()) {
                    Dragged -= d as EventHandler<MouseDragEventArgs>;
                }
            }
        }

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (DragListen) {
                if (InputManager.MouseMoved) {
                    OnDrag(new MouseEventArgs(dragOldMouseButton, Screen.MousePoint));
                } else if (!InputManager.MouseDown) {
                    DragListen = false;
                }
            }
            if (ScissorRect.HasValue) {
                UpdateChildScissorRect(ScissorRect);
            }
        }

        /// <inheritdoc />
        protected override void OnDisable(bool isRoot = false)
        {
            base.OnDisable(isRoot);
            OnDeselected(new MouseEventArgs(MouseButtons.None, Screen.MousePoint));
            RootUIObject?.RefreshPriority();
        }

        /// <inheritdoc />
        protected override void OnEnable(bool enableAllChildren = false)
        {
            base.OnEnable(enableAllChildren);
            RootUIObject.RefreshPriority();
        }

        public virtual void OnSelected(MouseEventArgs mouseEventArgs)
        {
            if (Destroyed)
                throw new InvalidOperationException("Attempted to access destroyed GameObject.");
            if (MouseOver)
                return;

            MouseOver = true;
            tmpUIComponents.Clear();
            GetComponents(tmpUIComponents);
            for (int i = 0; i < tmpUIComponents.Count; i++) {
                tmpUIComponents[i].OnSelected();
            }
            MouseIn = true;
            Selected?.Invoke(this, mouseEventArgs);
        }

        public virtual void OnDeselected(MouseEventArgs mouseEventArgs)
        {
            if (Destroyed)
                throw new InvalidOperationException("Attempted to access destroyed GameObject.");
            if (!MouseOver)
                return;

            MouseOver = false;
            tmpUIComponents.Clear();
            GetComponents(tmpUIComponents);
            for (int i = 0; i < tmpUIComponents.Count; i++) {
                tmpUIComponents[i].OnDeselected();
            }
            MouseIn = false;
            Deselected?.Invoke(this, mouseEventArgs);
        }

        public virtual void OnClick(MouseEventArgs mouseEventArgs)
        {
            if (Destroyed)
                throw new InvalidOperationException("Attempted to access destroyed GameObject.");

            tmpUIComponents.Clear();
            GetComponents(tmpUIComponents);
            for (int i = 0; i < tmpUIComponents.Count; i++) {
                tmpUIComponents[i].OnClick();
            }
            DragListen = true;
            dragOldMouseButton = mouseEventArgs.MouseButton;
            dragOldMousePoint = InputManager.MouseState.Position.ToNumericsVector2();
            dragOldPos = Transform.Position;
            Clicked?.Invoke(this, mouseEventArgs);
        }

        public virtual void OnClickAway(MouseEventArgs mouseEventArgs)
        {
            if (Destroyed)
                throw new InvalidOperationException("Attempted to access destroyed GameObject.");

            tmpUIComponents.Clear();
            GetComponents(tmpUIComponents);
            for (int i = 0; i < tmpUIComponents.Count; i++) {
                tmpUIComponents[i].OnClickAway();
            }
            DragListen = false;
            ClickedAway?.Invoke(this, mouseEventArgs);
        }

        public virtual void OnDrag(MouseEventArgs mouseEventArgs)
        {
            if (Destroyed)
                throw new InvalidOperationException("Attempted to access destroyed GameObject.");

            tmpUIComponents.Clear();
            GetComponents(tmpUIComponents);
            for (int i = 0; i < tmpUIComponents.Count; i++) {
                tmpUIComponents[i].OnDrag();
            }
            MouseDragEventArgs mouseDrag = new MouseDragEventArgs(mouseEventArgs.MousePoint - dragOldMousePoint, dragOldPos);
            Dragged?.Invoke(this, mouseDrag);
        }

        public virtual void OnClickRelease(MouseEventArgs mouseEventArgs)
        {
            if (Destroyed)
                throw new InvalidOperationException("Attempted to access destroyed GameObject.");

            tmpUIComponents.Clear();
            GetComponents(tmpUIComponents);
            for (int i = 0; i < tmpUIComponents.Count; i++) {
                tmpUIComponents[i].OnClickRelease();
            }
            DragListen = false;
            ClickReleased?.Invoke(this, mouseEventArgs);
        }

        public void SetSizeRatio(float x, float y)
        {
            if (Destroyed)
                throw new InvalidOperationException("Attempted to access destroyed GameObject.");

            Transform.SetSizeRatio(x, y);
        }

        public void SetSizeRatio(float x)
        {
            if (Destroyed)
                throw new InvalidOperationException("Attempted to access destroyed GameObject.");

            Transform.SetSizeRatio(x);
        }

        /// <inheritdoc />
        public UIObject(Vector2 pos, Point? size = null, bool noEngineCallback = false) : base(pos, 0f, Vector2.One)
        {
            Point s;
            if (size.HasValue && size.Value != Point.Zero) {
                s = size.Value;
            } else if(Sprites.Count > 0) {
                int x = 0, y = 0;
                for (int i = 0; i < Sprites.Count; i++) {
                    if (Sprites.Array[i].Size.X > x) {
                        x = Sprites.Array[i].Size.X;
                    }
                    if (Sprites.Array[i].Size.Y > y) {
                        y = Sprites.Array[i].Size.Y;
                    }
                }
                s = new Point(x, y);
                if (s == Point.Zero) {
                    s = new Point((int)Screen.ViewSize.X, (int)Screen.ViewSize.Y);
                }
            } else {
                s = new Point((int)Screen.ViewSize.X, (int)Screen.ViewSize.Y);
            }
            TransformProp = new UITransform(pos, s, 0f, this);
            RootUIObject = (UIObject)Transform.Root.GameObject;
            GameEngine.GameObjectConstructorCallback(this);
        }
    }

}
