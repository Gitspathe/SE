﻿using Microsoft.Xna.Framework;
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
    internal class WallDown : GameObject
    {

        /// <inheritdoc />
        public override bool IsDynamic => false;

        public override bool DestroyOnLoad => false; // Tiles are destroyed anyway.

        private static Material tmpMaterial = new Material();

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Sprite sprite = GetComponent<Sprite>();
            PhysicsObject physicsObj = GetComponent<PhysicsObject>();
            sprite.Material = tmpMaterial;
            sprite.SpriteTextureAsset = AssetManager.GetAsset<SpriteTexture>("wall_down");
            sprite.LayerDepth = 0.1f;
            sprite.Origin = new Vector2(0, -48);
            sprite.ShadowType = ShadowCasterType.Map;
            physicsObj.Body = Physics.CreateRectangle(new Rectangle(0, -48, 64, 16));
        }

        /// <inheritdoc />
        public WallDown(Vector2 pos, float rot, Vector2 scale) : base(pos, rot, scale) { }

    }

}
