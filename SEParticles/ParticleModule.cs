using System;
using System.Collections.Generic;
using System.Text;

namespace SEParticles
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

                emitter = value;
            }
        }
        private Emitter emitter;

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
        /// <param name="particles">Span of active particles.</param>
        public abstract void OnUpdate(float deltaTime, Span<Particle> particles);

        /// <summary>
        /// Copies the particle module.
        /// </summary>
        /// <returns>A deep copy.</returns>
        public abstract ParticleModule DeepCopy();
    }

    public enum TransitionType
    {
        Constant,
        Lerp,
        Curve,
        RandomConstant,
        RandomCurve
    }
}
