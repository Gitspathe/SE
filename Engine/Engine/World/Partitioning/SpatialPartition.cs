using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SE.Components;
using SE.Core;
using SE.Utility;
using Random = System.Random;
using Vector2 = System.Numerics.Vector2;

namespace SE.World.Partitioning
{
    /// <summary>
    /// Partitions GameObjects.
    /// </summary>
    public class SpatialPartition
    {
        internal bool NeedsPrune;
        internal PartitionTile[][] GridPartitionTiles;

        private int partitionTileSize, partitionTilesX, partitionTilesY;
        private int pruneOffset;
        private float pruneCounter;

        public Vector2 Position { get; private set; }
        public Rectangle Bounds { get; private set; }
        public bool IsActive { get; internal set; }

        internal SpatialPartition(int tSize, Rectangle bounds, QuickList<Type> extensions = null)
        {
            Bounds = bounds;
            Position = new Vector2(bounds.X, bounds.Y);
            GridPartitionTiles = null;

            partitionTileSize = tSize;
            partitionTilesX = bounds.Width / partitionTileSize;
            partitionTilesY = bounds.Height / partitionTileSize;
            GridPartitionTiles = new PartitionTile[partitionTilesX][];
            for (int x = 0; x < partitionTilesX; x++) {
                GridPartitionTiles[x] = new PartitionTile[partitionTilesX];
                for (int y = 0; y < partitionTilesY; y++) {
                    Rectangle b = new Rectangle(x * partitionTileSize, y * partitionTileSize, partitionTileSize, partitionTileSize);
                    GridPartitionTiles[x][y] = new PartitionTile(b, this, tSize, extensions);
                }
            }
            pruneOffset = SE.Utility.Random.Next(0, 120);
        }

        internal void UpdatePosition(Point newPos)
        {
            Position = new Vector2(newPos.X, newPos.Y);
            Bounds = new Rectangle(newPos.X, newPos.Y, Bounds.Width, Bounds.Height);
        }

        internal void Update()
        {
            if (pruneOffset > 0) {
                pruneOffset--;
            } else {
                pruneCounter -= Time.UnscaledDeltaTime;
                if (pruneCounter <= 0) {
                    NeedsPrune = ShouldPrune();
                    pruneCounter = 5.0f;
                }
            }
        }

