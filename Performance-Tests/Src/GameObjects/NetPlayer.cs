using DeeZ.Core;
using DeeZ.Engine.Common;
using DeeZ.Engine.Components;
using DeeZ.Engine.Components.Network;
using DeeZ.Engine.Physics;
using DeeZ.Engine.Rendering;
using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;

namespace DeeZEngine_Demos.GameObjects
{

    internal class NetPlayer : GameObject
    {

        /// <inheritdoc />
        public override bool IsDynamic => true;

        public override bool DestroyOnLoad => false;

        public PhysicsObject physics;

        private NetworkIdentity identity;

        /// <inheritdoc />
        protected sealed override void OnInitialize()
        {
            identity = new NetworkIdentity();
            Sprite sprite = new Sprite(AssetManager.Get<SpriteTexture>(this, "player"), Color.Blue, new Vector2(26, 26), 0.2f);
            physics = new PhysicsObject(new Rectangle(26, 26, 50, 50), 1.0f, BodyType.Dynamic);
            physics.Body.FixedRotation = true;
            physics.Body.SetFriction(0.0f);
            physics.Body.LinearDampening = 0.0f;

            AddComponent(sprite);
            AddComponent(physics);
            AddComponent(identity);
            identity.OnSetup += isOwner => {
                if (!identity.IsOwner)
                    physics.Body.BodyType = BodyType.Kinematic;

                AddComponent(new NetTransform(NetTransform.CompensationQuality.High));
                if (isOwner) {
                    NetworkedEntity player = new NetworkedEntity();
                    AddComponent(player);
                }
            };
            base.OnInitialize();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if(!identity.IsOwner)
                return;

            //Ray2D ray = new Ray2D(Transform.GlobalPosition, Screen.WorldMousePoint);
            //if (Physics.RayCast(ray, out RayCast2DHit hit)) {
            //    if (hit.GameObject is WallLeft) {
            //        Instantiate(typeof(WallUp), hit.Point);
            //    }
            //}
        }

        public NetPlayer(Vector2 pos, float rot, Vector2 scale) : base(pos, rot, scale) { }

    }

}
