using Microsoft.Xna.Framework.Graphics;
using SE.Attributes;
using SE.Common;
using SE.Components;
using SE.Core;
using SE.NeoRenderer;
using SE.Rendering;
using Vector2 = System.Numerics.Vector2;

namespace SEDemos.GameObjects
{

    [Components(typeof(SpriteComponent))]
    internal class Floor : GameObject
    {
        /// <inheritdoc />
        public override bool IsDynamic => false;

        public override bool DestroyOnLoad => false; // Tiles are destroyed anyway.

        private static SpriteMaterial tmpMaterial;

        /// <inheritdoc />
        protected sealed override void OnInitialize()
        {
            base.OnInitialize();
            if (tmpMaterial == null) {
                tmpMaterial = new SpriteMaterial();
            }

            SpriteComponent sprite = GetComponent<SpriteComponent>();
            sprite.Material = tmpMaterial;

            if (sprite.Material.MainTexture == null) {
                //var s = AssetManager.GetAsset<Texture2D>("grass_tex");
                
                //sprite.Material.MainTexture = AssetManager.GetAsset<SpriteTexture>("grass");
            }

            //sprite.Enabled = false;
            //base.Initialize();
        }

        /// <inheritdoc />
        public Floor(Vector2 pos, float rot, Vector2 scale) : base(pos, rot, scale) { }
    }

}
