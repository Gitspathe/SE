using DeeZ.Core;
using DeeZ.Engine.Attributes;
using DeeZ.Engine.Common;
using DeeZ.Engine.Components;
using DeeZ.Engine.Lighting;
using DeeZ.Engine.Rendering;
using DeeZ.Engine.Rendering.Data;
using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;

namespace DeeZEngine_Demos.GameObjects
{

    [Components(typeof(Sprite), typeof(PhysicsObject))]
    internal class WallRight : GameObject
    {

        /// <inheritdoc />
        public override bool IsDynamic => false;

        public override bool DestroyOnLoad => false; // Tiles are destroyed anyway.

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Sprite sprite = GetComponent<Sprite>();
            PhysicsObject physicsObj = GetComponent<PhysicsObject>();
            sprite.spriteTexture = AssetManager.Get<SpriteTexture>(this, "wall_right");
            sprite.LayerDepth = 0.1f;
            sprite.Origin = new Vector2(-48, 0);
            sprite.RenderType = RenderDataType.Basic;
            sprite.ShadowType = ShadowCasterType.Map;
            physicsObj.Body = Physics.CreateRectangle(new Rectangle(-48, 0, 16, 64));
        }

        /// <inheritdoc />
        public WallRight(Vector2 pos, float rot, Vector2 scale) : base(pos, rot, scale) { }

    }

}
