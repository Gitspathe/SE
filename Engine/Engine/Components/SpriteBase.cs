using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement;
using SE.Attributes;
using SE.Common;
using SE.Core;
using SE.Rendering;
using SE.World.Partitioning;
using SE.Core.Extensions;
using Vector2 = System.Numerics.Vector2;
using SE.Lighting;
using SE.Components.UI;
// ReSharper disable InconsistentNaming

namespace SE.Components
{

    [ExecuteInEditor]
    public abstract class SpriteBase : Component
    {
        // NOTE: Private protected fields are for increased performance. Access the properties instead
        // when not dealing with the core rendering system.

        public override int Queue => 100;

        public virtual Point Offset {
            get {
                if (Owner == null)
                    return unscaledOffset;

                Vector2 scale = Owner.Transform.GlobalScaleInternal;
                return new Point((int)(unscaledOffset.X * scale.X), (int)(unscaledOffset.Y * scale.Y));
            }
            set {
                if (Owner == null) {
                    unscaledOffset = value;
                    return;
                }

                Vector2 scale = Owner.Transform.GlobalScaleInternal;
                unscaledOffset = new Point((int)(value.X * scale.X), (int)(value.Y * scale.Y));
                RecalculateBounds();
            }
        }

        private protected Point unscaledOffset;
        public Point UnscaledOffset {
            get => unscaledOffset;
            set {
                unscaledOffset = value;
                RecalculateBounds();
            }
        }

        public virtual Point Size {
            get {
                if (Owner == null)
                    return unscaledSize;

                Vector2 scale = Owner.Transform.GlobalScaleInternal;
                return new Point((int)(unscaledSize.X * scale.X), (int)(unscaledSize.Y * scale.Y));
            }
            set {
                if (Owner == null) {
                    unscaledSize = value;
                    return;
                }

                Vector2 scale = Owner.Transform.GlobalScaleInternal;
                UnscaledSize = new Point((int)(value.X * scale.X), (int)(value.Y * scale.Y));
                RecalculateBounds();
            }
        }

        private protected Point unscaledSize;
        public Point UnscaledSize {
            get => unscaledSize;
            set {
                unscaledSize = value;
                RecalculateBounds();
            }
        }

        private protected Rectangle bounds;
        public Rectangle Bounds {
            get => bounds; 
            set => bounds = value;
        }

        private protected Vector2 origin;
        public Vector2 Origin {
            get => origin;
            set {
                origin = value;
                Owner?.RecalculateBounds();
            }
        }

        private protected Color color;
        public Color Color {
            get => color; 
            set => color = value;
        }

        private protected float layerDepth;
        public float LayerDepth {
            get => layerDepth; 
            set => layerDepth = value;
        }

        /// <summary>Cached owner transform. DO NOT MODIFY!</summary>
        protected Transform ownerTransform;

        public abstract void RecalculateBounds();

        internal override void OnInitializeInternal()
        {
            base.OnInitializeInternal();
            if (Enabled && !Owner.Sprites.Contains(this))
                Owner.AddSprite(this);

            ownerTransform = Owner.Transform;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Owner?.RemoveSprite(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Owner?.RemoveSprite(this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Owner != null && !Owner.Sprites.Contains(this)) {
                Owner?.AddSprite(this);
            }
        }

        public abstract void Render(Camera2D camera, Space space);
    }

}
