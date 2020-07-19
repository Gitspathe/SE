using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SE.Core;
using SE.Utility;

namespace SE.World.Partitioning
{
    public class PartitionTile
    {
        public Rectangle Bounds { get; private set; }

        internal QuickList<IPartitionObject> PartitionObjects = new QuickList<IPartitionObject>();

        public bool ShouldPrune {
            get {
                lock (tileLock) {
                    return PartitionObjects.Count < 1;
                }
            }
        }

        private object tileLock = new object();

        internal PartitionTile(Rectangle bounds)
        {
            Bounds = bounds;
        }

        public PartitionTile() { }

        public void Reset(Rectangle bounds)
        {
            Bounds = bounds;
            lock (tileLock) {
                PartitionObjects.Clear();
            }
        }

        public QuickList<T> Get<T>() where T : IPartitionObject
        {
            lock (tileLock) {
                QuickList<T> newList = new QuickList<T>();
                foreach (IPartitionObject obj in PartitionObjects) {
                    newList.Add((T) obj);
                }
                return newList;
            }
        }

        public void Get<T>(QuickList<T> existingList) where T : IPartitionObject
        {
            lock (tileLock) {
                IPartitionObject[] array = PartitionObjects.Array;
                for (int i = 0; i < PartitionObjects.Count; i++) {
                    existingList.Add((T)array[i]);
                }
            }
        }

        public void GetRaw(QuickList<IPartitionObject> existingList)
        {
            lock (tileLock) {
                existingList.AddRange(PartitionObjects);
            }
        }

        public void Insert(IPartitionObject obj)
        {
            if (obj.CurrentPartitionTile != null)
                SpatialPartitionManager.Remove(obj);

            lock (tileLock) {
                PartitionObjects.Add(obj);
                obj.CurrentPartitionTile = this;
                if (obj is IPartitionObjectExtended ext) {
                    ext.InsertedIntoPartition(this);
                }
            }
        }

        public void Remove(IPartitionObject obj)
        {
            lock (tileLock) {
                PartitionObjects.Remove(obj);
                obj.CurrentPartitionTile = null;
                if (obj is IPartitionObjectExtended ext) {
                    ext.RemovedFromPartition(this);
                }
            }
        }
    }
}