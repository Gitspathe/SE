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
            foreach (Action a in ManagerUpdates) {
                a.Invoke();
            }
        }

        public static bool Insert<T>(IPartitionObject<T> obj) where T : IPartitionObject<T> 
            => SpatialPartitionManager<T>.Insert((T) obj);

        public static void Remove<T>(IPartitionObject<T> obj) where T : IPartitionObject<T> 
            => SpatialPartitionManager<T>.Remove((T) obj);

        public static void DebugDraw(Camera2D cam)
        {
            foreach (Action<Camera2D> a in ManagerDraws) {
                a.Invoke(cam);
            }
        }
    }

    public static class SpatialPartitionManager<T> where T : IPartitionObject<T>
    {
        private static SpatialPartition<T> partition;
        private static PartitionTile<T> largeObjectTile;
        private static float pruneTime = 5.0f;
        private static float pruneTimer = pruneTime;

        public static int TileSize => SpatialPartitionManager.TileSize;

        internal static QuickList<T> IgnoredObjects { get; } = new QuickList<T>(256);

        public static int EntitiesCount {
            get { throw new NotImplementedException(); }
        }

        static SpatialPartitionManager()
        {
            largeObjectTile = new PartitionTile<T>();
            partition = new SpatialPartition<T>(TileSize);

            SpatialPartitionManager.ManagerUpdates.Add(Update);
            SpatialPartitionManager.ManagerDraws.Add(DrawBoundingRectangle);
        }

        internal static void Update()
        {
            pruneTimer -= Time.UnscaledDeltaTime;
            if (pruneTimer <= 0.0f) {
                partition.Prune();
                pruneTimer = pruneTime;
            }
        }

        internal static bool Insert(T obj)
        {
            if (obj is Component c && !c.Enabled)
                return false;
            if (obj is GameObject go)
                if (!go.Enabled || go.IgnoreCulling)
                    return false;

            if (obj.CurrentPartitionTile != null) {
                obj.RemoveFromPartition();
            }

            Rectangle aabb = obj.PartitionAABB;
            if (aabb.Width > TileSize || aabb.Height > TileSize) {
                largeObjectTile.Insert(obj);
                return true;
            }

            partition.Insert(obj);
            return true;
        }

        internal static void Remove(T obj)
        {
            obj.CurrentPartitionTile?.Remove(obj);
            largeObjectTile.Remove(obj);
            if (obj is GameObject) {
                IgnoredObjects.Remove(obj);
            }
        }

        internal static void AddIgnoredObject(T obj)
        {
            if (!IgnoredObjects.Contains(obj)) {
                IgnoredObjects.Add(obj);
            }
        }

        public static void GetFromRegion(QuickList<T> existingList, Rectangle regionBounds)
        {
            partition.GetFromRegion(existingList, regionBounds);
            largeObjectTile.Get(existingList);
        }

        internal static PartitionTile<T> GetTile(Vector2 position) 
            => partition.GetTile(position);

        internal static void DrawBoundingRectangle(Camera2D camera)
        {
            partition.DrawBoundingRectangle(camera);
        }
    }
}
