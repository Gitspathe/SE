using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SE.Common;
using SE.Components;
using SE.Utility;
using SE.World.Partitioning;
using Vector2 = System.Numerics.Vector2;

namespace SE.Core
{
    public static class SpatialPartitionManager
    {
        // Using a list for variable partition size.
        private static Dictionary<Type, SpatialPartition> partitions = new Dictionary<Type, SpatialPartition>();
        private static PartitionTile largeObjectTile;
        private static float pruneTime = 5.0f;
        private static float pruneTimer = pruneTime;

        public static int TileSize { get; private set; }
        internal static QuickList<GameObject> IgnoredGameObjects { get; } = new QuickList<GameObject>(256);

        public static int EntitiesCount {
            get { throw new NotImplementedException(); }
        }

        public static void Initialize(int tileSize)
        {
            largeObjectTile = new PartitionTile(Rectangle.Empty);
            TileSize = tileSize;
        }

        public static void Update()
        {
            pruneTimer -= Time.UnscaledDeltaTime;
            if (pruneTimer <= 0.0f) {
                foreach (SpatialPartition partition in partitions.Values) {
                    partition.Prune();
                }
                pruneTimer = pruneTime;
            }
        }

        private static void InsertGameObject(GameObject go)
        {
            if(!go.Enabled || go.IgnoreCulling)
                return;

            RectangleF bounds = go.Bounds;
            if (bounds.Width > TileSize || bounds.Height > TileSize) {
                largeObjectTile.Insert(go);
            } else {
                FetchPartition(typeof(GameObject)).Insert(go);
            }
        }

        public static void Insert(IPartitionObject obj)
        {
            if (obj is Component c && !c.Enabled)
                return;

            if (obj is GameObject go) {
                InsertGameObject(go);
            } else {
                FetchPartition(obj.PartitionObjectType).Insert(obj);
            }
        }

        public static void Remove(IPartitionObject obj)
        {
            obj.CurrentPartitionTile?.Remove(obj);
            if (obj is GameObject go) {
                IgnoredGameObjects.Remove(go);
                largeObjectTile.Remove(obj);
            }
        }

        internal static void AddIgnoredObject(GameObject go)
        {
            if (!IgnoredGameObjects.Contains(go)) {
                IgnoredGameObjects.Add(go);
            }
        }

        public static void GetFromRegion<T>(QuickList<T> existingList, Rectangle regionBounds) where T : IPartitionObject
        {
            FetchPartition(typeof(T)).GetFromRegion(existingList, regionBounds);
            largeObjectTile.Get(existingList);
        }

        public static void GetFromRegionRaw<T>(QuickList<IPartitionObject> existingList, Rectangle regionBounds) where T : IPartitionObject
        {
            FetchPartition(typeof(T)).GetFromRegionRaw(existingList, regionBounds);
            largeObjectTile.GetRaw(existingList);
        }

        internal static PartitionTile GetTile(Type type, Vector2 position) 
            => FetchPartition(type).GetTile(position);

        internal static List<PartitionTile> GetAdjacentTiles(Type type, Vector2 position, bool includeLargeObjects = true)
        {
            List<PartitionTile> tiles = new List<PartitionTile>();
            GetAdjacentTiles(type, tiles, position, includeLargeObjects);
            return tiles;
        }

        internal static void GetAdjacentTiles(Type type, List<PartitionTile> tileList, Vector2 position, bool includeLargeObjects = true)
        {
            FetchPartition(type)?.GetAdjacentTiles(tileList, position);
            if (includeLargeObjects) {
                tileList.Add(largeObjectTile);
            }
        }

        internal static void DrawBoundingRectangle(Camera2D camera)
        {
            foreach (var p in partitions) {
                p.Value.DrawBoundingRectangle(camera);
            }
        }

        internal static SpatialPartition FetchPartition(Type type)
        {
            if (partitions.TryGetValue(type, out SpatialPartition partition)) {
                return partition;
            }

            partition = new SpatialPartition(TileSize);
            partitions.Add(type, partition);
            return partition;
        }
    }
}
