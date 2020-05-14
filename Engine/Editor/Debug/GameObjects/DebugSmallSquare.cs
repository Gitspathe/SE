using Microsoft.Xna.Framework;
using SE.Common;
using SE.Components;
using SE.Core;
using SE.Rendering;
using Vector2 = System.Numerics.Vector2;

namespace SE.Editor.Debug.GameObjects
{

    internal class DebugSmallSquare : GameObject
    {

        /// <inheritdoc />
        public override bool IsDynamic => false;

        /// <inheritdoc />
        protected sealed override void OnInitialize()
        {
            base.OnInitialize();
            Sprite sprite = new Sprite(AssetManager.Get<SpriteTexture>(this, "debugsmallsquare"), Color.Orange, 0.1f);
            AddComponent(sprite);
        }

        /// <inheritdoc />
        public DebugSmallSquare(Vector2 pos, float rot, Vector2 scale, bool noEngineCallback = false) : base(pos, rot, scale)
        {
            OnInitialize();
        }

    }

}
