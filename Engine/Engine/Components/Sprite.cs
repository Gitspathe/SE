using Microsoft.Xna.Framework;
using SE.AssetManagement;
using SE.Lighting;
using SE.Rendering;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
// ReSharper disable UnusedMember.Global

namespace SE.Components
{
    /// <summary>
    /// Component used for rendering a GameObject.
    /// </summary>
    public sealed class Sprite : SpriteBase, ILit
    {
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
                            TextureSourceRectangle.Width,
                            TextureSourceRectangle.Height);
                    } else {
                        Shadow.Bounds = new Rectangle(
                            0 - (int)Origin.X,
                            0 - (int)Origin.Y,
                            TextureSourceRectangle.Width - (int)Origin.X,
                            TextureSourceRectangle.Height - (int)Origin.Y);
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

        public ShadowCaster Shadow { get; set; }

        public override void RecalculateBounds()
        {
            Rectangle bounds = Bounds;
            Point offset = Offset;
            Point size = Size;
            bounds.X = (int)(Owner.Transform.GlobalPositionInternal.X + (offset.X - Origin.X));
            bounds.Y = (int)(Owner.Transform.GlobalPositionInternal.Y + (offset.Y - Origin.Y));
            bounds.Width = size.X;
            bounds.Height = size.Y;
            Bounds = bounds;
        }

        public override void Render(Camera2D camera, Space space)
        {
            Vector2 position = ownerTransform.GlobalPositionInternal;
            if (space == Space.Screen) {
                position += camera.Position;
            }

            Core.Rendering.SpriteBatch.Draw(
                Data.Material.Texture,
                position,
                TextureSourceRectangle,
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
    }
}
