using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using SE.Core;
using SE.Utility;

namespace SE.World.Partitioning
{
    public sealed class PartitionTile<T> where T : IPartitionObject<T>
    {
        internal QuickList<T> PartitionObjects = new QuickList<T>();

        public bool ShouldPrune {
            get {
                lock (tileLock) {
                    return PartitionObjects.Count < 1;
                }
            }
        }

        private object tileLock = new object();

        internal void Reset()
        {
            lock (tileLock) {
                PartitionObjects.Clear();
            }
        }

        internal QuickList<T> Get()
        {
            lock (tileLock) {
                QuickList<T> newList = new QuickList<T>();
                newList.AddRange(PartitionObjects);
                return newList;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Get(QuickList<T> existingList)
        {
            lock (tileLock) {
                existingList.AddRange(PartitionObjects);
            }
        }

        internal void Insert(T obj)
        {
            lock (tileLock) {
                PartitionObjects.Add(obj);
                obj.CurrentPartitionTile = this;
            }
        }

        internal void Remove(T obj)
        {
            lock (tileLock) {
                PartitionObjects.Remove(obj);
                obj.CurrentPartitionTile = null;
            }
        }

        internal PartitionTile() { }
    }
}