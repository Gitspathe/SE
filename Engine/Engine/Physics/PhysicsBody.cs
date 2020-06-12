using System;
using System.Collections.Generic;
using SE.Components;
using SE.Core;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Contacts;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using Vector2 = System.Numerics.Vector2;
using MonoGameVector2 = Microsoft.Xna.Framework.Vector2;
using AetherBodyType = tainicom.Aether.Physics2D.Dynamics.BodyType;
using static SE.Core.Physics;

using AetherContact = tainicom.Aether.Physics2D.Dynamics.Contacts.Contact;
using AetherFixture = tainicom.Aether.Physics2D.Dynamics.Fixture;

namespace SE.Physics
{
    // TODO: Navigation.
    public class PhysicsBody : IPhysicsDependencyBody<PhysicsBody>, IDisposable
    {
        internal Body Body;
        internal bool AddedToPhysics;

        public IPhysicsBodyProvider Provider { get; internal set; }
        public List<Fixture> FixtureList { get; set; } = new List<Fixture>(1);

        internal JointEdge JointList => Body.JointList;
        internal ContactEdge ContactList => Body.ContactList;
        public PhysicsBody GetBody => this;

        /// <summary>True if PhysicsBody was destroyed while the world was locked.
        ///          The PhysicsBody will be destroyed when the world is next available.</summary>
        public bool PendingRemove { get; internal set; }

        /// <summary>True if the PhysicsBody was created while the world was locked.
        ///          In this state, some properties may not be set up.</summary>
        public bool PendingCreate { get; internal set; }

        /// <summary>True if the PhysicsBody is in a valid, non-error-prone state.</summary>
        public bool ValidState => !PendingCreate && !PendingRemove && AddedToPhysics;

        internal OnCollisionEventHandler OnCollisionEventHandler;
        public event OnCollisionEventHandler OnCollision {
            add => OnCollisionEventHandler += value;
            remove => OnCollisionEventHandler -= value;
        }

        internal OnSeparationEventHandler OnSeparationEventHandler;
        public event OnSeparationEventHandler OnSeparation {
            add => OnSeparationEventHandler += value;
            remove => OnSeparationEventHandler -= value;
        }

        public Vector2 Position {
            get => ToPixels(Body.Position);
            set => Body.Position = ToMeters(value);
        }

        public Vector2 WorldCenter => ToPixels(Body.WorldCenter);

        public Vector2 LocalCenter {
            get => ToPixels(Body.LocalCenter);
            set => Body.LocalCenter = ToMeters(value);
        }

        public float Mass {
            get => Body.Mass;
            set => Body.Mass = value;
        }

        public float Inertia {
            get => Body.Inertia;
            set => Body.Inertia = value;
        }

        public Vector2 LinearVelocity {
            get => ToPixels(Body.LinearVelocity);
            set => Body.LinearVelocity = ToMeters(value);
        }

        public float LinearDampening {
            get => Body.LinearDamping;
            set => Body.LinearDamping = value;
        }

        public float AngularVelocity {
            get => Body.AngularVelocity;
            set => Body.AngularVelocity = value;
        }

        public float AngularDampening {
            get => Body.AngularDamping;
            set => Body.AngularDamping = value;
        }

        public float Revolutions => Body.Revolutions;

        public int IslandIndex {
            get => Body.IslandIndex;
            set => Body.IslandIndex = value;
        }

        public BodyType BodyType {
            get => (BodyType)(int)Body.BodyType;
            set => Body.BodyType = (AetherBodyType)(int)value;
        }

        public bool IsBullet {
            get => Body.IsBullet;
            set => Body.IsBullet = value;
        }

        public bool SleepingAllowed {
            get => Body.SleepingAllowed;
            set => Body.SleepingAllowed = value;
        }

        public bool Awake {
            get => Body.Awake;
            set => Body.Awake = value;
        }

        public bool BodyEnabled {
            get => Body.Enabled;
            set => Body.Enabled = value;
        }

        public bool FixedRotation {
            get => Body.FixedRotation;
            set => Body.FixedRotation = value;
        }

        public bool IgnoreGravity {
            get => Body.IgnoreGravity;
            set => Body.IgnoreGravity = value;
        }

        public bool IgnoreCcd {
            get => Body.IgnoreCCD;
            set => Body.IgnoreCCD = value;
        }

        public void ApplyForce(Vector2 force) 
            => Body.ApplyForce(force.ToMonoGameVector2());

        public void ApplyForce(Vector2 force, Vector2 point) 
            => Body.ApplyForce(force.ToMonoGameVector2(), point.ToMonoGameVector2());

        public void ApplyTorque(float torque)
            => Body.ApplyTorque(torque);

