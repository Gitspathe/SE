using System;

namespace SEParticles.Modules
{
    /// <summary>
    /// An object which modifies particles.
    /// </summary>
    public abstract class ParticleModule
    {
        protected internal Emitter Emitter {
            get => emitter;
            set {
                if (emitter != null)
                    throw new InvalidOperationException("Individual particle modules may only be added to one emitter.");

                emitter = value ?? throw new NullReferenceException();
            }
        }
        private Emitter emitter;

        /// <summary>Determines if the module is processed by it's emitter.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Called when the module is added to an emitter.
        /// </summary>
        public virtual void OnInitialize() { }

        /// <summary>
        /// Notifies the module of the indexes of new particles. Called before <see cref="OnUpdate"/>.
        /// </summary>
        /// <param name="particlesIndex">Span of new particles indexes.</param>
        public virtual void OnParticlesActivated(Span<int> particlesIndex) { }

        /// <summary>
        /// Updates the particle module.
        /// </summary>
        /// <param name="deltaTime">Time in seconds elapsed since last update.</param>
        /// <param name="arrayPtr">Pointer to an array of particles.</param>
        /// <param name="length">Length of the particle array.</param>
        public abstract unsafe void OnUpdate(float deltaTime, Particle* arrayPtr, int length);

        /// <summary>
        /// Copies the particle module.
        /// </summary>
        /// <returns>A deep copy.</returns>
        public abstract ParticleModule DeepCopy();
    }
}
