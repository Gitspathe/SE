using System.Collections.Generic;
using SE.Common;
using SE.Components;
using SE.Core;
using SE.Physics;
using SE.Core.Extensions;
using Vector2 = System.Numerics.Vector2;

namespace SE.Engine.Components
{
    // TODO: Replace this with a much better version eventually. This is just a shitty temporary, untested version.
    public class PathfinderAgent : Component
    {
        private float timer = 2.0f;
        private Stack<NavigationGrid.Node> nodes;
        private NavigationGrid.Node node;

        private Vector2 lookAt = Vector2.Zero;
        private PhysicsObject myPhysics;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            myPhysics = Owner.GetComponent<PhysicsObject>();
        }

        protected override void OnUpdate()
        {
            timer -= Time.DeltaTime;
            if (timer < 0) {
                nodes = NavigationManager.NavGrids[0].FindPath(Owner.Transform.GlobalPositionInternal, Screen.MousePoint.HasValue ? Screen.MousePoint.Value : new Vector2(9999, 9999));
                timer = 0.333f;
            }
            if (nodes != null && nodes.Count > 0) {
                if (node == null || ((NavigationManager.NavGrids[0].GetPosition(node) - Owner.Transform.GlobalPositionInternal).Length() < 0.5f)) {
                    node = nodes.Pop();
                }
                if (node != null) {
                    Vector2 diff = NavigationManager.NavGrids[0].GetPosition(node) - Owner.Transform.GlobalPositionInternal;
                    lookAt = NavigationManager.NavGrids[0].GetPosition(node);
                    diff = Vector2.Normalize(diff);
                    myPhysics.Body.LinearVelocity = diff * 192;
                }
            }
            Owner.Transform.Rotation = Owner.Transform.GlobalPositionInternal.GetRotationFacing(lookAt);
            base.OnUpdate();
        }

    }

}