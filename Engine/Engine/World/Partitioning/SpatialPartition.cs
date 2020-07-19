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
    internal struct PartitionPoint
    {
        public int X;
        public int Y;

        public PartitionPoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    internal class PartitionPointComparer : IEqualityComparer<PartitionPoint>
    {
        public bool Equals(PartitionPoint x, PartitionPoint y)
        {
            return x.X == y.X && x.Y == y.Y;
        }

        public int GetHashCode(PartitionPoint obj)
        {
            return obj.X ^ obj.Y;
        }
    }

    /// <summary>
    /// Partitions GameObjects.
    /// </summary>
    public class SpatialPartition
    {
        internal Dictionary<PartitionPoint, PartitionTile> PartitionTiles = new Dictionary<PartitionPoint, PartitionTile>(128, new PartitionPointComparer());
        private QuickList<PartitionPoint> toRemove = new QuickList<PartitionPoint>();

        private static ObjectPool<PartitionTile> tilePool = new ObjectPool<PartitionTile>();

        private int partitionTileSize;

        internal SpatialPartition(int tSize)
        {
            partitionTileSize = tSize;
        }

        public void Prune()
        {
            toRemove.Clear();
            foreach ((PartitionPoint point, PartitionTile tile) in PartitionTiles) {
                if (tile.ShouldPrune) {
                    toRemove.Add(point);
                }
            }
            foreach (PartitionPoint point in toRemove) {
                RemovePartitionTile(point);
            }
        }

        /// <summary>
        /// Gets all GameObjects from within a Rectangle region. Allocates minimal memory.
        /// </summary>
        /// <param name="existingList">List of GameObject lists to allocate.</param>
        /// <param name="regionBounds">Rectangle bounds to check.</param>
        public void GetFromRegion<T>(QuickList<T> existingList, Rectangle regionBounds) where T : IPartitionObject
        {
            int X = regionBounds.X / partitionTileSize;
            int Y = regionBounds.Y / partitionTileSize;
            int width = Round(regionBounds.Width) / partitionTileSize;
            int height = Round(regionBounds.Height) / partitionTileSize;
            regionBounds = new Rectangle(X, Y, width, height);
            for (int x = regionBounds.X; x < regionBounds.Width + regionBounds.X; x++) {
                for (int y = regionBounds.Y; y < regionBounds.Height + regionBounds.Y; y++) {
                    if (PartitionTiles.TryGetValue(new PartitionPoint(x, y), out PartitionTile tile)) {
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
        public void GetFromRegionRaw(QuickList<IPartitionObject> existingList, Rectangle regionBounds)
        {
            int X = regionBounds.X / partitionTileSize;
            int Y = regionBounds.Y / partitionTileSize;
            int width = Round(regionBounds.Width) / partitionTileSize;
            int height = Round(regionBounds.Height) / partitionTileSize;
            regionBounds = new Rectangle(X, Y, width, height);
            for (int x = regionBounds.X; x < regionBounds.Width + regionBounds.X; x++) {
                for (int y = regionBounds.Y; y < regionBounds.Height + regionBounds.Y; y++) {
                    if (PartitionTiles.TryGetValue(new PartitionPoint(x, y), out PartitionTile tile)) {
                        tile.GetRaw(existingList);
                    }
                }
            }
        }

        private int Round(float value)
        {
            float result = MathF.Ceiling(value / partitionTileSize);
            if (value > 0 && result == 0) {
                result = 1;
            }
            return (int)(result * partitionTileSize)+partitionTileSize;
        }

        private int RoundPrecise(float value)
        {
            float result = MathF.Ceiling(value / partitionTileSize);
            if (value > 0 && result == 0) {
                result = 1;
            }
            return (int)(result * partitionTileSize);
        }

        /// <summary>
        /// Gets which PartitionTile a position belongs to.
        /// </summary>
        /// <param name="position">Position in pixels.</param>
        /// <returns>PartitionTile from the position.</returns>
        internal PartitionTile GetTile(Vector2 position)
        {
            PartitionPoint point = new PartitionPoint((int)position.X / partitionTileSize, (int)position.Y / partitionTileSize);
            return PartitionTiles.TryGetValue(point, out PartitionTile tile) 
                ? tile 
                : AddNewTile(point);
        }

        /// <summary>
        /// Gets a PartitionTile at a specific array index.
        /// </summary>
        /// <param name="index">Index of the PartitionTile.</param>
        /// <returns>PartitionTile at specified index.</returns>
        internal PartitionTile GetTile(Point index)
        {
            PartitionPoint point = new PartitionPoint(index.X, index.Y);
            return PartitionTiles.TryGetValue(point, out PartitionTile tile) 
                ? tile 
                : AddNewTile(point);
        }

        /// <summary>
        /// Finds a PartitionTile located at a specific position.
        /// </summary>
        /// <param name="position">Vector2 position to check in pixels.</param>
        /// <returns>PartitionTile at position.</returns>
        internal Point GetTileIndex(Vector2 position)
        {
            int x = (int)position.X / partitionTileSize;
            int y = (int)position.Y / partitionTileSize;
            return PartitionTiles.ContainsKey(new PartitionPoint(x, y))
                ? new Point(x, y) 
                : new Point(-1, -1);
        }

        /// <summary>
        /// Gets a list of PartitionTiles inside of a line. Allocates less memory than the standard method.
        /// </summary>
        /// <param name="tiles">Existing list of PartitionTiles to allocate.</param>
        /// <param name="start">Starting point of the line.</param>
        /// <param name="end">Destination point of the line.</param>
        /// <returns></returns>
        internal void GetTilesFromLine(List<PartitionTile> tiles, Vector2 start, Vector2 end)
        {
            Point pStart = GetTileIndex(start);
            Point pEnd = GetTileIndex(end);
            Point[] points = null; //Core.Physics.BresenhamLine(pStart, pEnd);
            throw new NotImplementedException();
            for (int i = 0; i < points.Length; i++) {
                PartitionTile t = GetTile(points[i]);
                if (t != null) {
                    tiles.Add(GetTile(points[i]));
                }
            }
        }

        /// <summary>
        /// Inserts a GameObject into the grid system.
        /// </summary>
        /// <param name="go">GameObject to add.</param>
        internal void Insert(IPartitionObject obj)
        {
            PartitionTile partitionTile = GetTile(obj.PartitionPosition);
            partitionTile?.Insert(obj);
        }

        /// <summary>
        /// Removes a GameObject from the grid system.
        /// </summary>
        /// <param name="go">GameObject to remove.</param>
        internal void Remove(IPartitionObject obj)
        {
            PartitionTile partitionTile = obj.CurrentPartitionTile;
            partitionTile?.Remove(obj);
        }

        internal PartitionTile AddNewTile(PartitionPoint point)
        {
            Rectangle bounds = new Rectangle(point.X * partitionTileSize, point.Y * partitionTileSize, partitionTileSize, partitionTileSize);
            PartitionTile tile = tilePool.Take();
            tile.Reset(bounds);
            PartitionTiles.Add(point, tile);
            return tile;
        }

        internal void RemovePartitionTile(PartitionPoint point)
        {
            if (PartitionTiles.TryGetValue(point, out PartitionTile tile)) {
                PartitionTiles.Remove(point);
                tilePool.Return(tile);
            }
        }

        internal void RemovePartitionTile(PartitionTile tile)
        {
            PartitionPoint point = new PartitionPoint(tile.Bounds.X / partitionTileSize, tile.Bounds.Y / partitionTileSize);
            if (PartitionTiles.TryGetValue(point, out PartitionTile _)) {
                PartitionTiles.Remove(point);
                tilePool.Return(tile);
            }
        }

        /// <summary>
        /// Draws a representation of the grid. Useful for debugging.
        /// </summary>
        internal void DrawBoundingRectangle(Camera2D camera)
        {
            foreach (PartitionTile val in PartitionTiles.Values) {
                Rectangle r = val.Bounds;
                Debug.DrawUtility.DrawRectangle(camera, r);
            }
        }

        /// <summary>
        /// Gets a 3x3 chunk of PartitionTiles from a specific position.
        /// </summary>
        /// <param name="tileList">A pre-initialized List of PartitionTiles for allocation.</param>
        /// <param name="position">Position of the center PartitionTile.</param>
        /// <returns>List of valid PartitionTiles within a 3x3 area.</returns>
        internal void GetAdjacentTiles(List<PartitionTile> tileList, Vector2 position)
        {
            PartitionTile t = GetTile(position);
            if (t != null) {
                GetAdjacentTiles(tileList, t);
            }
        }

        /// <summary>
        /// Gets a 3x3 chunk of PartitionTiles, using a PartitionTile as the centre of the chunk.
        /// </summary>
        /// <param name="tileList">A pre-initialized List of PartitionTiles for allocation.</param>
        /// <param name="tile">The center PartitionTile.</param>
        /// <returns>List of valid PartitionTiles within a 3x3 area. NULL if no valid PartitionTiles are found.</returns>
        internal void GetAdjacentTiles(List<PartitionTile> tileList, PartitionTile tile)
        {
            Vector2 partitionTilePos = new Vector2(tile.Bounds.X, tile.Bounds.Y);
            Point partitionTileIndex = GetTileIndex(partitionTilePos);
            if (partitionTileIndex.X == -1 && partitionTileIndex.Y == -1) {
                return;
            }

            tileList.Add(tile);
            // TODO
            //// North
            //if (partitionTileIndex.Y - 1 >= 0) {
            //    tileList.Add(GridPartitionTiles[partitionTileIndex.X][partitionTileIndex.Y - 1]);
            //}
            //// North east
            //if (partitionTileIndex.Y - 1 >= 0 && partitionTileIndex.X + 1 < partitionTilesX) {
            //    tileList.Add(GridPartitionTiles[partitionTileIndex.X + 1][partitionTileIndex.Y - 1]);
            //}
            //// East
            //if (partitionTileIndex.X + 1 < partitionTilesX) {
            //    tileList.Add(GridPartitionTiles[partitionTileIndex.X + 1][partitionTileIndex.Y]);
            //}
            //// South east
            //if (partitionTileIndex.Y + 1 < partitionTilesY && partitionTileIndex.X + 1 < partitionTilesX) {
            //    tileList.Add(GridPartitionTiles[partitionTileIndex.X + 1][partitionTileIndex.Y + 1]);
            //}
            //// South
            //if (partitionTileIndex.Y + 1 < partitionTilesY) {
            //    tileList.Add(GridPartitionTiles[partitionTileIndex.X][partitionTileIndex.Y + 1]);
            //}
            //// South west
            //if (partitionTileIndex.Y + 1 < partitionTilesY && partitionTileIndex.X - 1 >= 0) {
            //    tileList.Add(GridPartitionTiles[partitionTileIndex.X - 1][partitionTileIndex.Y + 1]);
            //}
            //// West
            //if (partitionTileIndex.X - 1 >= 0) {
            //    tileList.Add(GridPartitionTiles[partitionTileIndex.X - 1][partitionTileIndex.Y]);
            //}
            //// North west
            //if (partitionTileIndex.Y - 1 >= 0 && partitionTileIndex.X - 1 >= 0) {
            //    tileList.Add(GridPartitionTiles[partitionTileIndex.X - 1][partitionTileIndex.Y - 1]);
            //}
        }
    }
}
