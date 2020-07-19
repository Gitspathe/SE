using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SE.Components;
using SE.Core;
using SE.Pooling;
using SE.Utility;
using Random = System.Random;
using Vector2 = System.Numerics.Vector2;

namespace SE.World.Partitioning
{
    internal class PartitionPointComparer : IEqualityComparer<Point>
    {
        public bool Equals(Point x, Point y) => x.X == y.X && x.Y == y.Y;
        public int GetHashCode(Point obj) => obj.X ^ obj.Y;
    }

    /// <summary>
    /// Partitions GameObjects.
    /// </summary>
    public class SpatialPartition<T> where T : IPartitionObject<T>
    {
        internal Dictionary<Point, PartitionTile<T>> PartitionTiles = new Dictionary<Point, PartitionTile<T>>(128, new PartitionPointComparer());
        private QuickList<Point> toRemove = new QuickList<Point>();

        private static ObjectPool<PartitionTile<T>> tilePool = new ObjectPool<PartitionTile<T>>(512);

        protected int PartitionTileSize;

        internal SpatialPartition(int tSize)
        {
            PartitionTileSize = tSize;
        }

        public void Prune()
        {
            toRemove.Clear();
            foreach ((Point point, PartitionTile<T> tile) in PartitionTiles) {
                if (tile.ShouldPrune) {
                    toRemove.Add(point);
                }
            }
            foreach (Point point in toRemove) {
                RemovePartitionTile(point);
            }
        }

        /// <summary>
        /// Gets all GameObjects from within a Rectangle region. Allocates minimal memory.
        /// </summary>
        /// <param name="existingList">List of GameObject lists to allocate.</param>
        /// <param name="regionBounds">Rectangle bounds to check.</param>
        public void GetFromRegion(QuickList<T> existingList, Rectangle regionBounds)
        {
            int startX = regionBounds.X / PartitionTileSize;
            int startY = regionBounds.Y / PartitionTileSize;
            int width = RoundUp(regionBounds.Width) / PartitionTileSize;
            int height = RoundUp(regionBounds.Height) / PartitionTileSize;
            regionBounds = new Rectangle(startX, startY, width, height);
            for (int x = regionBounds.X; x < regionBounds.Width + regionBounds.X; x++) {
                for (int y = regionBounds.Y; y < regionBounds.Height + regionBounds.Y; y++) {
                    if (PartitionTiles.TryGetValue(new Point(x, y), out PartitionTile<T> tile)) {
                        tile.Get(existingList);
                    }
                }
            }
        }

        /// <summary>
        /// Gets all GameObjects from within a Rectangle region. Allocates minimal memory.
        /// </summary>
        /// <param name="existingList">List of GameObject lists to allocate.</param>
        /// <param name="regionBounds">Rectangle bounds to check.</param>
        public void GetFromRegionRaw(QuickList<IPartitionObject<T>> existingList, Rectangle regionBounds)
        {
            int startX = regionBounds.X / PartitionTileSize;
            int startY = regionBounds.Y / PartitionTileSize;
            int width = RoundUp(regionBounds.Width) / PartitionTileSize;
            int height = RoundUp(regionBounds.Height) / PartitionTileSize;
            regionBounds = new Rectangle(startX, startY, width, height);
            for (int x = regionBounds.X; x < regionBounds.Width + regionBounds.X; x++) {
                for (int y = regionBounds.Y; y < regionBounds.Height + regionBounds.Y; y++) {
                    if (PartitionTiles.TryGetValue(new Point(x, y), out PartitionTile<T> tile)) {
                        tile.GetRaw(existingList);
                    }
                }
            }
        }

        protected int RoundUp(float value)
        {
            float result = MathF.Ceiling(value / PartitionTileSize);
            if (value > 0 && result == 0) {
                result = 1;
            }
            return (int)(result * PartitionTileSize)+PartitionTileSize;
        }

