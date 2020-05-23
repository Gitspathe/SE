using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SE.Core.Extensions;
using Random = SE.Utility.Random;
using static SEParticles.ParticleMath;

namespace SEParticles.Shapes
{
    public class CircleShape : EmitterShape
    {
        public float Radius;
        public EmissionDirection Direction;
        public bool EdgeOnly;
        public bool Uniform;

        public override void Get(out Vector2 position, out float rotation, float uniformRatio)
        {
            float distance = EdgeOnly 
                ? Radius 
                : Random.Next(0.0f, Radius);
            rotation = Uniform 
                ? Between(-_PI, _PI, uniformRatio) 
                : Random.NextAngle();

            Vector2 heading = rotation.ToDirectionVector();
            position = Direction == EmissionDirection.In
                ? new Vector2(-heading.X * distance, -heading.Y * distance)
                : new Vector2(heading.X * distance, heading.Y * distance);

            if (Direction == EmissionDirection.None)
                rotation = Random.NextAngle();
        }

        public CircleShape(float radius, EmissionDirection direction = EmissionDirection.None, bool edgeOnly = false, bool uniform = false)
        {
            Radius = radius;
            Direction = direction;
            EdgeOnly = edgeOnly;
            Uniform = uniform;
        }
    }

    public enum EmissionDirection
    {
        None,
        In,
        Out
    }
}
