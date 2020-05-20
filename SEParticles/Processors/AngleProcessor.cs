using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SE.Utility;

namespace SEParticles.Processors
{
    public unsafe class AngleProcessor : ParticleProcessor
    {
        private Configuration Config = new Configuration();

        public class Configuration
        {
            public TransitionType TransitionType;
            public float Start, End;
            public Curve Curve;

            public float constMin, constMax;
        }

        public void SetConstant(float val)
        {
            Config.Start = val;
            Config.TransitionType = TransitionType.Constant;
        }

        public void SetLerp(float start, float end)
        {
            Config.Start = start;
            Config.End = end;
            Config.TransitionType = TransitionType.Lerp;
        }

        public void SetCurve(Curve curve)
        {
            Config.Curve = curve;
            Config.TransitionType = TransitionType.Curve;
        }

        public void SetRandomConstant(float min, float max)
        {
            if (min > max)
                ParticleMath.Swap(ref min, ref max);

            Config.constMin = min;
            Config.constMax = max;
            Config.TransitionType = TransitionType.RandomConstant;
        }

        public void SetRandomCurve(Curve curve)
        {
            Config.Curve = curve;
            Config.TransitionType = TransitionType.RandomCurve;
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
                        particle->Rotation += Config.Start * deltaTime;
                        particle++;
                    }
                } break;
                case TransitionType.Lerp: {
                    Particle* particle = ptr;
                    for (int i = 0; i < size; i++) {
                        float angleDelta = ParticleMath.Lerp(
                            Config.Start,
                            Config.End,
                            particle->TimeAlive / particle->InitialLife);

                        particle->Rotation += angleDelta * deltaTime;
                        particle++;
                    }
                } break;
                case TransitionType.Curve: {
                    Particle* particle = ptr;
                    for (int i = 0; i < size; i++) {
                        float lifeRatio = particle->TimeAlive / particle->InitialLife;
                        float angleDelta = Config.Curve.Evaluate(lifeRatio);
                        particle->Rotation += angleDelta * deltaTime;
                        particle++;
                    }
                } break;
                case TransitionType.RandomConstant: {
                    Particle* particle = ptr;
                    for (int i = 0; i < size; i++) {
                        float randDelta = ParticleMath.Between(Config.constMin, Config.constMax, particle->Seed);
                        particle->Rotation += randDelta * deltaTime;
                        particle++;
                    }
                } break;
                case TransitionType.RandomCurve: {
                    Particle* particle = ptr;
                    for (int i = 0; i < size; i++) {
                        float randDelta = Config.Curve.Evaluate(particle->Seed);
                        particle->Rotation += randDelta * deltaTime;
                        particle++;
                    }
                } break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static AngleProcessor Constant(float val)
        {
            AngleProcessor processor = new AngleProcessor();
            processor.SetConstant(val);
            return processor;
        }

        public static AngleProcessor Lerp(float start, float end)
        {
            AngleProcessor processor = new AngleProcessor();
            processor.SetLerp(start, end);
            return processor;
        }

        public static AngleProcessor Curve(Curve curve)
        {
            AngleProcessor processor = new AngleProcessor();
            processor.SetCurve(curve);
            return processor;
        }

        public static AngleProcessor RandomConstant(float min, float max)
        {
            AngleProcessor processor = new AngleProcessor();
            processor.SetRandomConstant(min, max);
            return processor;
        }

        public static AngleProcessor RandomCurve(Curve curve)
        {
            AngleProcessor processor = new AngleProcessor();
            processor.SetRandomCurve(curve);
            return processor;
        }

        public AngleProcessor()
        {
            Config = new Configuration();
        }
    }
}
