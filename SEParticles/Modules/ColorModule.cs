using System;
using System.Numerics;

namespace SEParticles.Modules
{
    public unsafe class ColorModule : ParticleModule
    {
        public Vector4 StartColor;
        public Vector4 EndColor;

        public override void Update(float deltaTime, Span<Particle> particles)
        {
            fixed (Particle* ptr = particles) {
                Process(deltaTime, particles.Length, ptr);
            }
        }

        // TODO.
        private void Process(float deltaTime, int size, Particle* ptr)
        {
            Particle* particle = ptr;
            for (int i = 0; i < size; i++) {
                float seed = particle->Seed;

                // Testing.
                //if (seed < 0.5f) {
                    particle->Color = Vector4.Lerp(new Vector4(0.0f, 1.0f, 0.5f, 1.0f), new Vector4(360.0f, 1.0f, 0.667f, 0.0f), particle->TimeAlive / particle->InitialLife);
                //} else {
                //    particle->Color = Vector4.Lerp(StartColor, EndColor, particle->TimeAlive / particle->InitialLife);
                //}
                //particle->Color = Vector4.Lerp(StartColor, EndColor, particle->TimeAlive / particle->InitialLife);
                particle++;
            }
        }

        public ColorModule(Vector4 start, Vector4 end)
        {
            StartColor = start;
            EndColor = end;
        }
    }
}
