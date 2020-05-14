using System.Numerics;
using SE.Core.Extensions;

namespace SE.Physics
{
    public struct Ray2D
    {
        /// <summary>Starting position of the ray.</summary>
        public Vector2 Position;

        /// <summary>Destination point of the ray.</summary>
        public Vector2 Destination;

        /// <summary>Direction the ray is traveling.</summary>
        public Vector2 Direction;

        /// <summary>How far the ray will travel.</summary>
        public float Distance;

        /// <summary>Creates a new Ray instance.</summary>
        /// <param name="position">Starting position.</param>
        /// <param name="direction">Direction for the Ray.</param>
        /// <param name="distance">How far the Ray will travel.</param>
        public Ray2D(Vector2 position, Vector2 direction, float distance)
        {
            Position = position;
            Direction = direction;
            Distance = distance;
            Destination = position + (direction * distance);
        }

        /// <summary>Creates a new Ray instance.</summary>
        /// <param name="position">Starting position.</param>
        /// <param name="direction">Direction for the Ray in degrees.</param>
        /// <param name="distance">How far the Ray will travel.</param>
        public Ray2D(Vector2 position, float direction, float distance)
        {
            Position = position;
            Direction = direction.GetRotationVector(distance);
            Distance = distance;
            Destination = position + (Direction * distance);
        }

        /// <summary>Creates a new Ray instance.</summary>
        /// <param name="position">Starting position.</param>
        /// <param name="destination">Destination position.</param>
        public Ray2D(Vector2 position, Vector2 destination)
        {
            Position = position;
            Direction = position - destination;
            Distance = (position - destination).Length();
            Destination = destination;
        }
    }
}
