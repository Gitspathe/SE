using System.Numerics;

namespace SEParticles
{
    /// <summary>
    /// Represents an object which controls the starting offset and direction of newly activated particles.
    /// </summary>
    public interface IEmitterShape
    {
        /// <summary>
        /// Calculates a position/offset and rotation for a new particle.
        /// </summary>
        /// <param name="position">Position for the new particle.</param>
        /// <param name="velocity">Velocity for the new particle.</param>
        /// <param name="uniformRatio">Ratio for uniform emission.</param>
        void Get(out Vector2 position, out Vector2 velocity, float uniformRatio);
    }

    public enum EmissionDirection
    {
        None,
        In,
        Out
    }
}
