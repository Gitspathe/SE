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
    // TODO: Move dictionary<Type, list> to TOP level. (so organize partitions by type, not the partition tiles.)
    // TODO: This could be solved by making this static class generic. (i.e, SpatialPartitionManager<Sprite>, etc.)

    internal static class SpatialPartitionManager<T> where T : IPartitionObject<T>
    {

    }

    public static class SpatialPartitionManager
    {
        // Using a list for variable partition size.
        private static QuickList<SpatialPartition> partitions = new QuickList<SpatialPartition>();
        private static QuickList<SpatialPartition> partitionPool = new QuickList<SpatialPartition>();

        private static PartitionTile largeObjectTile;

        internal static QuickList<GameObject> IgnoredGameObjects { get; } = new QuickList<GameObject>(256);

        public static int PartitionSize { get; private set; }
        public static int TileSize { get; private set; }

        internal static int InitialPoolSize { get; private set; } = 32;
        internal static int MaxPoolSize { get; private set; } = 128;

        public static int EntitiesCount {
            get {
                int total = 0;
                foreach (SpatialPartition partition in partitions) {
                    for (int x = 0; x < partition.GridPartitionTiles.Length; x++) {
                        for (int y = 0; y < partition.GridPartitionTiles[x].Length; y++) {
                            PartitionTile tile = partition.GridPartitionTiles[x][y];
                            foreach(KeyValuePair<Type, QuickList<IPartitionObject>> pair in tile.PartitionObjects) {
                                total += pair.Value.Count;
                            }
                        }
                    }
                }
                return total;
            }
        }

        private static void ReturnToPool(SpatialPartition partition)
        {
            partitions.Remove(partition);

            // Return the partition to the pool, if there's enough room.
            if (partitionPool.Count + 1 < MaxPoolSize) {
                partitionPool.Add(partition);
            }
            partition.IsActive = false;
        }

        private static SpatialPartition TakeFromPool(Point position)
        {
            // Create new partition if there's none in the pool.
            if (partitionPool.Count < 1) {
                return CreateNewPartition(position, true);
            }

            // Otherwise, return a partition that's in the pool.
            SpatialPartition partition = partitionPool.Array[partitionPool.Count - 1];
            partitions.Add(partition);
            partitionPool.Remove(partition);
            partition.UpdatePosition(position);
            partition.IsActive = true;
            return partition;
        }

        private static SpatialPartition CreateNewPartition(Point position, bool active = false)
        {
            Point newPos = ToPartitionPoint(position);
            Rectangle bounds = new Rectangle(newPos.X, newPos.Y, PartitionSize, PartitionSize);

            SpatialPartition partition = new SpatialPartition(TileSize, bounds);
            partitionPool.Add(partition);
            return active ? TakeFromPool(position) : partition;
        }

        public static void Initialize(int tileSize, int partitionSize)
        {
            largeObjectTile = new PartitionTile(Rectangle.Empty);
            TileSize = tileSize;
            PartitionSize = partitionSize;

            // Initialize pool.
            for (int i = 0; i < InitialPoolSize; i++) {
                Rectangle bounds = new Rectangle(0, 0, PartitionSize, PartitionSize);
                SpatialPartition partition = new SpatialPartition(TileSize, bounds);
                ReturnToPool(partition);
            }
        }

        public static void Update()
        {
            for (int i = 0; i < partitions.Count; i++) {
                SpatialPartition partition = partitions.Array[i];
                partition.Update();
                if (partition.NeedsPrune) {
                    ReturnToPool(partition);
                    partition.NeedsPrune = false;
                }
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
                SpatialPartition tile = FetchPartition(go.PartitionPosition)
                                        ?? TakeFromPool(ToPartitionPoint(go.PartitionPosition));
                tile.Insert(go);
            }
        }

        public static void Insert(IPartitionObject obj)
        {
            if (obj is Component c && !c.Enabled)
                return;

            if (obj is GameObject go) {
                InsertGameObject(go);
            } else {
                SpatialPartition tile = FetchPartition(obj.PartitionPosition)
                                        ?? TakeFromPool(ToPartitionPoint(obj.PartitionPosition));
                tile.Insert(obj);
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
            for (int i = 0; i < partitions.Count; i++) {
                SpatialPartition partition = partitions.Array[i];
                if (partition.Bounds.Intersects(regionBounds)) {
                    partition.GetFromRegion(existingList, ConvertToLocal(regionBounds, partition));
                }
            }
            largeObjectTile.Get(existingList);
        }

        public static void GetFromRegionRaw<T>(QuickList<IPartitionObject> existingList, Rectangle regionBounds) where T : IPartitionObject
        {
            for (int i = 0; i < partitions.Count; i++) {
                SpatialPartition partition = partitions.Array[i];
                if (partition.Bounds.Intersects(regionBounds)) {
                    partition.GetFromRegionRaw<T>(existingList, ConvertToLocal(regionBounds, partition));
                }
            }
            largeObjectTile.GetRaw<T>(existingList);
        }

        internal static QuickList<PartitionTile> GetTilesFromRegion(Rectangle regionBounds, bool includeLargeObjectTile = true)
        {
            QuickList<PartitionTile> tilesList = new QuickList<PartitionTile>();
            GetTilesFromRegion(tilesList, regionBounds, includeLargeObjectTile);
            return tilesList;
        }

        internal static void GetTilesFromRegion(QuickList<PartitionTile> tilesList, Rectangle regionBounds, bool includeLargeObjectTile = true)
        {
            for (int i = 0; i < partitions.Count; i++) {
                SpatialPartition partition = partitions.Array[i];
                if (partition.Bounds.Intersects(regionBounds)) {
                    partition.GetTilesFromRegion(tilesList, ConvertToLocal(regionBounds, partition));
                }
            }
            if(includeLargeObjectTile)
                tilesList.Add(largeObjectTile);
        }

        internal static PartitionTile GetTile(Vector2 position) 
            => FetchPartition(position).GetTile(position);

        internal static SpatialPartition FetchPartition(Vector2 position)
        {
            for (int i = 0; i < partitions.Count; i++) {
                if (partitions.Array[i].Bounds.Contains(new Point((int) position.X, (int) position.Y))) {
                    return partitions.Array[i];
                }
            }
            return null;
        }

        internal static List<PartitionTile> GetAdjacentTiles(Vector2 position, bool includeLargeObjects = true)
        {
            List<PartitionTile> tiles = new List<PartitionTile>();
            GetAdjacentTiles(tiles, position, includeLargeObjects);
            return tiles;
        }

        internal static void GetAdjacentTiles(List<PartitionTile> tileList, Vector2 position, bool includeLargeObjects = true)
        {
            SpatialPartition p = FetchPartition(position);
            p?.GetAdjacentTiles(tileList, position - p.Position);
            if (includeLargeObjects) {
                tileList.Add(largeObjectTile);
            }
        }

        internal static void DrawBoundingRectangle(Camera2D camera)
        {
            for (int i = 0; i < partitions.Count; i++) { 
                partitions.Array[i].DrawBoundingRectangle(camera);
            }
        }

        private static Point ToPartitionPoint(Point position)
            => new Point((int)MathF.Floor((float)position.X / PartitionSize) * PartitionSize,
                (int)MathF.Floor((float)position.Y / PartitionSize) * PartitionSize);

        private static Point ToPartitionPoint(Vector2 position)
            => new Point((int)MathF.Floor(position.X / PartitionSize) * PartitionSize,
                (int)MathF.Floor(position.Y / PartitionSize) * PartitionSize);

        private static Rectangle ConvertToLocal(Rectangle worldBounds, SpatialPartition localPartition) 
            => new Rectangle(worldBounds.X - localPartition.Bounds.X, 
                worldBounds.Y - localPartition.Bounds.Y,
                worldBounds.Width,
                worldBounds.Height);

        private static Point ConvertToLocal(Point worldPoint, SpatialPartition localPartition)
            => new Point(worldPoint.X - localPartition.Bounds.X, worldPoint.Y - localPartition.Bounds.Y);
    }
}
