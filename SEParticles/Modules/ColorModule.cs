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
        private Vector4[] startingColors;

        public ColorModule()
        {
            config = new Configuration();
        }

        public void SetLerp(Vector4 end)
        {
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

        public override ParticleModule DeepCopy()
        {
            return new ColorModule {
                config = config.DeepCopy()
            };
        }

        public override void OnInitialize()
        {
            startingColors = new Vector4[Emitter.ParticlesLength];
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
            for (int i = 0; i < particlesIndex.Length; i++) {
                int index = particlesIndex[i];
                startingColors[index] = Emitter.Particles[index].Color;
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
                case TransitionType.Lerp: {
                    for (int i = 0; i < size; i++) {
                        Vector4 color = Vector4.Lerp(
                            startingColors[i],
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

        public static ColorModule Lerp(Vector4 end)
        {
            ColorModule module = new ColorModule();
            module.SetLerp(end);
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

        public class Configuration
        {
            public TransitionType TransitionType;
            public Vector4 End;
            public Curve CurveH, CurveS, CurveL, CurveA;

            public bool IsRandom => TransitionType == TransitionType.RandomConstant ||
                                    TransitionType == TransitionType.RandomCurve;

            public Configuration DeepCopy()
            {
                return new Configuration {
                    TransitionType = TransitionType,
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
