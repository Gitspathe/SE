using DeeZ.Core;
using DeeZ.Engine.Attributes;
using DeeZ.Engine.Common;
using DeeZ.Engine.Components;
using DeeZ.Engine.Rendering;
using DeeZ.Engine.Rendering.Data;
using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;

namespace DeeZEngine_Demos.GameObjects
{
    [Components(typeof(Sprite))]
    public class UnloadTestObj : GameObject
    {
        public override bool IsDynamic => false;

        protected override void OnInitialize()
        {
            Sprite sprite = GetComponent<Sprite>();
            sprite.spriteTexture = AssetManager.Get<SpriteTexture>(this, "unload_test");
            sprite.Color = Color.Purple;
            sprite.LayerDepth = 1.0f;
            sprite.RenderType = RenderDataType.Basic;
            base.OnInitialize();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public UnloadTestObj(Vector2 pos, float rot, Vector2 scale) : base(pos, rot, scale) { }

    }
}