        /// <summary>
        /// Gets which PartitionTile a position belongs to.
        /// </summary>
        /// <param name="position">Position in pixels.</param>
        /// <returns>PartitionTile from the position.</returns>
        internal PartitionTile<T> GetTile(Vector2 position)
        {
            Point point = new Point((int) MathF.Floor(position.X / PartitionTileSize), (int) MathF.Floor(position.Y / PartitionTileSize));
            return PartitionTiles.TryGetValue(point, out PartitionTile<T> tile) 
                ? tile 
                : AddNewTile(point);
        }

        /// <summary>
        /// Gets a PartitionTile at a specific array index.
        /// </summary>
        /// <param name="index">Index of the PartitionTile.</param>
        /// <returns>PartitionTile at specified index.</returns>
        internal PartitionTile<T> GetTile(Point index)
        {
            return PartitionTiles.TryGetValue(index, out PartitionTile<T> tile) 
                ? tile 
                : AddNewTile(index);
        }

        /// <summary>
        /// Finds a PartitionTile located at a specific position.
        /// </summary>
        /// <param name="position">Vector2 position to check in pixels.</param>
        /// <returns>PartitionTile at position.</returns>
        internal Point GetTileIndex(Vector2 position)
        {
            int x = (int)position.X / PartitionTileSize;
            int y = (int)position.Y / PartitionTileSize;
            Point point = new Point(x, y);
            return PartitionTiles.ContainsKey(point)
                ? point
                : new Point(-1, -1);
        }

        /// <summary>
        /// Gets a list of PartitionTiles inside of a line. Allocates less memory than the standard method.
        /// </summary>
        /// <param name="tiles">Existing list of PartitionTiles to allocate.</param>
        /// <param name="start">Starting point of the line.</param>
        /// <param name="end">Destination point of the line.</param>
        /// <returns></returns>
        internal void GetTilesFromLine(List<PartitionTile<T>> tiles, Vector2 start, Vector2 end)
        {
            Point pStart = GetTileIndex(start);
            Point pEnd = GetTileIndex(end);
            Point[] points = null; //Core.Physics.BresenhamLine(pStart, pEnd);
            throw new NotImplementedException();
            //for (int i = 0; i < points.Length; i++) {
            //    PartitionTile t = GetTile(points[i]);
            //    if (t != null) {
            //        tiles.Add(GetTile(points[i]));
            //    }
            //}
        }

        /// <summary>
        /// Inserts a GameObject into the grid system.
        /// </summary>
        /// <param name="go">GameObject to add.</param>
        internal void Insert(IPartitionObject<T> obj)
        {
            PartitionTile<T> partitionTile = GetTile(obj.PartitionPosition);
            partitionTile?.Insert(obj);
        }

        /// <summary>
        /// Removes a GameObject from the grid system.
        /// </summary>
        /// <param name="go">GameObject to remove.</param>
        internal void Remove(IPartitionObject<T> obj)
        {
            PartitionTile<T> partitionTile = obj.CurrentPartitionTile;
            partitionTile?.Remove(obj);
        }

        internal PartitionTile<T> AddNewTile(Point point)
        {
            PartitionTile<T> tile = tilePool.Take();
            tile.Reset();
            PartitionTiles.Add(point, tile);
            return tile;
        }

        internal void RemovePartitionTile(Point point)
        {
            if (PartitionTiles.TryGetValue(point, out PartitionTile<T> tile)) {
                PartitionTiles.Remove(point);
                tilePool.Return(tile);
            }
        }

        /// <summary>
        /// Draws a representation of the grid. Useful for debugging.
        /// </summary>
        internal void DrawBoundingRectangle(Camera2D camera)
        {
            foreach (Point point in PartitionTiles.Keys) {
                Rectangle bounds = new Rectangle(point.X * PartitionTileSize, point.Y * PartitionTileSize, PartitionTileSize, PartitionTileSize);
                Debug.DrawUtility.DrawRectangle(camera, bounds);
            }
        }
    }
}
