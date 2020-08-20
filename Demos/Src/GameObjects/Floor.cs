using SE.Attributes;
using SE.Common;
using SE.Components;
using SE.Core;
using SE.Rendering;
using Vector2 = System.Numerics.Vector2;

namespace SEDemos.GameObjects
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
            sprite.SpriteTexture = AssetManager.Get<SpriteTexture>(this, "grass");
            //sprite.Enabled = false;
            //Disable();
            //base.Initialize();
        }

        /// <inheritdoc />
        public Floor(Vector2 pos, float rot, Vector2 scale) : base(pos, rot, scale) { }
    }

}
