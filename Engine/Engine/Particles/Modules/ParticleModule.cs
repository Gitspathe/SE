using System.Collections.Generic;

namespace SE.Particles.Modules
{

    /// <summary>
    /// Provides extendability to ParticleSystems. Attach modules to ParticleSystems for extra effects - lighting, for example.
    /// </summary>
    public class ParticleModule
    {

        public bool Enabled { get; set; }

        /// <summary>List of particles, which are currently active in the ParticleSystem.</summary>
        public List<Particle> Particles = new List<Particle>(256);

        protected ParticleSystem ParticleSystem;

        public virtual void Initialize()
        {

        }

        /// <summary>
        /// Called whenever a new particle is active.
        /// </summary>
        /// <param name="particle">Particle which was just created activated by the ParticleSystem.</param>
        public virtual void ParticleHook(Particle particle)
        {
            Particles.Add(particle);
        }

        /// <summary>
        /// Called whenever a particle is deactivated and pooled.
        /// </summary>
        /// <param name="particle">Particle which was deactivated by the ParticleSystem.</param>
        public virtual void ParticleUnhook(Particle particle)
        {
            Particles.Remove(particle);
        }

        /// <summary>
        /// Threaded update method, called each frame.
        /// </summary>
        public virtual void UpdateThreaded(float deltaTime) { }

        public ParticleModule(ParticleSystem particleSystem)
        {
            ParticleSystem = particleSystem;
        }
    }

}