        private bool ShouldPrune()
        {
            for (int x = 0; x < partitionTilesX; x++) {
                for (int y = 0; y < partitionTilesY; y++) {
                    PartitionTile tile = GridPartitionTiles[x][y];
                    if (tile.PartitionObjects.Count > 0)
                        return false;
                }
            }
            return true;
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
                    if (x < 0 || y < 0 || x > partitionTilesX - 1 || y > partitionTilesY - 1)
                        continue;

                    GridPartitionTiles[x][y].Get(existingList);
                }
            }
        }

        /// <summary>
        /// Gets all GameObjects from within a Rectangle region. Allocates minimal memory.
        /// </summary>
        /// <param name="existingList">List of GameObject lists to allocate.</param>
        /// <param name="regionBounds">Rectangle bounds to check.</param>
        public void GetFromRegionRaw<T>(QuickList<IPartitionObject> existingList, Rectangle regionBounds) where T : IPartitionObject
        {
            int X = regionBounds.X / partitionTileSize;
            int Y = regionBounds.Y / partitionTileSize;
            int width = Round(regionBounds.Width) / partitionTileSize;
            int height = Round(regionBounds.Height) / partitionTileSize;
            regionBounds = new Rectangle(X, Y, width, height);
            for (int x = regionBounds.X; x < regionBounds.Width + regionBounds.X; x++) {
                for (int y = regionBounds.Y; y < regionBounds.Height + regionBounds.Y; y++) {
                    if (x < 0 || y < 0 || x > partitionTilesX - 1 || y > partitionTilesY - 1)
                        continue;

                    GridPartitionTiles[x][y].GetRaw<T>(existingList);
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

        internal void GetTilesFromRegionOrdered(PartitionTile[][] tiles, Rectangle regionBounds)
        {
            int X = regionBounds.X / partitionTileSize;
            int Y = regionBounds.Y / partitionTileSize;
            int width = RoundPrecise(regionBounds.Width) / partitionTileSize;
            int height = RoundPrecise(regionBounds.Height) / partitionTileSize;
            regionBounds = new Rectangle(X, Y, width, height);

            int xIndex = 0, yIndex = 0;
            for (int x = regionBounds.X; x < regionBounds.Width + regionBounds.X; x++) {
                for (int y = regionBounds.Y; y < regionBounds.Height + regionBounds.Y; y++) {
                    if (x < 0 || y < 0 || x > partitionTilesX - 1 || y > partitionTilesY - 1) {
                        yIndex++;
                        continue;
                    }
                    tiles[xIndex][yIndex] = GridPartitionTiles[x][y];
                    yIndex++;
                }
                xIndex++;
            }
        }

        internal void GetTilesFromRegion(QuickList<PartitionTile> tileList, Rectangle regionBounds)
        {
            int X = regionBounds.X / partitionTileSize;
            int Y = regionBounds.Y / partitionTileSize;
            int width = RoundPrecise(regionBounds.Width) / partitionTileSize;
            int height = RoundPrecise(regionBounds.Height) / partitionTileSize;
            regionBounds = new Rectangle(X, Y, width, height);

            int xIndex = 0, yIndex = 0;
            for (int x = regionBounds.X; x < regionBounds.Width + regionBounds.X; x++) {
                for (int y = regionBounds.Y; y < regionBounds.Height + regionBounds.Y; y++) {
                    if (x < 0 || y < 0 || x > partitionTilesX - 1 || y > partitionTilesY - 1) {
                        yIndex++;
                        continue;
                    }
                    tileList.Add(GridPartitionTiles[x][y]);
                    yIndex++;
                }
                xIndex++;
            }
        }

        /// <summary>
        /// Gets which PartitionTile a position belongs to.
        /// </summary>
        /// <param name="position">Position in pixels.</param>
        /// <returns>PartitionTile from the position.</returns>
        internal PartitionTile GetTile(Vector2 position)
        {
            int x = (int)position.X / partitionTileSize;
            int y = (int)position.Y / partitionTileSize;
            if (x < 0 || y < 0 || x > partitionTilesX-1 || y > partitionTilesY-1)
                return null;
            
            return GridPartitionTiles[x][y];
        }

        /// <summary>
        /// Gets a PartitionTile at a specific array index.
        /// </summary>
        /// <param name="index">Index of the PartitionTile.</param>
        /// <returns>PartitionTile at specified index.</returns>
        internal PartitionTile GetTile(Point index)
        {
            int x = index.X;
            int y = index.Y;
            if (x < 0 || y < 0 || x > partitionTilesX-1 || y > partitionTilesY-1)
                return null;

            return GridPartitionTiles[x][y];
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
            if (x < 0 || y < 0 || x > partitionTilesX - 1 || y > partitionTilesY - 1)
                return new Point(-1,-1);
            
            return new Point(x, y);
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
            PartitionTile partitionTile = GetTile(obj.PartitionPosition - Position);
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

        /// <summary>
        /// Draws a representation of the grid. Useful for debugging.
        /// </summary>
        internal void DrawBoundingRectangle(Camera2D camera)
        {
            for(int x = 0; x < GridPartitionTiles.Length; x++) {
                for(int y = 0; y < GridPartitionTiles[x].Length; y++) {
                    if (GridPartitionTiles[x][y] == null)
                        continue;

                    Rectangle r = GridPartitionTiles[x][y].Bounds;
                    r.X += Bounds.X;
                    r.Y += Bounds.Y;

                    Debug.DrawUtility.DrawRectangle(camera, r);
                }
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
            // North
            if (partitionTileIndex.Y - 1 >= 0) {
                tileList.Add(GridPartitionTiles[partitionTileIndex.X][partitionTileIndex.Y - 1]);
            }
            // North east
            if (partitionTileIndex.Y - 1 >= 0 && partitionTileIndex.X + 1 < partitionTilesX) {
                tileList.Add(GridPartitionTiles[partitionTileIndex.X + 1][partitionTileIndex.Y - 1]);
            }
            // East
            if (partitionTileIndex.X + 1 < partitionTilesX) {
                tileList.Add(GridPartitionTiles[partitionTileIndex.X + 1][partitionTileIndex.Y]);
            }
            // South east
            if (partitionTileIndex.Y + 1 < partitionTilesY && partitionTileIndex.X + 1 < partitionTilesX) {
                tileList.Add(GridPartitionTiles[partitionTileIndex.X + 1][partitionTileIndex.Y + 1]);
            }
            // South
            if (partitionTileIndex.Y + 1 < partitionTilesY) {
                tileList.Add(GridPartitionTiles[partitionTileIndex.X][partitionTileIndex.Y + 1]);
            }
            // South west
            if (partitionTileIndex.Y + 1 < partitionTilesY && partitionTileIndex.X - 1 >= 0) {
                tileList.Add(GridPartitionTiles[partitionTileIndex.X - 1][partitionTileIndex.Y + 1]);
            }
            // West
            if (partitionTileIndex.X - 1 >= 0) {
                tileList.Add(GridPartitionTiles[partitionTileIndex.X - 1][partitionTileIndex.Y]);
            }
            // North west
            if (partitionTileIndex.Y - 1 >= 0 && partitionTileIndex.X - 1 >= 0) {
                tileList.Add(GridPartitionTiles[partitionTileIndex.X - 1][partitionTileIndex.Y - 1]);
            }
        }
    }
}
