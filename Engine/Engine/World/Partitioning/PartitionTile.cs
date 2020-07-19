using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SE.Core;
using SE.Utility;

namespace SE.World.Partitioning
{
    public class PartitionTile<T> where T : IPartitionObject<T>
    {
        internal QuickList<T> PartitionObjects = new QuickList<T>();

        public bool ShouldPrune {
            get {
                lock (TileLock) {
                    return PartitionObjects.Count < 1;
                }
            }
        }

        protected object TileLock = new object();

        public void Reset()
        {
            lock (TileLock) {
                PartitionObjects.Clear();
            }
        }

        public QuickList<T> Get()
        {
            lock (TileLock) {
                QuickList<T> newList = new QuickList<T>();
                foreach (IPartitionObject<T> obj in PartitionObjects) {
                    newList.Add((T) obj);
                }
                return newList;
            }
        }

        public void Get(QuickList<T> existingList)
        {
            lock (TileLock) {
                existingList.AddRange(PartitionObjects);
            }
        }

        public void Insert(T obj)
        {
            if (obj.CurrentPartitionTile != null)
                SpatialPartitionManager<T>.Remove(obj);

            lock (TileLock) {
                PartitionObjects.Add(obj);
                obj.CurrentPartitionTile = this;
            }
        }

        public void Remove(T obj)
        {
            lock (TileLock) {
                PartitionObjects.Remove(obj);
                obj.CurrentPartitionTile = null;
            }
        }
    }
}