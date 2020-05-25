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
        public bool AbsoluteValue = false;

        private Vector2[] startScales;
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
            startScales = new Vector2[Emitter.ParticlesLength];
        }

        private void RegenerateRandom()
        {
            if (!IsRandom || Emitter == null) 
                return;

            rand = new float[Emitter.ParticlesLength];
        }

        public override void OnParticlesActivated(Span<int> particlesIndex)
        {
            for (int i = 0; i < particlesIndex.Length; i++) {
                int index = particlesIndex[i];
                startScales[index] = Emitter.Particles[index].Scale;
                if (IsRandom) {
                    rand[index] = Random.Next(0.0f, 1.0f);
                }
            }
        }

        public override void OnUpdate(float deltaTime, Particle* arrayPtr, int length)
        {
            Process(deltaTime, length, arrayPtr);
        }

        private void Process(float deltaTime, int size, Particle* ptr)
        {
            Particle* particle = ptr;
            switch (transitionType) {
                case Transition.Lerp: {
                    for (int i = 0; i < size; i++) {
                        float scale = Between(start, end, particle->TimeAlive / particle->InitialLife);
                        particle->Scale = AbsoluteValue
                            ? new Vector2(scale, scale)
                            : new Vector2(scale, scale) * startScales[i];
                        particle++;
                    }
                } break;
                case Transition.Curve: {
                    for (int i = 0; i < size; i++) {
                        float scale = curve.Evaluate(particle->TimeAlive / particle->InitialLife);
                        particle->Scale = AbsoluteValue
                            ? new Vector2(scale, scale)
                            : new Vector2(scale, scale) * startScales[i];
                        particle++;
                    }
                } break;
                case Transition.RandomCurve: {
                    for (int i = 0; i < size; i++) {
                        float scale = curve.Evaluate(rand[i]);
                        particle->Scale = AbsoluteValue
                            ? new Vector2(scale, scale)
                            : new Vector2(scale, scale) * startScales[i];
                        particle++;
                    }
                } break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override ParticleModule DeepCopy() 
            => new ScaleModule {
                transitionType = transitionType,
                start = start,
                end = end,
                curve = curve.Clone()
            };

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

        public static ScaleModule RandomCurve(Curve curve)
        {
            ScaleModule module = new ScaleModule();
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
