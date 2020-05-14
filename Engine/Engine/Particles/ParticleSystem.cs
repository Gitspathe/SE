using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SE.AssetManagement;
using SE.Components;
using SE.Core;
using SE.Engine.Utility;
using SE.Particles.Modules;
using Vector2 = System.Numerics.Vector2;

namespace SE.Particles
{

    /// <summary>
    /// An object which creates particles.
    /// </summary>
    public class ParticleSystem : IAssetConsumer
    {
        public Point? BoundsSize;
        public Vector2 Position;
        public float Rotation;

        internal QuickList<Particle> ActiveParticles;
        internal List<ParticleModule> Modules = new List<ParticleModule>();
        internal bool HasModules;
        internal bool IsVisible;
        internal bool InVisibleList;
        internal bool InParticleEngine;
        internal int MyList;

        private bool hasEmitter;

        public virtual ParticleRenderType ParticleRenderType => ParticleRenderType.Additive;

        HashSet<IAsset> IAssetConsumer.ReferencedAssets { get; set; }

        private bool enabled;
        public bool Enabled {
            get => enabled;
            set {
                enabled = value;
                if (enabled) {
                    ParticleEngine.AddParticleSystem(this);
                } else {
                    ParticleEngine.RemoveParticleSystem(this);
                }
            }
        }

        private ParticleEmitter emitter;
        public ParticleEmitter Emitter {
            get => emitter;
            set {
                emitter = value;
                hasEmitter = emitter != null;
            }
        }

        public void AddModule(ParticleModule module)
        {
            Modules.Add(module);
            if (Modules.Count > 0) {
                HasModules = true;
            }
            module.Initialize();
        }

        public void RemoveModule(ParticleModule module)
        {
            Modules.Remove(module);
            if (Modules.Count < 1) {
                HasModules = false;
            }
        }

        public void ClearParticles()
        {
            for (int i = ActiveParticles.Count - 1; i >= 0; i--) {
                ActiveParticles.Array[i].Enabled = false;
                if (HasModules) {
                    for (int ii = 0; ii < Modules.Count; ii++) {
                        Modules[ii].ParticleUnhook(ActiveParticles.Array[i]);
                    }
                }
            }
        }

        public void ReturnToPool(Particle particle)
        {
            if (particle.InPool)
                return;

            if (HasModules) {
                for (int i = 0; i < Modules.Count; i++) {
                    Modules[i].ParticleUnhook(particle);
                }
            }
            ParticleEngine.ReturnParticle(particle);
            ActiveParticles.Remove(particle);
        }

        private Particle TakeFromPool()
        {
            Particle p = ParticleEngine.GetParticle();
            if (p == null) 
                return null;

            p.ParticleSystem = this;
            p.EmitterPosition = Position;
            p.EmitterRotation = Rotation;
            if (HasModules) {
                for (int i = 0; i < Modules.Count; i++) {
                    Modules[i].ParticleHook(p);
                }
            }

            ActiveParticles.Add(p);
            return p;
        }

        public void Emit(int amount)
        {
            if (!enabled || !IsVisible)
                return;

            for (int i = 0; i < amount; i++) {
                GenerateParticle();
            }
        }

        public void EmitOne()
        {
            if (!enabled || !IsVisible)
                return;

            GenerateParticle();
        }

        public void Dispose()
        {
            ((IAssetConsumer) this).DereferenceAssets();
        }

        public virtual Particle GenerateParticle()
        {
            return TakeFromPool();
        }

        public void UpdateThreaded(float deltaTime)
        {
            for (int i = 0; i < ActiveParticles.Count; i++) {
                ActiveParticles.Array[i].Update(deltaTime);
            }
            if (HasModules) {
                for (int i = 0; i < Modules.Count; i++) {
                    Modules[i].UpdateThreaded(deltaTime);
                }
            }
        }

        public void UpdateVisibility() {
            if (BoundsSize.HasValue) {
                Point sizeValue = BoundsSize.Value;
                Rectangle bounds = new Rectangle((int)Position.X - sizeValue.X / 2, (int)Position.Y - sizeValue.Y / 2, sizeValue.X, sizeValue.Y);
                IsVisible = false;
                foreach (Camera2D camera in Core.Rendering.cameras) {
                    if (camera.ViewBounds.Intersects(bounds)) {
                        IsVisible = true;
                    }
                }
            } else {
                IsVisible = true;
            }
        }

        public ParticleSystem(bool enabledAtStart = true, Point? boundsSize = null, int initialCapacity = 1024)
        {
            BoundsSize = boundsSize;
            Position = Vector2.Zero;
            Rotation = 0f;
            ActiveParticles = new QuickList<Particle>(initialCapacity);
            if (enabledAtStart) {
                Enabled = true;
            }
        }
    }

}
