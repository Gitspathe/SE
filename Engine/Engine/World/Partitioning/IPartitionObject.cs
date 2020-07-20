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

        void IPartitionObject.InsertIntoPartition() => SpatialPartitionManager<T>.Insert((T)this);
        void IPartitionObject.RemoveFromPartition() => SpatialPartitionManager<T>.Remove((T)this);
    }

    public interface IPartitionObjectExtended<T> : IPartitionObject<T> where T : IPartitionObject<T>
    {
        void InsertedIntoPartition();
        void RemovedFromPartition();
    }
}
