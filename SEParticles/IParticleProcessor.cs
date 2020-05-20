using System;
using System.Collections.Generic;
using System.Text;

namespace SEParticles
{
    public abstract class ParticleProcessor
    {
        protected internal Emitter Emitter;
        public abstract void Update(float deltaTime, Span<Particle> particles);
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
