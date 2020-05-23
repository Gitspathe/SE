using System;
using System.Numerics;
using SE.Core.Extensions;
using SE.Engine.Utility;
using SE.Utility;
using Random = SE.Utility.Random;
using static SEParticles.ParticleMath;

namespace SEParticles.Modules
{
    public unsafe class ColorModule : ParticleModule
    {
        private Configuration config;
        private Vector4[] rand;

        public ColorModule()
        {
            config = new Configuration();
        }

        public void SetConstant(Vector4 val)
        {
            config.Start = val;
            config.TransitionType = TransitionType.Constant;
        }

        public void SetLerp(Vector4 start, Vector4 end)
        {
            config.Start = start;
            config.End = end;
            config.TransitionType = TransitionType.Lerp;
        }

        public void SetCurve(Curve H, Curve S, Curve L, Curve A)
        {
            config.CurveH = H;
            config.CurveS = S;
            config.CurveL = L;
            config.CurveA = A;
            config.TransitionType = TransitionType.Curve;
        }

        public void SetRandomConstant(Vector4 min, Vector4 max)
        {
            if (min.X > max.X)
                Swap(ref min.X, ref max.X);
            if (min.Y > max.Y)
                Swap(ref min.Y, ref max.Y);
            if (min.Z > max.Z)
                Swap(ref min.Z, ref max.Z);
            if (min.W > max.W)
                Swap(ref min.W, ref max.W);

            config.Start = min;
            config.End = max;
            config.TransitionType = TransitionType.RandomConstant;
            RegenerateRandom();
        }

        public void SetRandomCurve(Curve H, Curve S, Curve L, Curve A)
        {
            config.CurveH = H;
            config.CurveS = S;
            config.CurveL = L;
            config.CurveA = A;
            config.TransitionType = TransitionType.RandomCurve;
            RegenerateRandom();
        }

        public override ParticleModule DeepCopy()
        {
            return new ColorModule {
                config = config.DeepCopy()
            };
        }

        public override void OnInitialize()
        {
            RegenerateRandom();
        }

        private void RegenerateRandom()
        {
            if (!config.IsRandom || Emitter == null) 
                return;

            rand = new Vector4[Emitter.ParticlesLength];
            for (int i = 0; i < rand.Length; i++) {
                rand[i] = new Vector4(
                    Random.Next(0.0f, 1.0f),
                    Random.Next(0.0f, 1.0f),
                    Random.Next(0.0f, 1.0f),
                    Random.Next(0.0f, 1.0f));
            }
        }

        public override void OnParticlesActivated(Span<int> particlesIndex)
        {
            if (config.IsRandom) {
                for (int i = 0; i < particlesIndex.Length; i++) {
                    rand[particlesIndex[i]] = new Vector4(
                        Random.Next(0.0f, 1.0f),
                        Random.Next(0.0f, 1.0f),
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
                        particle->Color = config.Start;
                        particle++;
                    }
                } break;
                case TransitionType.Lerp: {
                    for (int i = 0; i < size; i++) {
                        Vector4 color = Vector4.Lerp(
                            config.Start,
                            config.End,
                            particle->TimeAlive / particle->InitialLife);

                        particle->Color = color;
                        particle++;
                    }
                } break;
                case TransitionType.Curve: {
                    for (int i = 0; i < size; i++) {
                        float lifeRatio = particle->TimeAlive / particle->InitialLife;
                        Vector4 color = new Vector4(
                            config.CurveH.Evaluate(lifeRatio),
                            config.CurveS.Evaluate(lifeRatio), 
                            config.CurveL.Evaluate(lifeRatio),
                            config.CurveA.Evaluate(lifeRatio)
                        );
                        particle->Color = color;
                        particle++;
                    }
                } break;
                case TransitionType.RandomConstant: {
                    for (int i = 0; i < size; i++) {
                        Vector4 randData = rand[i];
                        float colorH = Between(config.Start.X, config.End.X, randData.X);
                        float colorS = Between(config.Start.Y, config.End.Y, randData.Y);
                        float colorL = Between(config.Start.Z, config.End.Z, randData.Z);
                        float colorA = Between(config.Start.W, config.End.W, randData.W);
                        particle->Color = new Vector4(colorH, colorS, colorL, colorA);

                        particle++;
                    }
                } break;
                case TransitionType.RandomCurve: {
                    for (int i = 0; i < size; i++) {
                        Vector4 randData = rand[i];
                        float colorH = config.CurveH.Evaluate(randData.X);
                        float colorS = config.CurveS.Evaluate(randData.Y);
                        float colorL = config.CurveL.Evaluate(randData.Z);
                        float colorA = config.CurveA.Evaluate(randData.W);
                        particle->Color = new Vector4(colorH, colorS, colorL, colorA);

                        particle++;
                    }
                } break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static ColorModule Constant(Vector4 val)
        {
            ColorModule module = new ColorModule();
            module.SetConstant(val);
            return module;
        }

        public static ColorModule Lerp(Vector4 start, Vector4 end)
        {
            ColorModule module = new ColorModule();
            module.SetLerp(start, end);
            return module;
        }

        public static ColorModule Curve(Curve H, Curve S, Curve L, Curve A)
        {
            ColorModule module = new ColorModule();
            module.SetCurve(H, S, L, A);
            return module;
        }

        public static ColorModule Curve(Curve4 curve)
        {
            ColorModule module = new ColorModule();
            module.SetCurve(curve.X, curve.Y, curve.Z, curve.W);
            return module;
        }

        public static ColorModule RandomConstant(Vector4 min, Vector4 max)
        {
            ColorModule module = new ColorModule();
            module.SetRandomConstant(min, max);
            return module;
        }

        public static ColorModule RandomCurve(Curve H, Curve S, Curve L, Curve A)
        {
            ColorModule module = new ColorModule();
            module.SetRandomCurve(H, S, L, A);
            return module;
        }

        public static ColorModule RandomCurve(Curve4 curve)
        {
            ColorModule module = new ColorModule();
            module.SetRandomCurve(curve.X, curve.Y, curve.Z, curve.W);
            return module;
        }

        public class Configuration
        {
            public TransitionType TransitionType;
            public Vector4 Start, End;
            public Curve CurveH, CurveS, CurveL, CurveA;

            public bool IsRandom => TransitionType == TransitionType.RandomConstant ||
                                    TransitionType == TransitionType.RandomCurve;

            public Configuration DeepCopy()
            {
                return new Configuration {
                    TransitionType = TransitionType,
                    Start = Start,
                    End = End,
                    CurveH = CurveH.Clone(),
                    CurveS = CurveS.Clone(),
                    CurveL = CurveL.Clone(),
                    CurveA = CurveA.Clone()
                };
            }
        }
    }
}
