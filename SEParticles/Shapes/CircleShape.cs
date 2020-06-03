using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SE.Core.Extensions;
using Random = SE.Utility.Random;
using static SEParticles.ParticleMath;

namespace SEParticles.Shapes
{
    public class CircleShape : IEmitterShape, IIntersectable
    {
        public Vector2 Center { get; set; }

        public float Radius {
            get => radius;
            set => radius = Clamp(value, 1.0f, float.MaxValue);
        }
        private float radius;

        public EmissionDirection Direction;
        public bool EdgeOnly;
        public bool Uniform;

        public CircleShape() : this(32.0f) { }

        public CircleShape(float radius, EmissionDirection direction = EmissionDirection.None, bool edgeOnly = false, bool uniform = false)
        {
            Radius = radius;
            Direction = direction;
            EdgeOnly = edgeOnly;
            Uniform = uniform;
        }

        public void Get(out Vector2 position, out Vector2 velocity, float uniformRatio)
        {
            float distance = EdgeOnly 
                ? radius 
                : Random.Next(0.0f, Radius);
            float rotation = Uniform
                ? Between(-_PI, _PI, uniformRatio) 
                : Random.NextAngle();

            velocity = rotation.ToDirectionVector();
            position = Direction == EmissionDirection.In
                ? new Vector2(-velocity.X * distance, -velocity.Y * distance)
                : new Vector2(velocity.X * distance, velocity.Y * distance);

            if (Direction == EmissionDirection.None)
                velocity = Random.NextUnitVector();
        }

        public bool Intersects(Vector2 point)
        {
            float dx = Math.Abs(point.X - Center.X);
            float dy = Math.Abs(point.Y - Center.Y);
            float R = radius;

            if (dx > R)
                return false;
            if (dy > R)
                return false;
            if (dx + dy <= R)
                return true;

            return (dx * dx) + (dy * dy) <= (R * R);
        }

        public bool Intersects(Vector4 bounds)
        {
            Vector2 circleDistance;

            circleDistance.X = Math.Abs(Center.X - bounds.X);
            circleDistance.Y = Math.Abs(Center.Y - bounds.Y);

            if (circleDistance.X > (bounds.Z/2 + radius)) { return false; }
            if (circleDistance.Y > (bounds.W/2 + radius)) { return false; }

            if (circleDistance.X <= (bounds.Z/2)) { return true; } 
            if (circleDistance.Y <= (bounds.W/2)) { return true; }

            float foo = (circleDistance.X - bounds.Z / 2) * (circleDistance.X - bounds.Z / 2);
            float bar = (circleDistance.Y - bounds.W / 2) * (circleDistance.Y - bounds.W / 2);
            float cornerDistanceSq = foo + bar;

            return (cornerDistanceSq <= (radius * radius));
        }
    }
}
