using DeeZ.Core;
using DeeZ.Core.Extensions;
using DeeZ.Engine.Attributes;
using DeeZ.Engine.Common;
using DeeZ.Engine.Components;
using DeeZ.Engine.Components.Lighting;
using DeeZ.Engine.Components.Network;
using DeeZ.Engine.Networking;
using DeeZ.Engine.Physics;
using DeeZ.Engine.Rendering;
using DeeZ.Engine.Rendering.Data;
using Microsoft.Xna.Framework;
using Random = DeeZ.Engine.Random;
using TestParticleSystem = DeeZEngine_Demos.Particles.TestParticleSystem;
using Vector2 = System.Numerics.Vector2;

namespace DeeZEngine_Demos.GameObjects
{
    
    [Components(
        typeof(NetworkIdentity),
        typeof(NetTransform),
        typeof(Sprite),
        typeof(PhysicsObject),
        typeof(ParticleEmitter),
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
                myPhysics.Body.SetRestitution(0.999f);
                myPhysics.Body.SetFriction(0.0f);

                //myPhysics.Body.OnCollision += Body_OnCollision; // Test event code.
                SetV(new Vector2(Random.Next(64, 256), Random.Next(64, 256)));
                c = new Color(Random.Next(100, 255), Random.Next(100, 255), Random.Next(100, 255));
            } else {
                myPhysics.Enabled = false;
            }
            GetComponent<ParticleEmitter>().ParticleSystem = new TestParticleSystem();

            networkIdentity.OnSerializeNetworkState += () => c.Serialize();
            networkIdentity.OnRestoreNetworkState += jsonString => {
                c = jsonString.Deserialize<Color>();
                sprite.Color = c;
                light.Color = c;
            };

            sprite.spriteTexture = AssetManager.Get<SpriteTexture>(this, "circle");
            sprite.Origin = new Vector2(16, 16);
            sprite.Color = c;
            sprite.RenderType = RenderDataType.Transparent;

            light.Size = new Vector2(400, 400);
            light.Color = c;
            light.Intensity = 1.0f;
            base.OnInitialize();
        }

        // TODO: Test event code. Remove when done.
        private bool Body_OnCollision(Fixture sender, Fixture other, Contact contact)
        {
            if (other.GameObject is WallDown) {
                Network.Instantiate("bouncy", new Vector2(Transform.Position.X, Transform.Position.Y + 100));
            }
            return true;
        }

        protected override void OnUpdate()
        { 
            base.OnUpdate();
            if(Network.InstanceType == NetInstanceType.Server)
                Transform.Rotation = myPhysics.Body.LinearVelocity.ToRotation();
        }

        private void SetV(Vector2 v)
        {
            myPhysics.Body.LinearVelocity = v;
        }

        public BouncyBall(Vector2 pos, float rot, Vector2 scale) : base(pos, rot, scale) { }

    }

}
