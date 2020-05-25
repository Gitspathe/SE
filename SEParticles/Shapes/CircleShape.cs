using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SE.Core.Extensions;
using Random = SE.Utility.Random;
using static SEParticles.ParticleMath;

namespace SEParticles.Shapes
{
    public class CircleShape : IEmitterShape
    {
        public float Radius;
        public EmissionDirection Direction;
        public bool EdgeOnly;
        public bool Uniform;

        public void Get(out Vector2 position, out Vector2 velocity, float uniformRatio)
        {
            float distance = EdgeOnly 
                ? Radius 
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

        public CircleShape(float radius, EmissionDirection direction = EmissionDirection.None, bool edgeOnly = false, bool uniform = false)
        {
            Radius = radius;
            Direction = direction;
            EdgeOnly = edgeOnly;
            Uniform = uniform;
        }
    }
}
