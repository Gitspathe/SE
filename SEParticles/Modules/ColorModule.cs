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
        private Vector4[] rand;
        private Vector4[] startingColors;
        private Vector4[] randomEndingColors;

        private Transition TransitionType;
        private Vector4 End1;
        private Vector4 End2;
        private Curve CurveH, CurveS, CurveL, CurveA;

        private bool IsRandom => TransitionType == Transition.RandomLerp;

        public void SetLerp(Vector4 end)
        {
            End1 = end;
            TransitionType = Transition.Lerp;
        }

        public void SetCurve(Curve H, Curve S, Curve L, Curve A)
        {
            CurveH = H;
            CurveS = S;
            CurveL = L;
            CurveA = A;
            TransitionType = Transition.Curve;
        }

        public void SetRandomLerp(Vector4 min, Vector4 max)
        {
            if (min.X > max.X)
                Swap(ref min.X, ref max.X);
            if (min.Y > max.Y)
                Swap(ref min.Y, ref max.Y);
            if (min.Z > max.Z)
                Swap(ref min.Z, ref max.Z);
            if (min.W > max.W)
                Swap(ref min.W, ref max.W);

            End1 = min;
            End2 = max;
            TransitionType = Transition.RandomLerp;
        }

        public override ParticleModule DeepCopy() 
            => new ColorModule {
                TransitionType = TransitionType,
                End1 = End1,
                End2 = End2,
                CurveH = CurveH.Clone(),
                CurveS = CurveS.Clone(),
                CurveL = CurveL.Clone(),
                CurveA = CurveA.Clone()
            };

        public override void OnInitialize()
        {
            startingColors = new Vector4[Emitter.ParticlesLength];
            RegenerateRandom();
        }

        private void RegenerateRandom()
        {
            if (!IsRandom || Emitter == null) 
                return;

            rand = new Vector4[Emitter.ParticlesLength];
            randomEndingColors = new Vector4[Emitter.ParticlesLength];
        }

        public override void OnParticlesActivated(Span<int> particlesIndex)
        {
            for (int i = 0; i < particlesIndex.Length; i++) {
                int index = particlesIndex[i];
                startingColors[index] = Emitter.Particles[index].Color;
                if (!IsRandom) 
                    continue;

                rand[particlesIndex[i]] = new Vector4(
                    Random.Next(0.0f, 1.0f),
                    Random.Next(0.0f, 1.0f),
                    Random.Next(0.0f, 1.0f),
                    Random.Next(0.0f, 1.0f));
                randomEndingColors[i] = new Vector4(
                    Between(End1.X, End2.X, rand[i].X),
                    Between(End1.Y, End2.Y, rand[i].Y),
                    Between(End1.Z, End2.Z, rand[i].Z),
                    Between(End1.W, End2.W, rand[i].W));
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
            switch (TransitionType) {
                case Transition.Lerp: {
                    for (int i = 0; i < size; i++) {
                        Vector4 color = Vector4.Lerp(
                            startingColors[i],
                            End1,
                            particle->TimeAlive / particle->InitialLife);
                        particle->Color = color;
                        particle++;
                    }
                } break;
                case Transition.Curve: {
                    for (int i = 0; i < size; i++) {
                        float lifeRatio = particle->TimeAlive / particle->InitialLife;
                        Vector4 color = new Vector4(
                            CurveH.Evaluate(lifeRatio),
                            CurveS.Evaluate(lifeRatio), 
                            CurveL.Evaluate(lifeRatio),
                            CurveA.Evaluate(lifeRatio));
                        particle->Color = color;
                        particle++;
                    }
                } break;
                case Transition.RandomLerp: {
                    for (int i = 0; i < size; i++) {
                        Vector4 color = Vector4.Lerp(
                            startingColors[i],
                            randomEndingColors[i],
                            particle->TimeAlive / particle->InitialLife);
                        particle->Color = color;
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

        public static ColorModule RandomLerp(Vector4 min, Vector4 max)
        {
            ColorModule module = new ColorModule();
            module.SetRandomLerp(min, max);
            return module;
        }

        private enum Transition
        {
            Lerp,
            Curve,
            RandomLerp
        }
    }
}
