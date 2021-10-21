using Microsoft.Xna.Framework;
using SE.Attributes;
using SE.Common;
using SE.Components;
using SE.Components.Lighting;
using SE.Core;
using SE.Rendering;
using Vector2 = System.Numerics.Vector2;

namespace SE.Editor.Debug.GameObjects
{

    [Components(typeof(Sprite), typeof(LightComponent))]
    public class Cursor : GameObject
    {
        public override bool DestroyOnLoad => false;

        /// <inheritdoc />
        public override bool IsDynamic => true;

        private LightComponent p;

        /// <inheritdoc />
        protected sealed override void OnInitialize()
        {
            base.OnInitialize();
            Sprite sprite = GetComponent<Sprite>();
            sprite.SpriteTextureAsset = AssetManager.GetAsset<SpriteTexture>("leveleditorcursor");
            sprite.Color = Color.Red;
            sprite.LayerDepth = 1.0f;
            sprite.Material.IgnoreLighting = true;
            sprite.Material.BlendMode = BlendMode.Transparent;
            p = GetComponent<LightComponent>();
            p.Light.Size = new Vector2(64.0f * 10);
            p.Light.CastsShadows = true;
            p.Light.Offset = new Vector2(-9999, -9999);
        }

        /// <inheritdoc />
        public Cursor(Vector2 pos, float rot, Vector2 scale) : base(pos, rot, scale) { }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            Vector2? vec = GameEngine.EditorCamera?.MouseToWorldPoint();
            p.Light.Offset = vec.HasValue
                ? vec.Value - Transform.GlobalPosition
                : new Vector2(-999999.9f, -999999.9f);
        }

    }

}