using System;
using System.Numerics;
using SE.Common;
using SE.Components;
using SE.Utility;
using Point = Microsoft.Xna.Framework.Point;

namespace SE.UI
{
    public class UITransform : Transform
    {
        private Alignment align;
        public Alignment Align {
            get => align;
            set {
                align = value;
                UpdateTransformation();
            }
        }

        private Point baseSize;
        private Point size;
        public Point Size {
            get => size;
            set {
                size = value;
                Scale = new Vector2((float)size.X / baseSize.X, (float)size.Y / baseSize.Y);
                GameObject.RecalculateBoundsInternal();
            }
        }

        public new UIObject GameObject => (UIObject) GameObjectProp;

        /// <summary>
        /// Parent size. Returns current Transform size if there is no parent.
        /// </summary>
        protected Vector2 ParentSize {
            get {
                UITransform parentTransform = (UITransform)Parent;
                RectangleF parentBounds = parentTransform?.GameObject.Bounds ?? GameObject.Bounds;
                return new Vector2(parentBounds.Width, parentBounds.Height);
            }
        }

        public void SetSizeRatio(float x, float y)
        {
            Size = new Point((int)(Size.X * x), (int)(Size.Y * y));
        }

        public void SetSizeRatio(float x)
        {
            Size = new Point((int)(Size.X * x), (int)(Size.Y * x));
        }

        protected override Matrix4x4 UpdateTransformationMatrix()
        {
            if (Align > 0)
                UpdateSpriteOriginPoints();

            return base.UpdateTransformationMatrix();
        }

        private void UpdateSpriteOriginPoints()
        {
            Vector2 size;
            Vector2 parentSize = ParentSize;
            Vector2 origin = Vector2.Zero;
            if ((align & Alignment.Center) == Alignment.Center) {
                size = new Vector2(GameObject.Bounds.Width, GameObject.Bounds.Height);
                origin += new Vector2(-(parentSize.X / 2) + (size.X / 2), -(parentSize.Y / 2) + (size.Y / 2));
            }
            if ((align & Alignment.Up) == Alignment.Up) {
                size = new Vector2(GameObject.Bounds.Width, GameObject.Bounds.Height);
                origin += new Vector2(-(parentSize.X / 2) + (size.X / 2), 0);
            }
            if ((align & Alignment.Right) == Alignment.Right) {
                size = new Vector2(GameObject.Bounds.Width, GameObject.Bounds.Height);
                origin += new Vector2(-(parentSize.X) + size.X, -(parentSize.Y / 2) + (size.Y / 2));
            }
            if ((align & Alignment.Down) == Alignment.Down) {
                size = new Vector2(GameObject.Bounds.Width, GameObject.Bounds.Height);
                origin += new Vector2(-(parentSize.X / 2) + (size.X / 2), -(parentSize.Y) + (size.Y / 2));
            }
            if ((align & Alignment.Left) == Alignment.Left) {
                size = new Vector2(GameObject.Bounds.Width, GameObject.Bounds.Height);
                origin += new Vector2(0, -(parentSize.Y / 2) + (size.Y / 2));
            }
            for (int i = 0; i < GameObject.Sprites.Count; i++) {
                SpriteBase s = GameObject.Sprites.Array[i];
                s.Origin = origin;
            }
        }

        public UITransform(Vector2 position, Point size, float rotation = 0, GameObject ownerGameObject = null)
            : base(new Vector3(position.X, position.Y, 0.0f), Vector2.One, rotation, ownerGameObject)
        {
            baseSize = size;
            this.size = size;
        }

        public new static UITransform Empty => new UITransform(Vector2.Zero, Point.Zero);

    }

    [Flags]
    public enum Alignment
    {
        None = 0,
        Center = 1,
        Up = 2,
        Right = 4,
        Down = 8,
        Left = 16
    }
}
