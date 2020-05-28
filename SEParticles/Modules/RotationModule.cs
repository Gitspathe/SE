using System;
using SE.Utility;
using Random = SE.Utility.Random;
using static SEParticles.ParticleMath;

namespace SEParticles.Modules
{
    public unsafe class RotationModule : ParticleModule
    {
        private Configuration config;
        private float[] rand;

        public RotationModule()
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

        public override void OnUpdate(float deltaTime, Particle* arrayPtr, int length)
        {
            Particle* tail = arrayPtr + length;
            int i = 0;

            switch (config.TransitionType) {
                case TransitionType.Constant: {
                    for (Particle* particle = arrayPtr; particle < tail; particle++) {
                        particle->Rotation += config.Start * deltaTime;
                    }
                } break;
                case TransitionType.Lerp: {
                    for (Particle* particle = arrayPtr; particle < tail; particle++) {
                        float angleDelta = ParticleMath.Lerp(
                            config.Start,
                            config.End,
                            particle->TimeAlive / particle->InitialLife);

                        particle->Rotation += angleDelta * deltaTime;
                    }
                } break;
                case TransitionType.Curve: {
                    for (Particle* particle = arrayPtr; particle < tail; particle++) {
                        particle->Rotation += config.Curve.Evaluate(particle->TimeAlive / particle->InitialLife) * deltaTime;
                    }
                } break;
                case TransitionType.RandomConstant: {
                    for (Particle* particle = arrayPtr; particle < tail; particle++, i++) {
                        particle->Rotation += Between(config.Start, config.End, rand[i]) * deltaTime;
                    }
                } break;
                case TransitionType.RandomCurve: {
                    for (Particle* particle = arrayPtr; particle < tail; particle++, i++) {
                        particle->Rotation += config.Curve.Evaluate(rand[i]) * deltaTime;
                    }
                } break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override ParticleModule DeepCopy()
        {
            return new RotationModule {
                config = config.DeepCopy(),
            };
        }

        public static RotationModule Constant(float val)
        {
            RotationModule module = new RotationModule();
            module.SetConstant(val);
            return module;
        }

        public static RotationModule Lerp(float start, float end)
        {
            RotationModule module = new RotationModule();
            module.SetLerp(start, end);
            return module;
        }

        public static RotationModule Curve(Curve curve)
        {
            RotationModule module = new RotationModule();
            module.SetCurve(curve);
            return module;
        }

        public static RotationModule RandomConstant(float min, float max)
        {
            RotationModule module = new RotationModule();
            module.SetRandomConstant(min, max);
            return module;
        }

        public static RotationModule RandomCurve(Curve curve)
        {
            RotationModule module = new RotationModule();
            module.SetRandomCurve(curve);
            return module;
        }

        public class Configuration
        {
            public TransitionType TransitionType;
            public float Start, End;
            public Curve Curve;

            public bool IsRandom => TransitionType == TransitionType.RandomConstant ||
                                    TransitionType == TransitionType.RandomCurve;

            public Configuration DeepCopy() 
                => new Configuration {
                    TransitionType = TransitionType,
                    Start = Start,
                    End = End,
                    Curve = Curve.Clone()
                };
        }
    }
}
