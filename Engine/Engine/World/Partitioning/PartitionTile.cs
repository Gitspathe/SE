using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SE.Core;
using SE.Utility;

namespace SE.World.Partitioning
{
    public class PartitionTile
    {
        public Rectangle Bounds { get; }

        internal Dictionary<Type, QuickList<IPartitionObject>> PartitionObjects = new Dictionary<Type, QuickList<IPartitionObject>>();

        private SpatialPartition myPartition;
        private int tileSize;

        private object tileLock = new object();

        internal PartitionTile(Rectangle bounds, SpatialPartition partition, int tileSize, QuickList<Type> extensions)
        {
            myPartition = partition;
            Bounds = bounds;
        }

        public QuickList<T> Get<T>() where T : IPartitionObject
        {
            lock (tileLock) {
                if (!PartitionObjects.TryGetValue(typeof(T), out QuickList<IPartitionObject> objects)) 
                    return null;

                QuickList<T> newList = new QuickList<T>();
                foreach (IPartitionObject obj in objects) {
                    newList.Add((T) obj);
                }
                return newList;
            }
        }

        public void Get<T>(QuickList<T> existingList) where T : IPartitionObject
        {
            lock (tileLock) {
                if (!PartitionObjects.TryGetValue(typeof(T), out QuickList<IPartitionObject> objects)) 
                    return;

                IPartitionObject[] array = objects.Array;
                for (int i = 0; i < objects.Count; i++) {
                    existingList.Add((T)array[i]);
                }
            }
        }

        public void GetRaw<T>(QuickList<IPartitionObject> existingList) where T : IPartitionObject
        {
            lock (tileLock) {
                if (!PartitionObjects.TryGetValue(typeof(T), out QuickList<IPartitionObject> objects))
                    return;

                existingList.AddRange(objects);
            }
        }

        public void Insert(IPartitionObject obj)
        {
            if (obj.CurrentPartitionTile != null)
                SpatialPartitionManager.Remove(obj);

            lock (tileLock) {
                Type type = obj.PartitionObjectType;
                if (PartitionObjects.TryGetValue(type, out QuickList<IPartitionObject> list)) {
                    list.Add(obj);
                } else {
                    QuickList<IPartitionObject> newList = new QuickList<IPartitionObject> { obj };
                    PartitionObjects.Add(type, newList);
                }
                obj.CurrentPartitionTile = this;
                if (obj is IPartitionObjectExtended ext) {
                    ext.InsertedIntoPartition(this);
                }
            }
        }

        public void Remove(IPartitionObject obj)
        {
            lock (tileLock) {
                Type type = obj.PartitionObjectType;
                if (PartitionObjects.TryGetValue(type, out QuickList<IPartitionObject> list)) {
                    if (!list.Remove(obj)) {
                        throw new NullReferenceException("The IPartitionObject wasn't found!");
                    }
                    if (list.Count < 1) {
                        PartitionObjects.Remove(type);
                    }
                }
                obj.CurrentPartitionTile = null;
                if (obj is IPartitionObjectExtended ext) {
                    ext.RemovedFromPartition(this);
                }
            }
        }
    }
}