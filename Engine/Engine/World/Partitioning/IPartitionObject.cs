using System;
using System.Numerics;

namespace SE.World.Partitioning
{
    /// <summary>
    /// Represents an object which can be added to the spatial partitioning system.
    /// </summary>
    public interface IPartitionObject
    {
        /// <summary>Object's position.</summary>
        Vector2 PartitionPosition { get; }
        /// <summary>Which type the object should be stored as in the spatial partitioning system.</summary>
        Type PartitionObjectType { get; }
        /// <summary>Should never be modified manually!</summary>
        PartitionTile CurrentPartitionTile { get; set; }
    }

    /// <summary>
    /// Extended interface of <see cref="IPartitionObject"/>, which supports callback functions for when an object is added or removed
    /// from the spatial partitioning system. Should only be used when needed, due to performance concerns.
    /// </summary>
    public interface IPartitionObjectExtended : IPartitionObject
    {
        /// <summary>Called after the object has been added to the spatial partitioning system.</summary>
        /// <param name="tile">Tile which the object was added to.</param>
        void InsertedIntoPartition(PartitionTile tile);
        /// <summary>Called after the object has been removed from the spatial partitioning system.</summary>
        /// <param name="tile">Tile which the object was removed from.</param>
        void RemovedFromPartition(PartitionTile tile);
    }
}
