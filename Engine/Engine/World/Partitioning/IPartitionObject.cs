using Microsoft.Xna.Framework;
using SE.Core;
using System;
using System.Numerics;

namespace SE.World.Partitioning
{
    // TODO: Move this to SE.Common or something?
    public interface IAABB
    {
        Rectangle AABB { get; }
    }

    public interface IPartitionObject : IAABB
    {
        void InsertIntoPartition();
        void RemoveFromPartition();
        uint PartitionLayer { get; }
    }

    /// <summary>
    /// Represents an object which can be added to the spatial partitioning system.
    /// </summary>
    public interface IPartitionObject<T> : IPartitionObject where T : IPartitionObject<T>
    {
        /// <summary>Current partition tile the object is in. It'd be unwise to modify this manually!</summary>
        PartitionTile<T> CurrentPartitionTile { get; set; }
    }
}
