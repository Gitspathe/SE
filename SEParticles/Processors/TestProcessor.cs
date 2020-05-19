using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SEParticles.Processors
{
    public unsafe class TestProcessor : ParticleProcessor
    {
        public Vector4 StartColor;
        public Vector4 EndColor;

        public override void Update(float deltaTime, Span<Particle> particles)
        {
            for (int i = 0; i < particles.Length; i++) {
                fixed (Particle* particle = &particles[i]) {
                    particle->Color = Vector4.Lerp(EndColor, StartColor, particle->TTL / particle->InitialLife);
                }
            }
        }

        public TestProcessor(Vector4 start, Vector4 end)
        {
            StartColor = start;
            EndColor = end;
        }
    }
}
