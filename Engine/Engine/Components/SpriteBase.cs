using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Attributes;
using SE.Common;
using SE.Core;
using SE.Rendering;
using SE.World.Partitioning;
using SE.Core.Extensions;
using Vector2 = System.Numerics.Vector2;
// ReSharper disable InconsistentNaming

namespace SE.Components
{

    [ExecuteInEditor]
    public abstract class SpriteBase : Component, IRenderable, IPartitionObjectExtended
    {
        // NOTE: Private protected fields are for increased performance. Access the properties instead
        // when not dealing with the core rendering system.

        public Vector2 PartitionPosition => Owner.Transform.GlobalPositionInternal;
        public Type PartitionObjectType => typeof(IRenderable);
        public IRenderable PartitionObject => this;
        public PartitionTile CurrentPartitionTile { get; set; }

        public override int Queue => 100;

        internal BlendMode blendMode;
        public BlendMode BlendMode {
            get => blendMode;
            set => blendMode = value;
        }

        public virtual Point Offset {
            get {
                if (Owner == null)
                    return unscaledOffset;

                Vector2 scale = Owner.Transform.GlobalScale;
                return new Point((int)(unscaledOffset.X * scale.X), (int)(unscaledOffset.Y * scale.Y));
            }
            set {
                if (Owner == null) {
                    unscaledOffset = value;
                    return;
                }

                Vector2 scale = Owner.Transform.GlobalScale;
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

                Vector2 scale = Owner.Transform.GlobalScale;
                return new Point((int)(unscaledSize.X * scale.X), (int)(unscaledSize.Y * scale.Y));
            }
            set {
                if (Owner == null) {
                    unscaledSize = value;
                    return;
                }

                Vector2 scale = Owner.Transform.GlobalScale;
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
        [NoSerialize] public virtual Rectangle Bounds {
            get => bounds; 
            set => bounds = value;
        }

        private protected Vector2 origin;
        public virtual Vector2 Origin {
            get => origin;
            set {
                origin = value;
                if (Owner != null && !Owner.IsDynamic) {
                    Owner.RecalculateBounds();
                }
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

        private protected Effect shader;
        public virtual Effect Shader {
            get => shader;
            set {
                shader = value;
                RegenerateDrawCall();
            }
        }

        private protected SpriteTexture spriteTexture;
        public virtual SpriteTexture SpriteTexture {
            get => spriteTexture;
            set {
                if (!Screen.IsFullHeadless && value.Texture == null)
                    throw new NullReferenceException("The specified SpriteTexture has no Texture2D asset. Ensure that the asset exists, and that it's being set.");

                spriteTexture = value;
                RegenerateDrawCall();
            }
        }

        private protected int drawCallID = -1;
        public int DrawCallID { 
            get => drawCallID;
            private set => drawCallID = value;
        }

        /// <summary>Cached owner transform. DO NOT MODIFY!</summary>
        protected Transform ownerTransform;

        public abstract void RecalculateBounds();

        public void InsertedIntoPartition(PartitionTile tile)
        {
            if (this is ILit lit && lit.Shadow != null) {
                SpatialPartitionManager.Insert(lit.Shadow);
            }
        }

        public void RemovedFromPartition(PartitionTile tile)
        {
            if (this is ILit lit) {
                lit.Shadow?.CurrentPartitionTile?.Remove(lit.Shadow);
            }
        }

        internal override void OnInitializeInternal()
        {
            base.OnInitializeInternal();
            if (Enabled && !Owner.Sprites.Contains(this))
                Owner.AddSprite(this);

            ownerTransform = Owner.Transform;
            RegenerateDrawCall();
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

        public void RegenerateDrawCall()
        {
            if(spriteTexture.Texture != null)
                DrawCallID = DrawCallDatabase.TryGetID(new DrawCall(spriteTexture.Texture, Shader));
        }

        public abstract void Render(Camera2D camera, Space space);
    }

}
