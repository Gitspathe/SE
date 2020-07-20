using Microsoft.Xna.Framework;
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
        Rectangle PartitionAABB { get; }
        /// <summary>Should never be modified manually!</summary>
        PartitionTile<T> CurrentPartitionTile { get; set; }
    }
}
