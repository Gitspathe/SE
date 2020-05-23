using System;
using System.Collections.Generic;
using System.Text;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;
using Random = SE.Utility.Random;

#if MONOGAME
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endif

namespace SEParticles
{
    public unsafe class Emitter
    {
        public QuickList<ParticleModule> Modules = new QuickList<ParticleModule>();

        // TODO: Random start color, rotation, etc.
        public Vector4 StartColor = Vector4.One;
        public float StartRotation;
        public Vector2 StartScale = new Vector2(1.0f, 1.0f);
        public float Life = 2.0f;
        public Vector2 Position;
#if MONOGAME
        public Texture2D Texture;
#endif

        private Particle[] particles;
        private int[] newParticles;

        private int numActive;
        private int numNew;

        public int ParticlesLength => particles.Length;
        public Span<Particle> ActiveParticles => new Span<Particle>(particles, 0, numActive);
        public Span<int> NewParticlesIndex => new Span<int>(newParticles, 0, numNew);

        public void Update(float deltaTime)
        {
            ParticleModule[] modules = Modules.Array;

            // Inform the modules of newly activated particles.
            for (int i = 0; i < Modules.Count; i++) {
                modules[i].OnParticlesActivated(NewParticlesIndex);
            }
            numNew = 0;

            // Update the particles, and deactivate those whose TTL <= 0.
            Span<Particle> activeParticles = ActiveParticles;
            int size = activeParticles.Length;
            fixed (Particle* ptr = activeParticles) {
                Particle* particle = ptr;
                for (int i = 0; i < size; i++) {
                    particle->TimeAlive += deltaTime;
                    if (particle->TimeAlive >= Life) {
                        particle->TimeAlive = Life;
                        DeactivateParticle(i);
                    }
                    particle++;
                }
            }

            // Update the modules.
            activeParticles = ActiveParticles;
            for (int i = 0; i < Modules.Count; i++) {
                modules[i].OnUpdate(deltaTime, activeParticles);
            }
        }

        public void DeactivateParticle(int index)
        {
            if (index != numActive) {
                particles[index] = particles[numActive];
            }
            numActive--;
        }

        public void Emit(int amount = 1)
        {
            // TODO: Emitter shapes.
            for (int i = 0; i < amount; i++) {
                if (numActive + 1 > particles.Length - 1)
                    return;
                
                numActive++;
                fixed (Particle* particle = &particles[numActive]) {
                    particle->Position = Position; // Temp, replace when adding emitter shapes.
                    particle->Rotation = StartRotation;
                    particle->Color = StartColor;
                    particle->Scale = StartScale;
                    particle->TimeAlive = 0.0f;
                    particle->InitialLife = Life;
#if MONOGAME
                    particle->SourceRectangle = new Rectangle(0, 0, Texture.Width, Texture.Height);
#endif
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

        public Emitter(int capacity = 512)
        {
            Position = Vector2.Zero;
            particles = new Particle[capacity];
            newParticles = new int[capacity];
            for (int i = 0; i < capacity; i++) {
                particles[i] = Particle.Default;
            }
        }

        public Emitter DeepCopy()
        {
            Emitter emitter = new Emitter(particles.Length) {
                StartColor = StartColor,
                StartRotation = StartRotation,
                StartScale = StartScale,
                Life = Life,
                Position = Position
            };
            for (int i = 0; i < Modules.Count; i++) {
                emitter.AddModule(Modules.Array[i].DeepCopy());
            }
            return emitter;
        }
    }
}
