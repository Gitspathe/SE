using System;
using System.Numerics;
using System.Runtime.InteropServices;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;

namespace SEParticles
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Particle
    {
        public Vector2 Position;
        public Vector2 Scale;
        public Vector4 Color; // H, S, L, A
        public Vector4 ColorRGBA;
        public float Rotation;
        public float InitialLife;
        public float TimeAlive;
        public float Seed;
        public bool Active;

        public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Particle));

        public void GenerateSeed() 
            => Seed = SE.Utility.Random.Next(0.0f, 1.0f);

        public static Particle Default 
            => new Particle(Vector2.Zero, Vector2.One, Vector4.One, 0f, 0f);

        public Particle(Vector2 position, Vector2 scale, Vector4 color, float rotation, float timeAlive)
        { 
            Position = position; 
            Scale = scale; 
            Color = color; 
            ColorRGBA = Vector4.Zero;
            Rotation = rotation; 
            TimeAlive = timeAlive;
            InitialLife = timeAlive;
            Active = false;
            Seed = SE.Utility.Random.Next(0.0f, 1.0f);
        }
    }
}
