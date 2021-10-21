using Microsoft.Xna.Framework;
using SE.AssetManagement;
using SE.Core;
using SE.Lighting;
using SE.Rendering;
using SE.World.Partitioning;
using System;
using Vector2 = System.Numerics.Vector2;
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace SE.Components
{
    /// <summary>
    /// Component used for rendering a GameObject.
    /// </summary>
    public sealed class Sprite : SpriteBase, ILit, IPartitionedRenderable
    {
        public Rectangle AABB => bounds;

        private ShadowCasterType shadowType = ShadowCasterType.None;
        public ShadowCasterType ShadowType {
            get => shadowType;
            set {
                Shadow?.Disable();
                shadowType = value;
                if (value != ShadowCasterType.None) {
                    if (Shadow == null) {
                        Shadow = new ShadowCaster();
                    }
                    if (value == ShadowCasterType.Map) {
                        Shadow.Bounds = new Rectangle(
                            0 - (int)Origin.X,
                            0 - (int)Origin.Y,
                            textureSourceRectangle.Width,
                            textureSourceRectangle.Height);
                    } else {
                        Shadow.Bounds = new Rectangle(
                            0 - (int)Origin.X,
                            0 - (int)Origin.Y,
                            textureSourceRectangle.Width - (int)Origin.X,
                            textureSourceRectangle.Height - (int)Origin.Y);
                        Shadow.CalculateHull(true);
                    }
                    Shadow.Position = Owner.Transform.GlobalPositionInternal;
                    Shadow.Scale = Owner.Transform.GlobalScaleInternal;
                    Shadow.Rotation = Owner.Transform.GlobalRotationInternal;
                    Shadow.ShadowCastType = shadowType;
                    Shadow.Enable();
                    UpdateShadow();
                }
            }
        }

        public Asset<SpriteTexture> SpriteTextureAsset {
            set {
                if (value == spriteTextureAssetInternal)
                    return;

                spriteTextureAssetInternal?.RemoveReference(AssetConsumer);
                spriteTextureAssetInternal = value;
                SpriteTexture = spriteTextureAssetInternal.Get(this);
                if (!Screen.IsFullHeadless && SpriteTexture.Texture == null)
                    throw new NullReferenceException("The specified SpriteTexture has no Texture2D asset. Ensure that the asset exists, and that it's being set.");

                Material.Texture = SpriteTexture.Texture;
                textureSourceRectangle = SpriteTexture.SourceRectangle;
            }
        }
        private Asset<SpriteTexture> spriteTextureAssetInternal;

        public SpriteTexture SpriteTexture { get; private set; }

        private Rectangle textureSourceRectangle;

        public Material Material {
            get => material;
            set {
                if (value == null)
                    value = Core.Rendering.BlankMaterial;
                if (value.Equals(material))
                    return;

                material = value;
            }
        }
        private Material material = Core.Rendering.BlankMaterial;

        public ShadowCaster Shadow { get; set; }

        public override void RecalculateBounds()
        {
            Point offset = Offset;
            Point size = Size;
            bounds.X = (int)(ownerTransform.GlobalPositionInternal.X + (offset.X - Origin.X));
            bounds.Y = (int)(ownerTransform.GlobalPositionInternal.Y + (offset.Y - Origin.Y));
            bounds.Width = size.X;
            bounds.Height = size.Y;
        }

        public override void Render(Camera2D camera, Space space)
        {
            Vector2 position = ownerTransform.GlobalPositionInternal;
            if (space == Space.Screen) {
                position += camera.Position;
            }

            Core.Rendering.SpriteBatch.DrawDeferred(
                Material.TextureInternal,
                position,
                textureSourceRectangle,
                color,
                ownerTransform.GlobalRotationInternal,
                origin,
                ownerTransform.GlobalScaleInternal,
                layerDepth);
        }

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            base.OnInitialize();
            ShadowType = ShadowType;
            Material.RegenerateDrawCall();
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            base.OnDisable();
            Shadow?.Disable();
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            Shadow?.Enable();
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Shadow?.Disable();
        }

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            base.OnUpdate();
            UpdateShadow();
        }

        public void UpdateShadow()
        {
            if (Shadow != null) {
                Shadow.Position = Owner.Transform.GlobalPositionInternal;
                Shadow.Scale = Owner.Transform.GlobalScaleInternal;
                Shadow.Rotation = Owner.Transform.GlobalRotationInternal;
            }
        }

        public void InsertIntoPartition()
        {
            SpatialPartitionManager.Insert(this);
            Shadow?.InsertIntoPartition();
        }

        public void RemoveFromPartition()
        {
            SpatialPartitionManager.Remove(this);
            Shadow?.RemoveFromPartition();
        }

        public uint PartitionLayer { get; }

        /// <summary>Creates a new sprite instance.</summary>
        /// <param name="spriteTexture">TextureSheet used.</param>
        /// <param name="color">Color used.</param>
        /// <param name="originPoint">Origin point for the sprite in pixels.</param>
        /// <param name="layerDepth">Render order of this sprite.</param>
        /// <param name="shadowType"></param>
        public Sprite(Asset<SpriteTexture> spriteTexture, Color color, Vector2 originPoint, float layerDepth = 0.0f, ShadowCasterType shadowType = ShadowCasterType.None)
        {
            SpriteTextureAsset = spriteTexture;
            Color = color;
            Origin = originPoint;
            LayerDepth = layerDepth;
            this.shadowType = shadowType;
            Size = new Point(SpriteTexture.SourceRectangle.Width, SpriteTexture.SourceRectangle.Height);
        }

        /// <summary>Creates a new basic RendererType sprite instance.</summary>
        /// <param name="spriteTexture">The SpriteTexture used to render this sprite.</param>
        /// <param name="color">Color used.</param>
        /// <param name="layerDepth">Render order of this sprite.</param>
        public Sprite(Asset<SpriteTexture> spriteTexture, Color color, float layerDepth = 0.0f, ShadowCasterType shadowType = ShadowCasterType.None)
            : this(spriteTexture, color, Vector2.Zero, layerDepth, shadowType) { }

        public Sprite() { }

        /// <summary>
        /// Returns an empty sprite.
        /// </summary>
        public static Sprite Empty => new Sprite(null, Color.White);

        public PartitionTile<IPartitionedRenderable> CurrentPartitionTile { get; set; }
    }
}
