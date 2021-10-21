using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement;
using SE.Attributes;
using SE.Common;
using SE.Components;
using SE.Components.Lighting;
using SE.Components.Network;
using SE.Core;
using SE.Core.Extensions;
using SE.Physics;
using SE.Rendering;
using Random = SE.Utility.Random;
using Vector2 = System.Numerics.Vector2;

namespace SEDemos.GameObjects
{

    [Components(
        typeof(NetworkIdentity),
        typeof(NetTransform2D),
        typeof(Sprite),
        typeof(PhysicsObject),
        typeof(ParticleSystem),
        typeof(LightComponent)
        )]

    public class BouncyBall : GameObject
    {
        public override bool IsDynamic => true;

        private PhysicsObject myPhysics;
        private LightComponent light;
        private Color c = Color.White;

        private static Material tmpMaterial = new Material();

        protected sealed override void OnInitialize()
        {
            myPhysics = GetComponent<PhysicsObject>();
            light = GetComponent<LightComponent>();
            NetworkIdentity networkIdentity = GetComponent<NetworkIdentity>();
            Sprite sprite = GetComponent<Sprite>();
            sprite.Material = tmpMaterial;
            if (Network.InstanceType == NetInstanceType.Server) {
                myPhysics.Body = Physics.CreateCircle(16, 1.0f, new Vector2(16), BodyType.Dynamic);
                myPhysics.Body.SetRestitution(0.5f);
                myPhysics.Body.SetFriction(0.0f);

                SetV(new Vector2(Random.Next(256, 512), Random.Next(256, 512)));
                c = new Color(Random.Next(100, 255), Random.Next(100, 255), Random.Next(100, 255));
            } else {
                myPhysics.Enabled = false;
            }

            Asset<SpriteTexture> tex = AssetManager.GetAsset<SpriteTexture>("circle");

            ParticleSystem system = GetComponent<ParticleSystem>();
            system.Texture = AssetManager.Get<Texture2D>(this, "Smoke");

            Sprite s = sprite;
            networkIdentity.OnSerializeNetworkState += () => {
                NetDataWriter writer = new NetDataWriter();
                writer.Put(s.Color.R);
                writer.Put(s.Color.G);
                writer.Put(s.Color.B);
                writer.Put(s.Color.A);
                return writer.CopyData();
            };

            networkIdentity.OnRestoreNetworkState += reader => {
                c = new Color(reader.GetByte(), reader.GetByte(), reader.GetByte(), reader.GetByte());
                sprite.Color = c;
                light.Color = c;
            };

            sprite.LayerDepth = 0.01f;
            sprite.SpriteTextureAsset = tex;
            sprite.Origin = new Vector2(16, 16);
            sprite.Color = c;
            sprite.Material.BlendMode = BlendMode.Transparent;

            light.Size = new Vector2(1200, 1200);
            light.Color = c;
            light.Intensity = 1.0f;

            //light.Enabled = false;
            base.OnInitialize();
        }

        private float timer = 5.0f;

        protected override void OnUpdate()
        {
            timer -= Time.DeltaTime;
            if (timer <= 0.0f) {
                Destroy();
                return;
            }

            base.OnUpdate();
            if (Network.InstanceType == NetInstanceType.Server) {
                Transform.Rotation = myPhysics.Body.LinearVelocity.ToRotation();
                if (myPhysics.Body.LinearVelocity.Length() < 256.0f) {
                    myPhysics.Body.LinearVelocity *= 3.0f;
                }
            }
        }

        private void SetV(Vector2 v)
        {
            myPhysics.Body.LinearVelocity = v;
        }

        public BouncyBall(Vector2 pos, float rot, Vector2 scale) : base(pos, rot, scale) { }
    }
}
