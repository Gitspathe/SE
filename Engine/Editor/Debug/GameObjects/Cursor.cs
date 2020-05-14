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
            sprite.SpriteTexture = AssetManager.Get<SpriteTexture>(this, "leveleditorcursor");
            sprite.Color = Color.Red;
            sprite.LayerDepth = 0.2f;
            sprite.IgnoreLight = true;
            sprite.BlendMode = BlendMode.Transparent;
            p = GetComponent<LightComponent>();
            p.Light.Size = new Vector2(64.0f * 10);
            p.Light.CastsShadows = true;
            p.Light.Offset = new Vector2(-9999,-9999);
            AddComponent(sprite);
            AddComponent(p);
        }

        /// <inheritdoc />
        public Cursor(Vector2 pos, float rot, Vector2 scale) : base(pos, rot, scale) { }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            Vector2? vec = GameEngine.EditorCamera?.CameraPointToWorld(Screen.MousePoint);
            if (vec.HasValue) {
                p.Light.Offset =vec.Value - Transform.GlobalPositionInternal;
            } else {
                p.Light.Offset = new Vector2(-999999.9f, -999999.9f);
            }
        }

    }

}