using Microsoft.Xna.Framework;
using SE.Core;
using System;
using System.Numerics;

namespace SE.World.Partitioning
{
    public interface IPartitionObject
    {
        Rectangle PartitionAABB { get; }
        void InsertIntoPartition();
        void RemoveFromPartition();
    }

    /// <summary>
    /// Represents an object which can be added to the spatial partitioning system.
    /// </summary>
    public interface IPartitionObject<T> : IPartitionObject where T : IPartitionObject<T>
    {
        /// <summary>Current partition tile the object is in. It'd be unwise to modify this manually!</summary>
        // TODO: Get rid of this and use a Dictionary<IPartitionObject, PartitionTile> instead?
        PartitionTile<T> CurrentPartitionTile { get; set; }
    }
}
