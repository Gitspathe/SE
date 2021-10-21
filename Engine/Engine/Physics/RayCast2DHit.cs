using SE.Common;
using SE.Components;
using System;
using System.Numerics;

namespace SE.Physics
{
    public struct RayCast2DHit
    {
        private Fixture fixture;
        public Fixture Fixture {
            get {
                if (fixture == null)
                    throw new NullReferenceException("The RayCast2DHit is empty. Did you forget to check if the raycast returned true?");

                return fixture;
            }
        }

        private Vector2 point;
        public Vector2 Point {
            get {
                if (fixture == null)
                    throw new NullReferenceException("The RayCast2DHit is empty. Did you forget to check if the raycast returned true?");

                return point;
            }
        }

        private Vector2 normal;
        public Vector2 Normal {
            get {
                if (fixture == null)
                    throw new NullReferenceException("The RayCast2DHit is empty. Did you forget to check if the raycast returned true?");

                return normal;
            }
        }

        private float distance;
        public float Distance {
            get {
                if (fixture == null)
                    throw new NullReferenceException("The RayCast2DHit is empty. Did you forget to check if the raycast returned true?");

                return distance;
            }
        }

        public PhysicsBody Body => Fixture.Body;
        public IPhysicsBodyProvider BodyProvider => Body?.Provider;
        public PhysicsObject PhysicsObject => (Body?.Provider as PhysicsObject);
        public GameObject GameObject => (Body?.Provider as PhysicsObject)?.Owner;

        internal RayCast2DHit(Fixture fixture, Vector2 point, Vector2 normal, float distance)
        {
            this.fixture = fixture;
            this.point = point;
            this.normal = normal;
            this.distance = distance;
        }
    }
}
