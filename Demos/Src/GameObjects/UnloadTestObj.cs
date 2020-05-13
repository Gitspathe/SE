using Microsoft.Xna.Framework;
using SE.Attributes;
using SE.Common;
using SE.Components;
using SE.Core;
using SE.Rendering;
using Vector2 = System.Numerics.Vector2;

namespace SEDemos.GameObjects
{
    [Components(typeof(Sprite))]
    public class UnloadTestObj : GameObject
    {
        public override bool IsDynamic => false;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Sprite sprite = GetComponent<Sprite>();
            sprite.SpriteTexture = AssetManager.Get<SpriteTexture>(this, "unload_test");
            sprite.Color = Color.Purple;
            sprite.LayerDepth = 1.0f;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public UnloadTestObj(Vector2 pos, float rot, Vector2 scale) : base(pos, rot, scale) { }

    }
}
