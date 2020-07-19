using SE.Core;
using System;
using System.Numerics;

namespace SE.World.Partitioning
{

    public interface IPartitionObject
    {
        void InsertIntoPartition();
        void RemoveFromPartition();
    }

    /// <summary>
    /// Represents an object which can be added to the spatial partitioning system.
    /// </summary>
    public interface IPartitionObject<T> : IPartitionObject where T : IPartitionObject<T>
    {
        /// <summary>Object's position.</summary>
        Vector2 PartitionPosition { get; }
        /// <summary>Should never be modified manually!</summary>
        PartitionTile<T> CurrentPartitionTile { get; set; }
    }

    /// <summary>
    /// Extended interface of <see cref="IPartitionObject"/>, which supports callback functions for when an object is added or removed
    /// from the spatial partitioning system. Should only be used when needed, due to performance concerns.
    /// </summary>
    public interface IPartitionObjectExtended<T> : IPartitionObject<T> where T : IPartitionObject<T>
    {
        /// <summary>Called after the object has been added to the spatial partitioning system.</summary>
        /// <param name="tile">Tile which the object was added to.</param>
        void InsertedIntoPartition(PartitionTile<T> tile);
        /// <summary>Called after the object has been removed from the spatial partitioning system.</summary>
        /// <param name="tile">Tile which the object was removed from.</param>
        void RemovedFromPartition(PartitionTile<T> tile);
    }
}
