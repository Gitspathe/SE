using System.Numerics;

namespace SEParticles.Shapes
{
    /// <summary>
    /// Represents a shape.
    /// </summary>
    public interface IShape
    {
        /// <summary>Approximate center of the shape.</summary>
        Vector2 Center { get; set; }
    }

    /// <summary>
    /// Represents an object which controls the starting offset and direction of newly activated particles.
    /// </summary>
    public interface IEmitterShape : IShape
    {
        /// <summary>
        /// Calculates a position/offset and rotation for a new particle.
        /// </summary>
        /// <param name="position">Position for the new particle.</param>
        /// <param name="velocity">Velocity for the new particle.</param>
        /// <param name="uniformRatio">Ratio for uniform emission.</param>
        void Get(out Vector2 position, out Vector2 velocity, float uniformRatio);
    }

    /// <summary>
    /// Represents a shape which can be intersected. Used for area modules.
    /// </summary>
    public interface IIntersectable : IShape
    {
        /// <summary>
        /// Tests if a point intersects the shape.
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>True if the point is within the shape.</returns>
        bool Intersects(Vector2 point);
        /// <summary>
        /// Tests if a rectangle of given bounds intersects the shape.
        /// </summary>
        /// <param name="bounds">Bounds of the rectangle, where X=X, Y=Y, Z=Width, W=Height.</param>
        /// <returns>True if the rectangle intersects the shape.</returns>
        bool Intersects(Vector4 bounds);
    }

    public enum EmissionDirection
    {
        None,
        In,
        Out
    }
}
