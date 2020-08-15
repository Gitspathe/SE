﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    public abstract class SpriteBase : Component, IRenderable
    {
        // NOTE: Private protected fields are for increased performance. Access the properties instead
        // when not dealing with the core rendering system.

        public Rectangle AABB => bounds;
        public PartitionTile<IRenderable> CurrentPartitionTile { get; set; }

        public override int Queue => 100;

        public BlendMode BlendMode {
            get => Material.BlendMode;
            set => Material.BlendMode = value;
        }

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
        public virtual Rectangle Bounds {
            get => bounds; 
            set => bounds = value;
        }

        private protected Vector2 origin;
        public virtual Vector2 Origin {
            get => origin;
            set {
                origin = value;
                Owner?.RecalculateBounds();
            }
        }

        private protected Color color;
        public virtual Color Color {
            get => color; 
            set => color = value;
        }

        private protected float layerDepth;
        public virtual float LayerDepth {
            get => layerDepth; 
            set => layerDepth = value;
        }

        // TODO: Move this out of SpriteBase. Or at least don't include it in UISlicedSprite since it's useless there.
        public virtual SpriteTexture SpriteTexture {
            set {
                if (!Screen.IsFullHeadless && value.Texture == null)
                    throw new NullReferenceException("The specified SpriteTexture has no Texture2D asset. Ensure that the asset exists, and that it's being set.");

                Material.Texture = value.Texture;
                TextureSourceRectangle = value.SourceRectangle;
            }
        }

        protected Rectangle TextureSourceRectangle;

        public Material Material { get; }

        /// <summary>Cached owner transform. DO NOT MODIFY!</summary>
        protected Transform ownerTransform;

        public SpriteBase()
        {
            Material = new Material(this);
        }

        public abstract void RecalculateBounds();

        internal override void OnInitializeInternal()
        {
            base.OnInitializeInternal();
            if (Enabled && !Owner.Sprites.Contains(this))
                Owner.AddSprite(this);

            ownerTransform = Owner.Transform;
            Material.RegenerateDrawCall();
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
        
        public void InsertIntoPartition()
        {
            // UISprites ignore partitioning.
            if(Material.UISprite != null)
                return;

            SpatialPartitionManager.Insert(this);
            Material.RenderableTypeInfo.Lit?.Shadow?.InsertIntoPartition();
        }

        public void RemoveFromPartition()
        {
            SpatialPartitionManager.Remove(this);
            Material.RenderableTypeInfo.Lit?.Shadow?.InsertIntoPartition();
        }
    }

}
