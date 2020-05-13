using DeeZ.Core;
using DeeZ.Engine.Attributes;
using DeeZ.Engine.Common;
using DeeZ.Engine.Components;
using DeeZ.Engine.Rendering;
using DeeZ.Engine.Rendering.Data;
using Microsoft.Xna.Framework;
using Random = DeeZ.Engine.Random;
using Vector2 = System.Numerics.Vector2;

namespace DeeZEngine_Demos.GameObjects
{

    [Components(typeof(Sprite))]
    internal class Floor : GameObject
    {
        /// <inheritdoc />
        public override bool IsDynamic => false;

        public override bool DestroyOnLoad => false; // Tiles are destroyed anyway.

        /// <inheritdoc />
        protected sealed override void OnInitialize()
        {
            base.OnInitialize();
            Sprite sprite = GetComponent<Sprite>();
            sprite.spriteTexture = AssetManager.Get<SpriteTexture>(this, "floor");
            sprite.RenderType = RenderDataType.Basic;
            //base.Initialize();
        }

        /// <inheritdoc />
        public Floor(Vector2 pos, float rot, Vector2 scale) : base(pos, rot, scale) { }
    }

}
