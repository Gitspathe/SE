using System;
using System.Runtime.InteropServices;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;
using Random = SE.Utility.Random;
using Vector4 = System.Numerics.Vector4;

#if MONOGAME
using Microsoft.Xna.Framework;
#endif

namespace SEParticles
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Particle
    {
        public Vector2 Position;
        public Vector2 Scale;
        public Vector2 Direction;         // Direction the particle travels in
        public Vector4 Color;             // H, S, L, A
        public float Mass;                // Used for repel and attract type functionality
        public float Speed;
        public float SpriteRotation;      // Sprite rotation
        public float InitialLife;
        public float TimeAlive;
#if MONOGAME
        public Rectangle SourceRectangle; // Texture source rectangle.
#endif

        public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Particle));

        public static Particle Default 
            => new Particle(Vector2.Zero, Vector2.One, Vector4.Zero, 0f, 1.0f);

        public Particle(Vector2 position, Vector2 scale, Vector4 color, float spriteRotation, float timeAlive)
        { 
            Position = position;
            Scale = scale;
            Direction = Vector2.Zero;
            Color = color;
            Mass = 0.0f;
            Speed = 0.0f;
            SpriteRotation = spriteRotation;
            TimeAlive = timeAlive;
            InitialLife = timeAlive;
#if MONOGAME
            SourceRectangle = Rectangle.Empty;
#endif
        }
    }
}
