using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Random = SE.Utility.Random;

namespace SEParticles.Shapes
{
    public class PointShape : EmitterShape
    {
        public override void Get(out Vector2 position, out float rotation, float uniformRatio)
        {
            position = Vector2.Zero;
            rotation = Random.NextAngle();
        }
    }
}
