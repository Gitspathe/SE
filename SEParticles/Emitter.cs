using System;
using System.Collections.Generic;
using SE.Utility;
using SEParticles.Shapes;
using System.Buffers;
using SEParticles.AreaModules;
using SEParticles.Modules;
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
    /// <summary>
    /// Core of the particle engine. Emitters hold a buffer of particles, and a list of <see cref="ParticleModule"/>.
    /// </summary>
    public unsafe class Emitter : IDisposable
    {
        public IAdditionalData AdditionalData;
        public IEmitterShape Shape;

        public EmitterConfig Config;

        public Vector2 Size;
#if MONOGAME
        public Texture2D Texture;
        public Rectangle StartRect; // TODO. Support sprite-sheet animations + random start sprite-sheet source rect.
#endif

        internal HashSet<AreaModule> AreaModules = new HashSet<AreaModule>();
        internal int ParticleEngineIndex = -1;
        internal Particle[] Particles;
        
        private QuickList<ParticleModule> modules = new QuickList<ParticleModule>();
        private int[] newParticles;
        private int numActive;
        private int numNew;

        public Vector2 Position {
            get => Shape.Center;
            set => Shape.Center = value;
        }

        public Vector4 Bounds { get; private set; } // X, Y, Width, Height
        public int ParticlesLength => Particles.Length;
        public ref Particle GetParticle(int index) => ref Particles[index];
        public Span<Particle> ActiveParticles => new Span<Particle>(Particles, 0, numActive);
        private Span<int> NewParticleIndexes => new Span<int>(newParticles, 0, numNew);

        /// <summary>Controls whether or not the emitter will emit new particles.</summary>
        public bool EmissionEnabled { get; set; } = true;

        /// <summary>Enabled/Disabled state. Disabled emitters are not updated or registered to the particle engine.</summary>
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

        public Emitter(Vector2 size, int capacity = 2048, IEmitterShape shape = null)
        {
            if (!ParticleEngine.Initialized)
                throw new InvalidOperationException("Particle engine has not been initialized. Call ParticleEngine.Initialize() first.");

            Config = new EmitterConfig();
            Shape = shape ?? new PointShape();
            Size = size;

            Position = Vector2.Zero;
            switch (ParticleEngine.AllocationMode) {
                case ParticleAllocationMode.ArrayPool:
                    Particles = ArrayPool<Particle>.Shared.Rent(capacity);
                    newParticles = ArrayPool<int>.Shared.Rent(capacity);
                    break;
                case ParticleAllocationMode.Array:
                    Particles = new Particle[capacity];
                    newParticles = new int[capacity];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            for (int i = 0; i < capacity; i++) {
                Particles[i] = Particle.Default;
            }
        }

        public Emitter(int capacity = 2048, IEmitterShape shape = null) 
            : this(new Vector2(512.0f, 512.0f), capacity, shape) { }

        internal void Update(float deltaTime)
        {
            ParticleModule[] modules = this.modules.Array;

            // Update shape center.
            Shape.Center = Position;
            Bounds = new Vector4(Position.X - (Size.X / 2), Position.Y - (Size.Y / 2), Size.X, Size.Y);

            // Inform the modules of newly activated particles.
            for (int i = 0; i < this.modules.Count; i++) {
                modules[i].OnParticlesActivated(NewParticleIndexes);
            }
            numNew = 0;

            fixed (Particle* ptr = Particles) {
                Particle* tail = ptr + numActive;
                int i = 0;

                // Update the particles, and deactivate those whose TTL <= 0.
                for (Particle* particle = ptr; particle < tail; particle++, i++) {
                    particle->TimeAlive += deltaTime;
                    if (particle->TimeAlive >= particle->InitialLife) {
                        DeactivateParticleInternal(particle, i);
                    }
                }

                // Update enabled modules.
                for (i = 0; i < this.modules.Count; i++) {
                    if(!modules[i].Enabled)
                        continue;

                    modules[i].OnUpdate(deltaTime, ptr, numActive);
                }

                // Update the area modules influencing this emitter.
                foreach (AreaModule areaModule in AreaModules) {
                    areaModule.ProcessParticles(deltaTime, ptr, numActive);
                }

                // Update particle positions.
                tail = ptr + numActive;
                for (Particle* particle = ptr; particle < tail; particle++) {
                    particle->Position += particle->Direction * particle->Speed * deltaTime;
                }
            }
        }

        internal void CheckParticleIntersections(QuickList<Particle> list, AreaModule areaModule)
        {
            Span<Particle> particles = ActiveParticles;
            for (int i = 0; i < particles.Length; i++) {
                ref Particle particle = ref particles[i];
                if (areaModule.Shape.Intersects(particle.Position)) {
                    list.Add(particle);
                }
            }
        }

        /// <summary>
        /// Deactivates a particles at specified index.
        /// </summary>
        /// <param name="index">Index of particle to deactivate.</param>
        public void DeactivateParticle(int index)
        {
            if (index > numActive || index < 0)
                throw new IndexOutOfRangeException(nameof(index));

            numActive--;
            if (index != numActive) {
                Particles[index] = Particles[numActive];
            }
        }

        // Higher performance deactivation function with fewer safety checks.
        private void DeactivateParticleInternal(Particle* particle, int index)
        {
            particle->Position = new Vector2(-float.MaxValue, -float.MaxValue);
            numActive--;
            if (index != numActive) {
                Particles[index] = Particles[numActive];
            }
        }

        public void Emit(int amount = 1)
        {
            if (!enabled || !EmissionEnabled)
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

        public T GetModule<T>() where T : ParticleModule 
            => (T) GetModule(typeof(T));

        public ParticleModule GetModule(Type moduleType)
        {
            if (moduleType == null)
                throw new ArgumentNullException(nameof(moduleType));
            if (!moduleType.IsSubclassOf(typeof(ParticleModule)))
                throw new ArgumentException("Type is not of particle module.", nameof(moduleType));

            ParticleModule[] arr = modules.Array;
            for (int i = 0; i < modules.Count; i++) {
                if (arr[i].GetType() == moduleType) {
                    return arr[i];
                }
            }
            return null;
        }

        public IEnumerable<T> GetModules<T>() where T : ParticleModule 
            => GetModules(typeof(T)) as IEnumerable<T>;

        public IEnumerable<ParticleModule> GetModules(Type moduleType)
        {
            if (moduleType == null)
                throw new ArgumentNullException(nameof(moduleType));
            if (!moduleType.IsSubclassOf(typeof(ParticleModule)))
                throw new ArgumentException("Type is not of particle module.", nameof(moduleType));

            QuickList<ParticleModule> tmpModules = new QuickList<ParticleModule>();
            ParticleModule[] arr = modules.Array;
            for (int i = 0; i < modules.Count; i++) {
                if (arr[i].GetType() == moduleType) {
                    tmpModules.Add(arr[i]);
                }
            }
            return tmpModules;
        }

        public void AddModule(ParticleModule module)
        {
            if (module == null)
                throw new ArgumentException(nameof(module));

            modules.Add(module);
            module.Emitter = this;
            module.OnInitialize();
        }

        public Emitter DeepCopy()
        {
            Emitter emitter = new Emitter(Particles.Length) {
                Config = Config.DeepCopy(),
                Position = Position
            };
            for (int i = 0; i < modules.Count; i++) {
                emitter.AddModule(modules.Array[i].DeepCopy());
            }
            return emitter;
        }

        public void Dispose()
        {
            Enabled = false;
            switch (ParticleEngine.AllocationMode) {
                case ParticleAllocationMode.ArrayPool:
                    ArrayPool<Particle>.Shared.Return(Particles, true);
                    ArrayPool<int>.Shared.Return(newParticles, true);
                    break;
                case ParticleAllocationMode.Array:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
