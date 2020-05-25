using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using static SEParticles.ParticleMath;

namespace SEParticles.Modules
{
    public unsafe class AttractorModule : ParticleModule
    {
        public Vector2 Position = new Vector2(256.0f, 256.0f);
        public float MaxDistance = 4096.0f;
        public float MinDistance = 64.0f;

        public float Intensity = 10.0f;
        public float SpeedIncrease = 8.0f;

        public override void OnUpdate(float deltaTime, Span<Particle> particles)
        {
            fixed (Particle* ptr = particles) {
                Process(deltaTime, particles.Length, ptr);
            }
        }

        private void Process(float deltaTime, int size, Particle* ptr)
        {
            Particle* particle = ptr;
            for (int i = 0; i < size; i++) {
                float distance = (Position - particle->Position).Length();
                if (distance <= MaxDistance) {
                    float ratio = GetRatio(MaxDistance, MinDistance, distance);
                    float speedDelta = SpeedIncrease * ratio;
                    GetAngle(particle->Position, Position, out Vector2 direction, out float angle);

                    particle->Direction = Vector2.Lerp(particle->Direction, direction, ratio * Intensity * deltaTime);
                    particle->Rotation = Lerp(particle->Rotation, angle, ratio * Intensity * deltaTime);
                    particle->Speed += speedDelta;
                }
                particle++;
            }
        }

        private void GetAngle(Vector2 from, Vector2 to, out Vector2 direction, out float angle)
        {
            direction = Vector2.Normalize(to - from);
#if NETSTANDARD2_1
            angle = MathF.Atan2(-direction.X, direction.Y);
#else
            angle = (float)Math.Atan2(-direction.X, direction.Y);
#endif
        }

        public static AttractorModule Basic(Vector2 position, float minDistance, float maxDistance,
            float intensity = 10.0f, float speedIncrease = 0.0f) 
            => new AttractorModule {
                Position = position,
                MinDistance = minDistance,
                MaxDistance = maxDistance,
                Intensity = intensity,
                SpeedIncrease = speedIncrease
            };

        public override ParticleModule DeepCopy()
        {
            throw new NotImplementedException();
        }
    }
}
