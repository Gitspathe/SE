using System;
using System.Numerics;
using SE.Core.Extensions;
using SE.Utility;
using Random = SE.Utility.Random;

namespace SEParticles.Modules
{
    public unsafe class SpeedModule : ParticleModule
    {
        public bool AbsoluteValue = false;
        
        private float[] rand;

        private Transition transitionType;
        private float start, end;
        private Curve curve;

        private bool IsRandom => transitionType == Transition.RandomCurve;

        public void SetLerp(float start, float end)
        {
            this.start = start;
            this.end = end;
            transitionType = Transition.Lerp;
        }

        public void SetCurve(Curve curve)
        {
            this.curve = curve;
            transitionType = Transition.Curve;
        }

        public void SetRandomCurve(Curve curve)
        {
            this.curve = curve;
            transitionType = Transition.RandomCurve;
            RegenerateRandom();
        }

        public override void OnInitialize()
        {
            RegenerateRandom();
        }

        private void RegenerateRandom()
        {
            if (!IsRandom || Emitter == null) 
                return;

            rand = new float[Emitter.ParticlesLength];
        }

        public override void OnParticlesActivated(Span<int> particlesIndex)
        {
            if (IsRandom) {
                for (int i = 0; i < particlesIndex.Length; i++) {
                    rand[particlesIndex[i]] = Random.Next(0.0f, 1.0f);
                }
            }
        }

        public override void OnUpdate(float deltaTime, Particle* arrayPtr, int length)
        {
            Particle* tail = arrayPtr + length;

            switch (transitionType) {
                case Transition.Lerp: {
                    for (Particle* particle = arrayPtr; particle < tail; particle++) {
                        float velocity = ParticleMath.Lerp(start, end, particle->TimeAlive / particle->InitialLife);
                        particle->Speed = AbsoluteValue
                            ? velocity
                            : particle->Speed + (velocity * deltaTime);
                    }
                } break;
                case Transition.Curve: {
                    for (Particle* particle = arrayPtr; particle < tail; particle++) {
                        float velocity = curve.Evaluate(particle->TimeAlive / particle->InitialLife);
                        particle->Speed = AbsoluteValue
                            ? velocity
                            : particle->Speed + (velocity * deltaTime);
                    }
                } break;
                case Transition.RandomCurve: {
                    int i = 0;
                    for (Particle* particle = arrayPtr; particle < tail; particle++, i++) {
                        float velocity = curve.Evaluate(rand[i]);
                        particle->Speed = AbsoluteValue 
                            ? velocity
                            : particle->Speed + (velocity * deltaTime);
                    }
                } break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override ParticleModule DeepCopy()
            => new SpeedModule {
                transitionType = transitionType,
                start = start,
                end = end,
                curve = curve.Clone()
            };

        public static SpeedModule Lerp(float start, float end)
        {
            SpeedModule module = new SpeedModule();
            module.SetLerp(start, end);
            return module;
        }

        public static SpeedModule Curve(Curve curve)
        {
            SpeedModule module = new SpeedModule();
            module.SetCurve(curve);
            return module;
        }

        public static SpeedModule RandomCurve(Curve curve)
        {
            SpeedModule module = new SpeedModule();
            module.SetRandomCurve(curve);
            return module;
        }

        private enum Transition
        {
            Lerp,
            Curve,
            RandomCurve
        }
    }
}
