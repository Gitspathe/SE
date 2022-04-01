using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement;
using SE.Common;
using SE.Core;
using SE.Rendering;
using SE.World.Partitioning;

using Vector2 = System.Numerics.Vector2;

namespace SE.NeoRenderer
{
    public sealed class SpriteComponent : Component, IPartitionObject<SpriteComponent>
    {
        public override int Queue => 100;

        public PartitionTile<SpriteComponent> CurrentPartitionTile { get; set; }
        public Rectangle AABB => bounds;

        public void InsertIntoPartition() => SpatialPartitionManager.Insert(this);
        public void RemoveFromPartition() => SpatialPartitionManager.Remove(this);

        public Point Offset {
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

        private Point unscaledOffset;
        public Point UnscaledOffset {
            get => unscaledOffset;
            set {
                unscaledOffset = value;
                RecalculateBounds();
            }
        }

        public Point Size {
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

        private Point unscaledSize;
        public Point UnscaledSize {
            get => unscaledSize;
            set {
                unscaledSize = value;
                RecalculateBounds();
            }
        }

        private Rectangle bounds;
        public Rectangle Bounds {
            get => bounds;
            set => bounds = value;
        }

        private Vector2 origin;
        public Vector2 Origin {
            get => origin;
            set {
                origin = value;
                Owner?.RecalculateBounds();
            }
        }

        private Color color;
        public Color Color {
            get => color;
            set => color = value;
        }

        private float layerDepth;
        public float LayerDepth {
            get => layerDepth;
            set => layerDepth = value;
        }

        private SpriteMaterial material;
        public SpriteMaterial Material {
            get => material;
            set {
                if(material == value)
                    return;

                if (material != null) {
                    SpriteMaterialHandler.DecrementMaterialRefCount(material);
                }

                material = value;
                SpriteMaterialHandler.IncrementMaterialRefCount(material);
            }
        }

        /// <summary>Cached owner transform. DO NOT MODIFY!</summary>
        internal Transform OwnerTransform;

        internal void AddToBatcher(int index, SpriteBatcher batcher)
        {
            batcher.Add(
                index,
                OwnerTransform.GlobalPositionInternal, 
                new Rectangle(0, 0, 64, 64), // Temporary.
                color, 
                OwnerTransform.GlobalRotationInternal, 
                origin,
                OwnerTransform.GlobalScaleInternal,
                layerDepth);
        }

        protected override void OnOwnerBoundsUpdated()
        {
            RecalculateBounds();
        }

        public void RecalculateBounds()
        {
            Point offset = Offset;
            Point size = Size;
            bounds.X = (int)(OwnerTransform.GlobalPositionInternal.X + (offset.X - Origin.X));
            bounds.Y = (int)(OwnerTransform.GlobalPositionInternal.Y + (offset.Y - Origin.Y));
            bounds.Width = size.X;
            bounds.Height = size.Y;
        }

        internal override void OnInitializeInternal()
        {
            base.OnInitializeInternal();
            OwnerTransform = Owner.Transform;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }
    }
}
