using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Vector2 = System.Numerics.Vector2;

namespace SEParticles
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Particle
    {
        public Vector2 Position;
        public Vector2 Scale;
        public Vector4 Color; // RGBA
        public float Rotation;
        public float InitialLife;
        public float TTL;
        public bool Active;

        public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Particle));

        public static Particle Default 
            => new Particle(Vector2.Zero, Vector2.One, Vector4.One, 0f, 0f);

        public Particle(Vector2 position, Vector2 scale, Vector4 color, float rotation, float ttl)
        { 
            Position = position; 
            Scale = scale; 
            Color = color; 
            Rotation = rotation; 
            TTL = ttl;
            InitialLife = ttl;
            Active = false;
        }
    }
}
