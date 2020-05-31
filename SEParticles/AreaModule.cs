using System;
using System.Collections.Generic;
using System.Text;

namespace SEParticles
{
    public abstract class AreaModule
    {
        internal bool AddedToEngine = false;
        internal HashSet<Emitter> AttachedEmitters = new HashSet<Emitter>();

        public bool Enabled {
            get => enabled;
            set {
                enabled = value;
                if (enabled)
                    ParticleEngine.AddAreaModule(this);
                else
                    ParticleEngine.RemoveAreaModule(this);
            }
        }
        private bool enabled = true;

        public IIntersectable Shape { get; set; }

        public AreaModule(IIntersectable shape)
        {
            Shape = shape;
        }

        public abstract unsafe void OnUpdate(float deltaTime, Particle* particles, int length);
    }

    public class AreaParticleModule : AreaModule
    {
        public override unsafe void OnUpdate(float deltaTime, Particle* particles, int length)
        {
            Particle* tail = particles + length;
            for (Particle* particle = particles; particle < tail; particle++) {
                if (Shape.Intersects(particle->Position)) {
                    particle->Color.X = 0.0f;
                }
            }
        }

        public AreaParticleModule(IIntersectable shape) : base(shape) { }
    }
}
