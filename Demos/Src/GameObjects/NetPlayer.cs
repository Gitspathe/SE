using Microsoft.Xna.Framework;
using SE.Common;
using SE.Components;
using SE.Components.Network;
using SE.Core;
using SE.Input;
using SE.Physics;
using SE.Rendering;
using Vector2 = System.Numerics.Vector2;

namespace SEDemos.GameObjects
{

    internal class NetPlayer : GameObject
    {
        /// <inheritdoc />
        public override bool IsDynamic => true;

        public override bool DestroyOnLoad => false;

        public PhysicsObject Physics;

        public static Players PlayerIndex = Players.One;

        private NetworkIdentity identity;

        /// <inheritdoc />
        protected sealed override void OnInitialize()
        {
            identity = new NetworkIdentity();
            Sprite sprite = new Sprite(AssetManager.Get<SpriteTexture>(this, "player"), Color.Blue, new Vector2(26, 26), 0.2f);
            Physics = new PhysicsObject(new Rectangle(26, 26, 50, 50), 1.0f, BodyType.Dynamic);
            Physics.Body.FixedRotation = true;
            Physics.Body.SetFriction(0.0f);
            Physics.Body.LinearDampening = 0.0f;

            AddComponent(sprite);
            AddComponent(Physics);
            AddComponent(identity);
            identity.OnSetup += isOwner => {
                if (!identity.IsOwner)
                    Physics.Body.BodyType = BodyType.Kinematic;

                AddComponent(new NetTransform2D(NetTransform2D.CompensationQuality.High));
                if (isOwner) {
                    //AddComponent(new PathfinderAgent());
                    NetworkedEntity player = new NetworkedEntity();
                    player.PlayerIndex = PlayerIndex;
                    PlayerIndex += 1;
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
