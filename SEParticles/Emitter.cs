﻿using System;
using System.Collections.Generic;
using System.Text;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace SEParticles
{
    public unsafe class Emitter
    {
        public QuickList<ParticleProcessor> Processors = new QuickList<ParticleProcessor>();
        public Particle[] Particles;

        public Vector4 StartColor = Vector4.One;
        public float StartRotation = 0f;
        public Vector2 StartScale = new Vector2(1.0f, 1.0f);
        public float Life = 2.0f;
        public Vector2 Position;

        private int lastActiveIndex;

        public Span<Particle> ActiveParticles => new Span<Particle>(Particles, 0, lastActiveIndex);

        public void Update(float deltaTime)
        {
            Span<Particle> particles = ActiveParticles;
            int size = particles.Length;
            fixed (Particle* ptr = particles) {
                Particle* particle = ptr;
                for (int i = 0; i < size; i++) {
                    particle->TimeAlive += deltaTime;
                    if (particle->Active && particle->TimeAlive >= Life) {
                        DeactivateParticle(i, particle);
                    }
                    particle++;
                }
            }

            ParticleProcessor[] processors = Processors.Array;
            Span<Particle> activeParticles = ActiveParticles;
            for (int i = 0; i < Processors.Count; i++) {
                processors[i].Update(deltaTime, activeParticles);
            }

            // Update RGBA color.
            particles = activeParticles;
            size = particles.Length;
            fixed (Particle* ptr = particles) {
                Particle* particle = ptr;
                for (int i = 0; i < size; i++) {
                    particle->ColorRGBA = particle->Color.ToRgba();
                    particle++;
                }
            }
        }

        public void DeactivateParticle(int index, Particle* particle)
        {
            particle->Active = false;
            lastActiveIndex--;
            if (lastActiveIndex < 0)
                lastActiveIndex = 0;
            if (index != lastActiveIndex)
                Particles[index] = Particles[lastActiveIndex];
        }

        public void Emit(int amount = 1)
        {
            // TODO: Emitter shapes.
            for (int i = 0; i < amount; i++) {
                fixed (Particle* particle = &Particles[lastActiveIndex++]) {
                    particle->Position = Position; // Temp, replace when adding emitter shapes.
                    particle->Rotation = StartRotation;
                    particle->Color = StartColor;
                    particle->Scale = StartScale;
                    particle->TimeAlive = 0.0f;
                    particle->InitialLife = Life;
                    particle->Active = true;
                    particle->GenerateSeed();
                }

                int lenMinusOne = Particles.Length - 1;
                if (lastActiveIndex > lenMinusOne) {
                    lastActiveIndex = lenMinusOne;
                }
            }
        }

        public void AddProcessor(ParticleProcessor processor)
        {
            Processors.Add(processor);
            processor.Emitter = this;
        }

        public Emitter(int capacity = 4096)
        {
            Position = Vector2.Zero;
            Particles = new Particle[capacity];
            for (int i = 0; i < capacity; i++) {
                Particles[i] = Particle.Default;
            }
        }
    }
}
