using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Attributes;
using SE.Common;
using SE.Components;
using SE.Components.Lighting;
using SE.Components.Network;
using SE.Core;
using SE.Core.Extensions;
using SE.Engine.Networking;
using SE.Physics;
using SE.Rendering;
using Random = SE.Utility.Random;
using Vector2 = System.Numerics.Vector2;

namespace SEDemos.GameObjects
{

    [Components(
        typeof(NetworkIdentity),
        typeof(NetTransform),
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

        protected sealed override void OnInitialize()
        {
            myPhysics = GetComponent<PhysicsObject>();
            light = GetComponent<LightComponent>();
            NetworkIdentity networkIdentity = GetComponent<NetworkIdentity>();
            Sprite sprite = GetComponent<Sprite>();
            if (Network.InstanceType == NetInstanceType.Server) {
                myPhysics.Body = Physics.CreateCircle(16, 1.0f, new Vector2(16), BodyType.Dynamic);
                myPhysics.Body.SetRestitution(0.5f);
                myPhysics.Body.SetFriction(0.0f);

                myPhysics.Body.OnCollision += Body_OnCollision; // Test event code.
                SetV(new Vector2(Random.Next(256, 512), Random.Next(256, 512)));
                c = new Color(Random.Next(100, 255), Random.Next(100, 255), Random.Next(100, 255));
            } else {
                myPhysics.Enabled = false;
            }

            SpriteTexture tex = AssetManager.Get<SpriteTexture>(this, "circle");

            ParticleSystem system = GetComponent<ParticleSystem>();
            system.Texture = AssetManager.Get<Texture2D>(this, "Smoke");
            system.SourceRect = tex.SourceRectangle;

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

            sprite.SpriteTexture = tex;
            sprite.Origin = new Vector2(16, 16);
            sprite.Color = c;
            sprite.BlendMode = BlendMode.Transparent;

            light.Size = new Vector2(400, 400);
            light.Color = c;
            light.Intensity = 1.0f;

            //light.Enabled = false;
            base.OnInitialize();
        }

        // TODO: Test event code. Remove when done.
        private bool Body_OnCollision(Fixture sender, Fixture other, Contact contact)
        {
            if (other.GameObject is WallDown) {
                //NetHelper.Instantiate("bouncy", position: new Vector2(Transform.Position.X, Transform.Position.Y - 24));
                Destroy();
            }
            return true;
        }

        protected override void OnUpdate()
        { 
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
