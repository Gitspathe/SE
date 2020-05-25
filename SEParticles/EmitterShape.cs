using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SEParticles
{
    public abstract class EmitterShape
    {
        /// <summary>
        /// Calculates a position/offset and rotation for a new particle.
        /// </summary>
        /// <param name="position">Position for the new particle.</param>
        /// <param name="velocity">Velocity for the new particle.</param>
        /// <param name="uniformRatio">Ratio for uniform emission.</param>
        public abstract void Get(out Vector2 position, out Vector2 velocity, float uniformRatio);
    }
}
