namespace SE.Physics
{

    public delegate bool OnCollisionEventHandler(Fixture sender, Fixture other, Contact contact);

    public delegate void OnSeparationEventHandler(Fixture sender, Fixture other, Contact contact);

    public delegate void AfterCollisionEventHandler(Fixture sender, Fixture other, ContactVelocityConstraint impulse);

    public delegate bool BeforeCollisionEventHandler(Fixture sender, Fixture other);

    public delegate bool CollisionFilterDelegate(Fixture fixtureA, Fixture fixtureB);
}