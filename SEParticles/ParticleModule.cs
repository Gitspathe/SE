using System;
using System.Collections.Generic;
using System.Text;

namespace SEParticles
{
    public abstract class ParticleModule
    {
        protected internal Emitter Emitter;

        public virtual void OnInitialize() { }
        public virtual void OnParticlesActivated(Span<int> particlesIndex) { }
        public abstract void OnUpdate(float deltaTime, Span<Particle> particles);
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
