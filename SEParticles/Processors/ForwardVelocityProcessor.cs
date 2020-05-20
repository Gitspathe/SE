using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using SE.Core.Extensions;
using SE.Utility;

namespace SEParticles.Processors
{
    public unsafe class ForwardVelocityProcessor : ParticleProcessor
    {
        public Configuration Config = new Configuration();

        public class Configuration
        {
            internal TransitionType TransitionType;
            internal float Start, End;
            internal Curve Curve;

            public void SetConstant(float val)
            {
                Start = val;
                TransitionType = TransitionType.Constant;
            }

            public void SetLerp(float start, float end)
            {
                Start = start;
                End = end;
                TransitionType = TransitionType.Lerp;
            }

            public void SetCurve(Curve curve)
            {
                Curve = curve;
                TransitionType = TransitionType.Curve;
            }
        }

        public override void Update(float deltaTime, Span<Particle> particles)
        {
            fixed (Particle* ptr = particles) {
                Process(deltaTime, particles.Length, ptr);
            }
        }

        private void Process(float deltaTime, int size, Particle* ptr)
        {
            switch (Config.TransitionType) {
                case TransitionType.Constant: {
                    Particle* particle = ptr;
                    for (int i = 0; i < size; i++) {
                        Vector2 velocityVector = particle->Rotation.ToDirectionVector() * Config.Start;
                        particle->Position += velocityVector * deltaTime;
                        particle++;
                    }
                } break;
                case TransitionType.Lerp: {
                    Particle* particle = ptr;
                    for (int i = 0; i < size; i++) {
                        float velocity = ParticleMath.Lerp(
                            Config.Start,
                            Config.End,
                            particle->TimeAlive / particle->InitialLife);

                        Vector2 velocityVector = particle->Rotation.ToDirectionVector() * velocity;
                        particle->Position += velocityVector * deltaTime;
                        particle++;
                    }
                } break;
                case TransitionType.Curve: {
                    Particle* particle = ptr;
                    for (int i = 0; i < size; i++) {
                        float lifeRatio = particle->TimeAlive / particle->InitialLife;
                        float velocity = Config.Curve.Evaluate(lifeRatio);
                        Vector2 velocityVector = particle->Rotation.ToDirectionVector() * velocity;
                        particle->Position += velocityVector * deltaTime;
                        particle++;
                    }
                } break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public ForwardVelocityProcessor(float constantValue)
        {
            Config.SetConstant(constantValue);
        }

        public ForwardVelocityProcessor(float lerpStart, float lerpEnd)
        {
            Config.SetLerp(lerpStart, lerpEnd);
        }

        public ForwardVelocityProcessor(Curve curve)
        {
            Config.SetCurve(curve);
        }
    }
}
