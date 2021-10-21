using SE.Common;
using SE.Components;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Dynamics;
using AetherCategory = tainicom.Aether.Physics2D.Dynamics.Category;
using AetherFixture = tainicom.Aether.Physics2D.Dynamics.Fixture;

namespace SE.Physics
{
    public class Fixture : IPhysicsDependencyFixture<Fixture>
    {
        internal AetherFixture InternalFixture;

        public Fixture GetFixture => this;

        public short CollisionGroup {
            get => InternalFixture.CollisionGroup;
            set => InternalFixture.CollisionGroup = value;
        }

        public Category CollidesWith {
            get => (Category)((int)(InternalFixture.CollidesWith));
            set => InternalFixture.CollidesWith = (AetherCategory)((int)(value));
        }

        public Category CollisionCategories {
            get => (Category)((int)(InternalFixture.CollisionCategories));
            set => InternalFixture.CollisionCategories = (AetherCategory)((int)(value));
        }

        public bool IsSensor {
            get => InternalFixture.IsSensor;
            set => InternalFixture.IsSensor = value;
        }

        public Shape Shape => InternalFixture.Shape;
        public PhysicsBody Body => InternalFixture.DependencyBody as PhysicsBody;
        public IPhysicsBodyProvider BodyProvider => Body?.Provider;
        public PhysicsObject PhysicsObject => (Body?.Provider as PhysicsObject);
        public GameObject GameObject => (BodyProvider as PhysicsObject)?.Owner;

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
            InternalFixture.DependencyFixture = this;
        }

        internal Fixture(AetherFixture internalFixture)
        {
            InternalFixture = internalFixture;
            internalFixture.DependencyFixture = this;
        }
    }

}
