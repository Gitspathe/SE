using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SE.Particles;
using SE.Particles.Modules;
using SE.Physics;
using SE.World.Partitioning;
using Vector2 = System.Numerics.Vector2;
namespace SE.Engine.Particles.Modules
{
    // TODO: Convert to new physics.
    public class CollisionModule : ParticleModule
    {
        private Point colliderSize;
        private Vector2 size;

        private PartitionTile[][] tiles;
        private Dictionary<Particle, Rectangle> colliders = new Dictionary<Particle, Rectangle>(128);
        private List<Particle> collided = new List<Particle>();
        private List<Particle> firstCollisions = new List<Particle>();

        //private List<ICollider> tmpColliders = new List<ICollider>();
        private LayerType[] physicsLayers;

        private int currentUpdate;
        private int updatesNeeded;

        private object threadLock = new object();

        private bool destroyOnCollision;
        public bool DestroyOnCollision {
            get => destroyOnCollision;
            set {
                destroyOnCollision = value;
                if (destroyOnCollision) {
                    stick = false;
                }
            }
        }

        private bool stick;
        public bool Stick {
            get => stick;
            set {
                stick = value;
                if (stick) {
                    destroyOnCollision = false;
                }
            }
        }

        private bool doAction;
        private Action<Particle> action;
        public Action<Particle> OnCollision {
            get => action;
            set {
                action = value;
                if (action != null) {
                    doAction = true;
                } else {
                    doAction = false;
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void ParticleHook(Particle particle)
        {
            base.ParticleHook(particle);
            colliders.Add(particle, new Rectangle((int)particle.GlobalPosition.X, (int)particle.GlobalPosition.Y,
                colliderSize.X, colliderSize.Y));
        }

        public override void ParticleUnhook(Particle particle)
        {
            base.ParticleUnhook(particle);
            colliders.Remove(particle);
            collided.Remove(particle);
            firstCollisions.Remove(particle);
        }

        public override void UpdateThreaded(float deltaTime)
        {
            throw new NotImplementedException();
            //    base.UpdateThreaded(deltaTime);
            //    currentUpdate++;
            //    if (currentUpdate < updatesNeeded) 
            //        return;

            //    foreach (KeyValuePair<Particle, Rectangle> collider in colliders) {
            //        tmpColliders.Clear();
            //        Particle particle = collider.Key;
            //        if (collided.Contains(particle))
            //            continue;

            //        Rectangle rect = collider.Value;
            //        rect.X = (int)particle.globalPosition.X;
            //        rect.Y = (int)particle.globalPosition.Y;
            //        PartitionTile t = SpatialPartitionManager.GetTile(particle.globalPosition);
            //        if (t == null)
            //            continue;

            //        CollisionExtension tileCollision = t.collision;
            //        tileCollision.GetCollidersOfLayer(tmpColliders, physicsLayers);
            //        for (int i = 0; i < tmpColliders.Count; i++) {
            //            if (tmpColliders[i].Owner == particleSystem.Emitter.Owner)
            //                continue;

            //            if (tmpColliders[i].TestIntersection(rect, Vector2.Zero)) {
            //                firstCollisions.Add(particle);
            //            }
            //        }
            //    }

            //    lock (threadLock) {
            //        for (int i = 0; i < firstCollisions.Count; i++) {
            //            if (doAction) {
            //                action.Invoke(firstCollisions[i]);
            //            }
            //            collided.Add(firstCollisions[i]);
            //        }
            //        firstCollisions.Clear();
            //        for (int i = 0; i < collided.Count; i++) {
            //            Particle particle = collided[i];
            //            if (destroyOnCollision) {
            //                particle.Enabled = false;
            //            } else if (stick) {
            //                particle.currentVelocity = Vector2.Zero;
            //            }
            //        }
            //    }
            //    currentUpdate = 0;
        }

        public CollisionModule(ParticleSystem particleSystem, Point colliderSize, LayerType[] layers, CollisionQuality quality = CollisionQuality.Low) : base(particleSystem)
        {
            switch (quality) {
                case CollisionQuality.Low:
                    updatesNeeded = 5;
                    break;
                case CollisionQuality.Medium:
                    updatesNeeded = 3;
                    break;
                case CollisionQuality.High:
                    updatesNeeded = 1;
                    break;
            }
            this.colliderSize = colliderSize;
            physicsLayers = layers;
        }

        public enum CollisionQuality
        {
            High,
            Medium,
            Low
        }

    }

}