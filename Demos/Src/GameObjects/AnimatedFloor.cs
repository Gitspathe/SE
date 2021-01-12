using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SE;
using SE.Animating;
using SE.Common;
using SE.Components;
using SE.Core;
using SE.Pooling;
using SE.Rendering;
using Vector2 = System.Numerics.Vector2;
using Random = SE.Utility.Random;
namespace SEDemos.GameObjects
{

    public class AnimatedFloor : GameObject, IPoolableGameObject
    {
        public bool ReturnOnDestroy { get; set; }
        public IGameObjectPool MyPool { get; set; }

        /// <inheritdoc />
        public override bool IsDynamic => true;

        public override bool DestroyOnLoad => false; // Tiles are destroyed anyway.

        private BasicAnimator anim;

        /// <inheritdoc />
        protected sealed override void OnInitialize()
        {
            base.OnInitialize();
            int m = Random.Next(5);
            Sprite sprite = new Sprite(AssetManager.GetAsset<SpriteTexture>("floor"), Color.White) {
                LayerDepth = 0.1f, 
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
            Animation aC = new Animation(f) {
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

            Animation aC2 = new Animation(f2);
            aC2.Add(col);
            aC2.WrapMode = WrapMode.PingPong;

            anim = new BasicAnimator();
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
                if(anim.IsPlaying("Anim"))
                    anim.Play("Anim2");
                else
                    anim.Play("Anim");

                timer = 0f;
            }
        }

        /// <inheritdoc />
        public AnimatedFloor(Vector2 pos, float rot, Vector2 scale) : base(pos, rot, scale) { }

        public void TakenFromPool() { }
        public void ReturnedToPool() { }
        public void PoolInitialize() { }
    }

}
