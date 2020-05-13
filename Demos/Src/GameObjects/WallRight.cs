using Microsoft.Xna.Framework;
using SE.Attributes;
using SE.Common;
using SE.Components;
using SE.Core;
using SE.Lighting;
using SE.Rendering;
using Vector2 = System.Numerics.Vector2;

namespace SEDemos.GameObjects
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
            sprite.SpriteTexture = AssetManager.Get<SpriteTexture>(this, "wall_right");
            sprite.LayerDepth = 0.1f;
            sprite.Origin = new Vector2(-48, 0);
            sprite.ShadowType = ShadowCasterType.Map;
            physicsObj.Body = Physics.CreateRectangle(new Rectangle(-48, 0, 16, 64));
        }

        /// <inheritdoc />
        public WallRight(Vector2 pos, float rot, Vector2 scale) : base(pos, rot, scale) { }

    }

}
