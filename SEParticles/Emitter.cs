using System;
using System.Collections.Generic;
using SE.Utility;
using SEParticles.Shapes;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;
using Random = SE.Utility.Random;
using static SEParticles.ParticleMath;

#if MONOGAME
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endif

namespace SEParticles
{
    public unsafe class Emitter
    {
        public QuickList<ParticleModule> Modules = new QuickList<ParticleModule>();
        public IAdditionalData AdditionalData;
        public IEmitterShape Shape;

        public EmitterConfig Config;
        public Vector2 Position;
#if MONOGAME
        public Texture2D Texture;
        public Rectangle StartRect; // TODO. Support sprite-sheet animations + random start sprite-sheet source rect.
#endif

        internal int ParticleEngineIndex = -1;
        internal Particle[] Particles;
        
        private int[] newParticles;
        private int numActive;
        private int numNew;

        public int ParticlesLength => Particles.Length;
        public ref Particle GetParticle(int index) => ref Particles[index];
        public Span<Particle> ActiveParticles => new Span<Particle>(Particles, 0, numActive);
        public Span<int> NewParticlesIndex => new Span<int>(newParticles, 0, numNew);

        public bool Enabled {
            get => enabled;
            set {
                enabled = value;
                if (enabled)
                    ParticleEngine.AddEmitter(this);
                else
                    ParticleEngine.RemoveEmitter(this);
            }
        }
        private bool enabled;

        public void Update(float deltaTime)
        {
            ParticleModule[] modules = Modules.Array;
            int i;

            // Inform the modules of newly activated particles.
            for (i = 0; i < Modules.Count; i++) {
                modules[i].OnParticlesActivated(NewParticlesIndex);
            }
            numNew = 0;

            fixed (Particle* ptr = Particles) {
                Particle* end = ptr + numActive;
                i = 0;

                // Update the particles, and deactivate those whose TTL <= 0.
                for (Particle* particle = ptr; particle < end; particle++, i++) {
                    particle->TimeAlive += deltaTime;
                    if (particle->TimeAlive >= particle->InitialLife) {
                        DeactivateParticle(i);
                    }
                }

                // Update the modules.
                for (i = 0; i < Modules.Count; i++) {
                    modules[i].OnUpdate(deltaTime, ptr, numActive);
                }

                // Update particle positions.
                end = ptr + numActive;
                for (Particle* particle = ptr; particle < end; particle++) {
                    particle->Position += particle->Direction * particle->Speed * deltaTime;
                }
            }
        }

        public void DeactivateParticle(int index)
        {
            numActive--;
            if (index != numActive) {
                Particles[index] = Particles[numActive];
            }
        }

        public void Emit(int amount = 1)
        {
            if (!enabled)
                return;

            for (int i = 0; i < amount; i++) {
                if (numActive + 1 > Particles.Length)
                    return;

                fixed (Particle* particle = &Particles[numActive++]) {
                    Shape.Get(out particle->Position, out particle->Direction, (float)i / amount);
                    particle->Position += Position;
                    particle->TimeAlive = 0.0f;
#if MONOGAME
                    particle->SourceRectangle = StartRect;
#endif
    
                    // Configure particle speed.
                    EmitterConfig.SpeedConfig speed = Config.Speed;
                    switch (Config.Color.StartValueType) {
                        case EmitterConfig.StartingValue.Normal: {
                            particle->Speed = speed.Min;
                        } break;
                        case EmitterConfig.StartingValue.Random: {
                            particle->Speed = Between(speed.Min, speed.Max, Random.Next());
                        } break;
                        case EmitterConfig.StartingValue.RandomCurve: {
                            particle->Speed = speed.Curve.Evaluate(Random.Next());
                        } break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    // Configure particle scale.
                    EmitterConfig.ScaleConfig scale = Config.Scale;
                    switch (Config.Color.StartValueType) {
                        case EmitterConfig.StartingValue.Normal: {
                            particle->Scale = scale.Min;
                        } break;
                        case EmitterConfig.StartingValue.Random: {
                            if (scale.TwoDimensions) {
                                particle->Scale = new Vector2(
                                    Between(scale.Min.X, scale.Max.X, Random.Next()),
                                    Between(scale.Min.Y, scale.Max.Y, Random.Next()));
                            } else {
                                float s = Random.Next();
                                particle->Scale = new Vector2(
                                    Between(scale.Min.X, scale.Max.X, s),
                                    Between(scale.Min.Y, scale.Max.Y, s));
                            }
                        } break;
                        case EmitterConfig.StartingValue.RandomCurve: {
                            if (scale.TwoDimensions) {
                                particle->Scale = new Vector2(
                                    scale.Curve.X.Evaluate(Random.Next()),
                                    scale.Curve.Y.Evaluate(Random.Next()));
                            } else {
                                float s = Random.Next();
                                particle->Scale = new Vector2(
                                    scale.Curve.X.Evaluate(s),
                                    scale.Curve.Y.Evaluate(s));
                            }
                        } break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    // Configure particle color.
                    EmitterConfig.ColorConfig color = Config.Color;
                    switch (Config.Color.StartValueType) {
                        case EmitterConfig.StartingValue.Normal: {
                            particle->Color = color.Min;
                        } break;
                        case EmitterConfig.StartingValue.Random: {
                            particle->Color = new Vector4(
                                Between(color.Min.X, color.Max.X, Random.Next()),
                                Between(color.Min.Y, color.Max.Y, Random.Next()),
                                Between(color.Min.Z, color.Max.Z, Random.Next()),
                                Between(color.Min.W, color.Max.W, Random.Next()));
                        } break;
                        case EmitterConfig.StartingValue.RandomCurve: {
                            particle->Color = new Vector4(
                                color.Curve.X.Evaluate(Random.Next()), 
                                color.Curve.Y.Evaluate(Random.Next()),
                                color.Curve.Z.Evaluate(Random.Next()),
                                color.Curve.W.Evaluate(Random.Next()));
                        } break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    // Configure particle life.
                    EmitterConfig.LifeConfig life = Config.Life;
                    switch (Config.Color.StartValueType) {
                        case EmitterConfig.StartingValue.Normal: {
                            particle->InitialLife = life.Min;
                        } break;
                        case EmitterConfig.StartingValue.Random: {
                            particle->InitialLife = Between(life.Min, life.Max, Random.Next());
                        } break;
                        case EmitterConfig.StartingValue.RandomCurve: {
                            particle->InitialLife = life.Curve.Evaluate(Random.Next());
                        } break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                newParticles[numNew++] = numActive;
            }
        }

        public void AddModule(ParticleModule module)
        {
            Modules.Add(module);
            module.Emitter = this;
            module.OnInitialize();
        }

        public Emitter(int capacity = 2048, IEmitterShape shape = null)
        {
            Config = new EmitterConfig();
            Shape = shape ?? new PointShape();

            Position = Vector2.Zero;
            Particles = new Particle[capacity];
            newParticles = new int[capacity];
            for (int i = 0; i < capacity; i++) {
                Particles[i] = Particle.Default;
            }
        }

        public Emitter DeepCopy()
        {
            Emitter emitter = new Emitter(Particles.Length) {
                Config = Config.DeepCopy(),
                Position = Position
            };
            for (int i = 0; i < Modules.Count; i++) {
                emitter.AddModule(Modules.Array[i].DeepCopy());
            }
            return emitter;
        }

    }
}
