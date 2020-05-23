using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SE.Engine.Utility;
using SE.Utility;
using Random = SE.Utility.Random;
using static SEParticles.ParticleMath;

namespace SEParticles.Modules
{
    public unsafe class ScaleModule : ParticleModule
    {
        private Configuration config;
        private float[] rand;

        public ScaleModule()
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

        private void Process(float deltaTime, int size, Particle* ptr)
        {
            Particle* particle = ptr;
            switch (config.TransitionType) {
                case TransitionType.Constant: {
                    for (int i = 0; i < size; i++) {
                        particle->Scale = new Vector2(config.Start, config.Start);
                        particle++;
                    }
                } break;
                case TransitionType.Lerp: {
                    for (int i = 0; i < size; i++) {
                        float scale = Between(config.Start, config.End, particle->TimeAlive / particle->InitialLife);
                        particle->Scale = new Vector2(scale, scale);
                        particle++;
                    }
                } break;
                case TransitionType.Curve: {
                    for (int i = 0; i < size; i++) {
                        float scale = config.Curve.Evaluate(particle->TimeAlive / particle->InitialLife);
                        particle->Scale = new Vector2(scale, scale);
                        particle++;
                    }
                } break;
                case TransitionType.RandomConstant: {
                    for (int i = 0; i < size; i++) {
                        float scale = Between(
                            config.Start, 
                            config.End, 
                            rand[i]);

                        particle->Scale = new Vector2(scale, scale);
                        particle++;
                    }
                } break;
                case TransitionType.RandomCurve: {
                    for (int i = 0; i < size; i++) {
                        float scale = config.Curve.Evaluate(rand[i]);
                        particle->Scale = new Vector2(scale, scale);
                        particle++;
                    }
                } break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override ParticleModule DeepCopy()
        {
            return new ScaleModule {
                config = config.DeepCopy()
            };
        }

        public static ScaleModule Constant(float val)
        {
            ScaleModule module = new ScaleModule();
            module.SetConstant(val);
            return module;
        }

        public static ScaleModule Lerp(float start, float end)
        {
            ScaleModule module = new ScaleModule();
            module.SetLerp(start, end);
            return module;
        }

        public static ScaleModule Curve(Curve curve)
        {
            ScaleModule module = new ScaleModule();
            module.SetCurve(curve);
            return module;
        }

        public static ScaleModule RandomConstant(float min, float max)
        {
            ScaleModule module = new ScaleModule();
            module.SetRandomConstant(min, max);
            return module;
        }

        public static ScaleModule RandomCurve(Curve curve)
        {
            ScaleModule module = new ScaleModule();
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
}
