using System;
using System.Numerics;
using SE.Core.Extensions;
using SE.Utility;
using Random = SE.Utility.Random;
using static SEParticles.ParticleMath;

namespace SEParticles.Modules
{
    public unsafe class ForwardVelocityModule : ParticleModule
    {
        private Configuration config;
        private float[] rand;

        public ForwardVelocityModule()
        {
            config = new Configuration();
        }

        public void SetConstant(float val)
        {
            config.Start = val;
            config.TransitionType = TransitionType.Constant;
        }

        public void SetLerp(float start, float end)
        {
            config.Start = start;
            config.End = end;
            config.TransitionType = TransitionType.Lerp;
        }

        public void SetCurve(Curve curve)
        {
            config.Curve = curve;
            config.TransitionType = TransitionType.Curve;
        }

        public void SetRandomConstant(float min, float max)
        {
            if (min > max)
                Swap(ref min, ref max);

            config.Start = min;
            config.End = max;
            config.TransitionType = TransitionType.RandomConstant;
            RegenerateRandom();
        }

        public void SetRandomCurve(Curve curve)
        {
            config.Curve = curve;
            config.TransitionType = TransitionType.RandomCurve;
            RegenerateRandom();
        }

        public override void OnInitialize()
        {
            RegenerateRandom();
        }

        private void RegenerateRandom()
        {
            if (!config.IsRandom || Emitter == null) 
                return;

            rand = new float[Emitter.ParticlesLength];
            for (int i = 0; i < rand.Length; i++) {
                rand[i] = Random.Next(0.0f, 1.0f);
            }
        }

        public override void OnParticlesActivated(Span<int> particlesIndex)
        {
            if (config.IsRandom) {
                for (int i = 0; i < particlesIndex.Length; i++) {
                    rand[particlesIndex[i]] = Random.Next(0.0f, 1.0f);
                }
            }
        }

        public override void OnUpdate(float deltaTime, Span<Particle> particles)
        {
            fixed (Particle* ptr = particles) {
                Process(deltaTime, particles.Length, ptr);
            }
        }

        public override ParticleModule DeepCopy()
        {
            return new ForwardVelocityModule {
                config = config.DeepCopy()
            };
        }

        private void Process(float deltaTime, int size, Particle* ptr)
        {
            Particle* particle = ptr;
            switch (config.TransitionType) {
                case TransitionType.Constant: {
                    for (int i = 0; i < size; i++) {
                        Vector2 velocityVector = particle->Heading * config.Start;
                        particle->Position += velocityVector * deltaTime;
                        particle++;
                    }
                } break;
                case TransitionType.Lerp: {
                    for (int i = 0; i < size; i++) {
                        float velocity = ParticleMath.Lerp(
                            config.Start,
                            config.End,
                            particle->TimeAlive / particle->InitialLife);

                        Vector2 velocityVector = particle->Heading * velocity;
                        particle->Position += velocityVector * deltaTime;
                        particle++;
                    }
                } break;
                case TransitionType.Curve: {
                    for (int i = 0; i < size; i++) {
                        float lifeRatio = particle->TimeAlive / particle->InitialLife;
                        float velocity = config.Curve.Evaluate(lifeRatio);
                        Vector2 velocityVector = particle->Heading * velocity;
                        particle->Position += velocityVector * deltaTime;
                        particle++;
                    }
                } break;
                case TransitionType.RandomConstant: {
                    for (int i = 0; i < size; i++) {
                        float velocity = Between(
                            config.Start, 
                            config.End, 
                            rand[i]);

                        Vector2 velocityVector = particle->Heading * velocity; 
                        particle->Position += velocityVector * deltaTime;
                        particle++;
                    }
                } break;
                case TransitionType.RandomCurve: {
                    for (int i = 0; i < size; i++) {
                        float velocity = config.Curve.Evaluate(rand[i]);
                        Vector2 velocityVector = particle->Heading * velocity;
                        particle->Position += velocityVector * deltaTime;

                        particle++;
                    }
                } break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static ForwardVelocityModule Constant(float val)
        {
            ForwardVelocityModule module = new ForwardVelocityModule();
            module.SetConstant(val);
            return module;
        }

        public static ForwardVelocityModule Lerp(float start, float end)
        {
            ForwardVelocityModule module = new ForwardVelocityModule();
            module.SetLerp(start, end);
            return module;
        }

        public static ForwardVelocityModule Curve(Curve curve)
        {
            ForwardVelocityModule module = new ForwardVelocityModule();
            module.SetCurve(curve);
            return module;
        }

        public static ForwardVelocityModule RandomConstant(float min, float max)
        {
            ForwardVelocityModule module = new ForwardVelocityModule();
            module.SetRandomConstant(min, max);
            return module;
        }

        public static ForwardVelocityModule RandomCurve(Curve curve)
        {
            ForwardVelocityModule module = new ForwardVelocityModule();
            module.SetRandomCurve(curve);
            return module;
        }
    }

    public class Configuration
    {
        public TransitionType TransitionType;
        public float Start, End;
        public Curve Curve;

        public bool IsRandom => TransitionType == TransitionType.RandomConstant ||
                                TransitionType == TransitionType.RandomCurve;

        public Configuration DeepCopy()
        {
            return new Configuration {
                TransitionType = TransitionType,
                Start = Start,
                End = End,
                Curve = Curve.Clone()
            };
        }
    }
}
