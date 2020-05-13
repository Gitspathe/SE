﻿using DeeZ.Core;
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
    internal class WallDown : GameObject
    {

        /// <inheritdoc />
        public override bool IsDynamic => false;

        public override bool DestroyOnLoad => false; // Tiles are destroyed anyway.

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Sprite sprite = GetComponent<Sprite>();
            PhysicsObject physicsObj = GetComponent<PhysicsObject>();
            sprite.spriteTexture = AssetManager.Get<SpriteTexture>(this, "wall_down");
            sprite.LayerDepth = 0.1f;
            sprite.Origin = new Vector2(0, -48);
            sprite.RenderType = RenderDataType.Basic;
            sprite.ShadowType = ShadowCasterType.Map;
            physicsObj.Body = Physics.CreateRectangle(new Rectangle(0, -48, 64, 16));
        }

        /// <inheritdoc />
        public WallDown(Vector2 pos, float rot, Vector2 scale) : base(pos, rot, scale) { }

    }

}