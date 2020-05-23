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
    public unsafe class Scale2Module : ParticleModule
    {
        private Configuration config;
        private Vector2[] rand;

        public Scale2Module()
        {
            config = new Configuration();
        }

        public void SetConstant(Vector2 val)
        {
            config.Start = val;
            config.TransitionType = TransitionType.Constant;
        }

        public void SetLerp(Vector2 start, Vector2 end)
        {
            config.Start = start;
            config.End = end;
            config.TransitionType = TransitionType.Lerp;
        }

        public void SetCurve(Curve x, Curve y)
        {
            config.CurveX = x;
            config.CurveY = y;
            config.TransitionType = TransitionType.Curve;
        }

        public void SetRandomConstant(Vector2 min, Vector2 max)
        {
            if (min.X > max.X)
                Swap(ref min.X, ref max.X);
            if (min.Y > max.Y)
                Swap(ref min.Y, ref max.Y);

            config.Start = min;
            config.End = max;
            config.TransitionType = TransitionType.RandomConstant;
            RegenerateRandom();
        }

        public void SetRandomCurve(Curve x, Curve y)
        {
            config.CurveX = x;
            config.CurveY = y;
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

            rand = new Vector2[Emitter.ParticlesLength];
            for (int i = 0; i < rand.Length; i++) {
                rand[i] = new Vector2(Random.Next(0.0f, 1.0f), Random.Next(0.0f, 1.0f));
            }
        }

        public override void OnParticlesActivated(Span<int> particlesIndex)
        {
            if (config.IsRandom) {
                for (int i = 0; i < particlesIndex.Length; i++) {
                    rand[particlesIndex[i]] = new Vector2(
                        Random.Next(0.0f, 1.0f),
                        Random.Next(0.0f, 1.0f));
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
                        particle->Scale = config.Start;
                        particle++;
                    }
                } break;
                case TransitionType.Lerp: {
                    for (int i = 0; i < size; i++) {
                        float lifeRatio = particle->TimeAlive / particle->InitialLife;
                        particle->Scale = new Vector2(
                            Between(config.Start.X, config.End.X, lifeRatio),
                            Between(config.Start.Y, config.End.Y, lifeRatio));

                        particle++;
                    }
                } break;
                case TransitionType.Curve: {
                    for (int i = 0; i < size; i++) {
                        float lifeRatio = particle->TimeAlive / particle->InitialLife;
                        particle->Scale = new Vector2(
                            config.CurveX.Evaluate(lifeRatio),
                            config.CurveY.Evaluate(lifeRatio));

                        particle++;
                    }
                } break;
                case TransitionType.RandomConstant: {
                    for (int i = 0; i < size; i++) {
                        Vector2 partcleSeed = rand[i];
                        particle->Scale = new Vector2(
                            Between(config.Start.X, config.End.X, partcleSeed.X),
                            Between(config.Start.Y, config.End.Y, partcleSeed.Y));

                        particle++;
                    }
                } break;
                case TransitionType.RandomCurve: {
                    for (int i = 0; i < size; i++) {
                        Vector2 partcleSeed = rand[i];
                        particle->Scale = new Vector2(
                            config.CurveX.Evaluate(partcleSeed.X),
                            config.CurveY.Evaluate(partcleSeed.Y));

                        particle++;
                    }
                } break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override ParticleModule DeepCopy()
        {
            return new Scale2Module {
                config = config.DeepCopy()
            };
        }

        public static Scale2Module Constant(Vector2 val)
        {
            Scale2Module module = new Scale2Module();
            module.SetConstant(val);
            return module;
        }

        public static Scale2Module Lerp(Vector2 start, Vector2 end)
        {
            Scale2Module module = new Scale2Module();
            module.SetLerp(start, end);
            return module;
        }

        public static Scale2Module Curve(Curve x, Curve y)
        {
            Scale2Module module = new Scale2Module();
            module.SetCurve(x, y);
            return module;
        }

        public static Scale2Module Curve(Curve2 curve)
        {
            Scale2Module module = new Scale2Module();
            module.SetCurve(curve.X, curve.Y);
            return module;
        }

        public static Scale2Module RandomConstant(Vector2 min, Vector2 max)
        {
            Scale2Module module = new Scale2Module();
            module.SetRandomConstant(min, max);
            return module;
        }

        public static Scale2Module RandomCurve(Curve x, Curve y)
        {
            Scale2Module module = new Scale2Module();
            module.SetRandomCurve(x, y);
            return module;
        }

        public static Scale2Module RandomCurve(Curve2 curve)
        {
            Scale2Module module = new Scale2Module();
            module.SetRandomCurve(curve.X, curve.Y);
            return module;
        }

        public class Configuration
        {
            public TransitionType TransitionType;
            public Vector2 Start, End;
            public Curve CurveX, CurveY;

            public bool IsRandom => TransitionType == TransitionType.RandomConstant ||
                                    TransitionType == TransitionType.RandomCurve;

            public Configuration DeepCopy()
            {
                return new Configuration {
                    TransitionType = TransitionType,
                    Start = Start,
                    End = End,
                    CurveX = CurveX.Clone(),
                    CurveY = CurveY.Clone()
                };
            }
        }
    }
}
