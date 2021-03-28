using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SE.Common;
using SE.Components;
using SE.Utility;
using SE.World.Partitioning;
using Vector2 = System.Numerics.Vector2;
// ReSharper disable StaticMemberInGenericType

namespace SE.Core
{
    public static class SpatialPartitionManager
    {
        internal static QuickList<Action> ManagerUpdates = new QuickList<Action>();
        internal static QuickList<Action<Camera2D>> ManagerDraws = new QuickList<Action<Camera2D>>();

        public static int TileSize { get; private set; } = 192;

        public static void Update()
        {
            Action[] array = ManagerUpdates.Array;
            for (int i = 0; i < ManagerUpdates.Count; i++) {
                array[i].Invoke();
            }
        }

        public static void DebugDraw(Camera2D cam)
        {
            Action<Camera2D>[] array = ManagerDraws.Array;
            for (int i = 0; i < ManagerUpdates.Count; i++) {
                array[i].Invoke(cam);
            }
        }

        public static void Insert<T>(IPartitionObject<T> obj) where T : IPartitionObject<T> 
            => SpatialPartitionManager<T>.Insert((T) obj);

        public static void Remove<T>(IPartitionObject<T> obj) where T : IPartitionObject<T> 
            => SpatialPartitionManager<T>.Remove((T) obj);
    }

    public static class SpatialPartitionManager<T> where T : IPartitionObject<T>
    {
        private static Dictionary<uint, SpatialPartition<T>> partition;
        private static PartitionTile<T> largeObjectTile;
        private static float pruneTime = 5.0f;
        private static float pruneTimer = pruneTime;

        public static int TileSize => SpatialPartitionManager.TileSize;

        public static int EntitiesCount {
            get { throw new NotImplementedException(); }
        }

        static SpatialPartitionManager()
        {
            largeObjectTile = new PartitionTile<T>();
            partition = new Dictionary<uint, SpatialPartition<T>>();
            partition.Add(0, new SpatialPartition<T>(TileSize));

            SpatialPartitionManager.ManagerUpdates.Add(Update);
            SpatialPartitionManager.ManagerDraws.Add(DrawBoundingRectangle);
        }

        internal static void Update()
        {
            pruneTimer -= Time.UnscaledDeltaTime;
            if (!(pruneTimer <= 0.0f)) 
                return;

            foreach (SpatialPartition<T> p in partition.Values) {
                p.Prune();
            }
            pruneTimer = pruneTime;
        }

        internal static void Insert(T obj)
        {
            if (obj.CurrentPartitionTile != null) {
                obj.RemoveFromPartition();
            }

            // If the object is too large, add it to the large object tile.
            Rectangle aabb = obj.AABB;
            if (aabb.Width > TileSize || aabb.Height > TileSize) {
                largeObjectTile.Insert(obj);
                return; // This wasn't here before, might cause a bug!
            }

            // Try and get the partition for the object's layer. If one isn't found, create a new one.
            if (!partition.TryGetValue(obj.PartitionLayer, out SpatialPartition<T> p)) {
                p = new SpatialPartition<T>(TileSize);
                partition.Add(obj.PartitionLayer, p);
            }
            p.Insert(obj);
        }

        internal static void Remove(T obj)
        {
            obj.CurrentPartitionTile?.Remove(obj);
            largeObjectTile.Remove(obj);
        }

        public static void GetFromRegion(QuickList<T> existingList, Rectangle regionBounds, uint layer = 0)
        {
            if (!partition.TryGetValue(layer, out SpatialPartition<T> p))
                throw new NullReferenceException();

            p.GetFromRegion(existingList, regionBounds);
            largeObjectTile.Get(existingList);
        }

        internal static PartitionTile<T> GetTile(Vector2 position, uint layer = 0)
        {
            if (!partition.TryGetValue(layer, out SpatialPartition<T> p))
                throw new NullReferenceException();

            return p.GetTile(position);
        }

        internal static void DrawBoundingRectangle(Camera2D camera)
        {
            foreach (SpatialPartition<T> p in partition.Values) {
                p.DrawBoundingRectangle(camera);
            }
        }
    }
}
