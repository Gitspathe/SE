using System.Collections.Generic;
using DeeZ.Core;
using DeeZ.Engine.Animation;
using DeeZ.Engine.Common;
using DeeZ.Engine.Components;
using DeeZ.Engine.Pooling;
using DeeZ.Engine.Rendering;
using DeeZ.Engine.Rendering.Data;
using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;
using Random = DeeZ.Engine.Random;

namespace DeeZEngine_Demos.GameObjects
{

    public class AnimatedFloor : GameObject, IPoolableGameObject
    {
        public bool InPool { get; set; }
        public bool ReturnOnDestroy { get; set; }
        public IGameObjectPool MyPool { get; set; }

        /// <inheritdoc />
        public override bool IsDynamic => true;

        public override bool DestroyOnLoad => false; // Tiles are destroyed anyway.

        private BasicAnimation anim;

        /// <inheritdoc />
        protected sealed override void OnInitialize()
        {
            base.OnInitialize();
            int m = Random.Next(5);
            Sprite sprite = new Sprite(AssetManager.Get<SpriteTexture>(this, "floor"), Color.White) {
                LayerDepth = 0.1f, 
                RenderType = RenderDataType.Basic
            };
            AddComponent(sprite);

            Vector2 startPos = Transform.Position;
            AnimatedVector2 f = new AnimatedVector2();
            Curve c = new Curve();
            c.Keys.Add(new CurveKey(0, 0));
            c.Keys.Add(new CurveKey(1, 200));
            c.Keys.Add(new CurveKey(2, 100));
            c.Keys.Add(new CurveKey(2.5f, 300));
            f.SetCurve(c, c);
            f.Attach(new Ref<Vector2>(i => Transform.Position = i + startPos));
            AnimationCollection aC = new AnimationCollection(f) {
                WrapMode = WrapMode.PingPong
            };

            AnimatedVector2 f2 = new AnimatedVector2();
            Curve c2 = new Curve();
            c2.Keys.Add(new CurveKey(0, 100));
            c2.Keys.Add(new CurveKey(1, 400));
            c2.Keys.Add(new CurveKey(2, 150));
            c2.Keys.Add(new CurveKey(2.5f, 250));
            f2.SetCurve(c2, c2);
            f2.Attach(new Ref<Vector2>(i => Transform.Position = i + startPos));

            AnimatedColor col = new AnimatedColor();
            col.SetLerp(Color.White, new Color((byte)Random.Next(255), (byte)Random.Next(255), (byte)Random.Next(255), (byte)1.0f), 2.5f);
            col.Attach(new Ref<Color>(color => {
                List<Sprite> sprites = GetAllComponents<Sprite>();
                for (int i = 0; i < sprites.Count; i++) {
                    sprites[i].Color = color;
                }
            }));

            AnimationCollection aC2 = new AnimationCollection(f2);
            aC2.Add(col);
            aC2.WrapMode = WrapMode.PingPong;

            anim = new BasicAnimation();
            anim.AddAnimation("Anim", aC);
            anim.AddAnimation("Anim2", aC2);
            AddComponent(anim);
            anim.Play("Anim2");
        }

        private float timer;
        protected override void OnUpdate()
        {
            base.OnUpdate();
            timer += Time.DeltaTime;
            if (timer > 10) {
                if(anim.isPlaying("Anim"))
                    anim.Play("Anim2");
                else
                    anim.Play("Anim");

                timer = 0f;
            }
        }

        /// <inheritdoc />
        public AnimatedFloor(Vector2 pos, float rot, Vector2 scale) : base(pos, rot, scale) { }

    }

}
