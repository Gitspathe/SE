using SE.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SEParticles.Shapes;
using static SEParticles.ParticleMath;

namespace SEParticles.AreaModules
{
    public class ForceModule : AreaModule
    {
        public float MaxDistance = 4096.0f;
        public float MinDistance = 128.0f;

        public float Intensity = 25.0f;
        public float SpeedIncrease = 12.0f;

        private Mode mode = Mode.Attract;

        public ForceModule(IIntersectable shape, Vector2? position = null) : base(shape, position) { }
        
        public override unsafe void ProcessParticles(float deltaTime, Particle* particles, int length)
        {
            Particle* tail = particles + length;
            for (Particle* particle = particles; particle < tail; particle++) {
                if (!Shape.Intersects(particle->Position)) 
                    continue;

                float distance = (Position - particle->Position).Length();
                if (!(distance <= MaxDistance)) 
                    continue;

                float ratio = GetRatio(MaxDistance, MinDistance, distance);
                float speedDelta = SpeedIncrease * ratio;
                GetAngle(particle->Position, Position, out Vector2 direction, out float angle);
                if(mode == Mode.Repel)
                    direction = -direction;

                particle->Direction = Vector2.Lerp(particle->Direction, direction, ratio * Intensity * deltaTime);
                particle->SpriteRotation = Lerp(particle->SpriteRotation, angle, ratio * Intensity * deltaTime);
                particle->Speed += speedDelta;
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

        public static ForceModule Attract(IIntersectable shape, Vector2 position, float minDistance, float maxDistance, 
            float intensity = 10.0f, float speedIncrease = 10.0f) 
            => new ForceModule(shape) {
                Position = position,
                MinDistance = minDistance,
                MaxDistance = maxDistance,
                Intensity = intensity,
                SpeedIncrease = speedIncrease,
                mode = Mode.Attract
            };

        public static ForceModule Repel(IIntersectable shape, Vector2 position, float minDistance, float maxDistance, 
            float intensity = 10.0f, float speedIncrease = 10.0f) 
            => new ForceModule(shape) {
                Position = position,
                MinDistance = minDistance,
                MaxDistance = maxDistance,
                Intensity = intensity,
                SpeedIncrease = speedIncrease,
                mode = Mode.Repel
            };

        private enum Mode
        {
            Attract,
            Repel
        }
    }
}
