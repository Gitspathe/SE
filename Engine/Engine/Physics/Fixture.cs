using SE.Common;
using SE.Components;
using tainicom.Aether.Physics2D.Collision.Shapes;
using AetherFixture = tainicom.Aether.Physics2D.Dynamics.Fixture;
using AetherCategory = tainicom.Aether.Physics2D.Dynamics.Category;

namespace SE.Physics
{
    public class Fixture
    {
        internal AetherFixture InternalFixture;

        public short CollisionGroup {
            get => InternalFixture.CollisionGroup;
            set => InternalFixture.CollisionGroup = value;
        }

        public Category CollidesWith {
            get => (Category) ((int)(InternalFixture.CollidesWith));
            set => InternalFixture.CollidesWith = (AetherCategory) ((int)(value));
        }

        public Category CollisionCategories {
            get => (Category) ((int)(InternalFixture.CollisionCategories));
            set => InternalFixture.CollisionCategories = (AetherCategory) ((int)(value));
        }

        public bool IsSensor {
            get => InternalFixture.IsSensor;
            set => InternalFixture.IsSensor = value;
        }

        public Shape Shape => InternalFixture.Shape;
        public PhysicsBody PhysicsBody => InternalFixture.PhysicsBody;
        public PhysicsObject PhysicsObject => PhysicsBody?.PhysicsObject;
        public GameObject GameObject => PhysicsObject?.Owner;

        public float Friction {
            get => InternalFixture.Friction;
            set => InternalFixture.Friction = value;
        }

        public float Restitution {
            get => InternalFixture.Restitution;
            set => InternalFixture.Restitution = value;
        }

        public void GetAabb(out AABB aabb, int childIndex)
        {
            InternalFixture.GetAABB(out tainicom.Aether.Physics2D.Collision.AABB tmpAabb, childIndex);
            aabb = new AABB(tmpAabb);
        }

        internal Fixture(Shape shape)
        {
            InternalFixture = new AetherFixture(shape);
            InternalFixture.DeeZFixture = this;
        }

        internal Fixture(AetherFixture internalFixture)
        {
            internalFixture.DeeZFixture = this;
        }
    }

}
