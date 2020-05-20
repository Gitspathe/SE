using System;
using SE.Utility;

namespace SEParticles.Modules
{
    public unsafe class AngleModule : ParticleModule
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

        public static AngleModule Constant(float val)
        {
            AngleModule module = new AngleModule();
            module.SetConstant(val);
            return module;
        }

        public static AngleModule Lerp(float start, float end)
        {
            AngleModule module = new AngleModule();
            module.SetLerp(start, end);
            return module;
        }

        public static AngleModule Curve(Curve curve)
        {
            AngleModule module = new AngleModule();
            module.SetCurve(curve);
            return module;
        }

        public static AngleModule RandomConstant(float min, float max)
        {
            AngleModule module = new AngleModule();
            module.SetRandomConstant(min, max);
            return module;
        }

        public static AngleModule RandomCurve(Curve curve)
        {
            AngleModule module = new AngleModule();
            module.SetRandomCurve(curve);
            return module;
        }

        public AngleModule()
        {
            Config = new Configuration();
        }
    }
}