        public void ApplyLinearImpulse(Vector2 impulse) 
            => Body.ApplyLinearImpulse(impulse.ToMonoGameVector2());

        public void ApplyLinearImpulse(Vector2 impulse, Vector2 point) 
            => Body.ApplyLinearImpulse(impulse.ToMonoGameVector2(), point.ToMonoGameVector2());

        public void ApplyAngularImpulse(float impulse) 
            => Body.ApplyAngularImpulse(impulse);

        public void ResetMassData() 
            => Body.ResetMassData();

        public void ResetDynamics() 
            => Body.ResetDynamics();

        public Vector2 GetWorldPoint(Vector2 localPoint) 
            => ToPixels(Body.GetWorldPoint(ToMeters(localPoint)));

        public Vector2 GetWorldVector(Vector2 localVector) 
            => ToPixels(Body.GetWorldVector(ToMeters(localVector)));

        public Vector2 GetLocalPoint(Vector2 worldPoint)
            => ToPixels(Body.GetLocalPoint(ToMeters(worldPoint)));

        public Vector2 GetLocalVector(Vector2 worldVector) 
            => ToPixels(Body.GetLocalVector(ToMeters(worldVector)));

        public Vector2 GetLinearVelocityFromWorldPoint(Vector2 worldPoint) 
            => ToPixels(Body.GetLinearVelocityFromWorldPoint(ToMeters(worldPoint)));

        public Vector2 GetLinearVelocityFromLocalPoint(Vector2 localPoint) 
            => ToPixels(Body.GetLinearVelocityFromLocalPoint(ToMeters(localPoint)));

        public void SetRestitution(float restitution) 
            => Body.SetRestitution(restitution);

        public void SetFriction(float friction) 
            => Body.SetFriction(friction);

        public void SetIsSensor(bool isSensor) 
            => Body.SetIsSensor(isSensor);

        private bool isDisposed;

        internal void OverridePosition(Vector2 pos) 
        {
            if(!AddedToPhysics || PendingRemove)
                return;

            Body.Position = ToMeters(pos);
        }

        public void AddFixture(Fixture fixture)
        {
            FixtureList.Add(fixture);
            Body.Add(fixture.InternalFixture);
        }

        public void RemoveFixture(Fixture fixture)
        {
            FixtureList.Remove(fixture);
            Body.Remove(fixture.InternalFixture);
        }

        public Fixture AddFixture(Shape shape)
        {
            Fixture fixture = new Fixture(Body.CreateFixture(shape));
            FixtureList.Add(fixture);
            return fixture;
        }

        public Fixture AddRectangle(float width, float height, float density, Vector2 offset)
        {
            MonoGameVector2 finalOffset = new MonoGameVector2((width * 0.5f) + -offset.X, -(height * 0.5f) + -offset.Y);
            Fixture fixture = new Fixture(Body.CreateRectangle(ToMeters(width), ToMeters(height), density, ToMeters(finalOffset)));
            FixtureList.Add(fixture);
            return fixture;
        }

        public Fixture AddCircle(float radius, float density, Vector2 offset)
        {
            MonoGameVector2 finalOffset = new MonoGameVector2(-radius + offset.X, -radius + offset.Y);
            Fixture fixture = new Fixture(Body.CreateCircle(ToMeters(radius), density, ToMeters(finalOffset)));
            FixtureList.Add(fixture);
            return fixture;
        }

        public PhysicsBody(Body body, IPhysicsBodyProvider provider = null)
        {
            Add(this);
            body.DependencyBody = this;
            Body = body;
            Provider = provider;
            SetupEvents();
        }

        private void SetupEvents()
        {
            Body.OnCollision += OnBodyCollisionEvent;
            Body.OnSeparation += OnBodySeperateEvent;
        }

        private void OnBodySeperateEvent(AetherFixture sender, AetherFixture other, AetherContact contact)
        {
            if (ValidState) {
                AddSeparationEvent(this, sender, other, contact);
            }
        }

        private bool OnBodyCollisionEvent(AetherFixture sender, AetherFixture other, AetherContact contact)
        {
            if (ValidState) {
                AddCollisionEvent(this, sender, other, contact);
            }
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing = true)
        {
            if(isDisposed)
                return;

            Remove(this);
            Body.OnCollision -= OnBodyCollisionEvent;
            Body.OnSeparation -= OnBodySeperateEvent;
            OnCollisionEventHandler = null;
            OnSeparationEventHandler = null;
            isDisposed = true;
        }
    }

    public interface IPhysicsBodyProvider : IDisposable
    {
        PhysicsBody Body { get; }
    }
}
