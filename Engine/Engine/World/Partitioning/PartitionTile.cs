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

        internal void Reset()
        {
            lock (TileLock) {
                PartitionObjects.Clear();
            }
        }

        internal QuickList<T> Get()
        {
            lock (TileLock) {
                QuickList<T> newList = new QuickList<T>();
                newList.AddRange(PartitionObjects);
                return newList;
            }
        }

        internal void Get(QuickList<T> existingList)
        {
            lock (TileLock) {
                existingList.AddRange(PartitionObjects);
            }
        }

        internal void Insert(T obj)
        {
            lock (TileLock) {
                PartitionObjects.Add(obj);
                obj.CurrentPartitionTile = this;
            }
        }

        internal void Remove(T obj)
        {
            lock (TileLock) {
                PartitionObjects.Remove(obj);
                obj.CurrentPartitionTile = null;
            }
        }

        internal PartitionTile() { }
    }
}