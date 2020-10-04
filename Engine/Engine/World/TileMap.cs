using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using SE.Attributes;
using SE.Common;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;

namespace SE.World
{
    [ExecuteInEditor]
    public class TileMap : Component
    {
        public int ChunkSize { get; private set; } = 32;
        public int TileSize { get; private set; } = 64;

        public TileMapRenderer Renderer { get; private set; }

        //public Scene Scene { get; internal set; }
        //public string FolderPath => Path.Combine(Scene.FolderPath, Scene.LevelName + ".tilemap");

        public QuickList<ITileProvider> TileSet { get; private set; } = new QuickList<ITileProvider>();

        public Dictionary<Point, TileChunk> Chunks { get; } = new Dictionary<Point, TileChunk>();

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Renderer = new TileMapRenderer();
            TileMapRendererManager.AddRenderer(Renderer);
            //if (!Directory.Exists(FolderPath)) {
            //    Directory.CreateDirectory(FolderPath);
            //}
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
        }

        protected override void OnEnable()
        {
            TileMapRendererManager.AddRenderer(Renderer);
        }

        protected override void OnDisable()
        {
            TileMapRendererManager.RemoveRenderer(Renderer);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            TileMapRendererManager.RemoveRenderer(Renderer);
        }

        public virtual void NewChunk(Point chunkPoint)
        {
            if (Chunks.ContainsKey(chunkPoint))
                throw new Exception("Chunk at point already exists.");

            TileChunk chunk = new TileChunk(this, chunkPoint);
            Chunks.Add(chunkPoint, chunk);
            TrySaveChunk(chunkPoint);
        }

        public void PlaceTile(int layer, Point tilePoint, int tileID)
        {
            Point chunkIndex = Snap(tilePoint, ChunkSize);
            if (Chunks.TryGetValue(chunkIndex, out TileChunk chunk)) {
                Point chunkTilePos = chunk.ToLocalPoint(tilePoint);
                chunk.PlaceTile(layer, chunkTilePos, tileID);
            } else {
                NewChunk(chunkIndex);
                PlaceTile(layer, tilePoint, tileID);
            }
        }

        public void RemoveTile(int layer, Point tilePoint, int tileID)
        {
            Point chunkIndex = Snap(tilePoint, ChunkSize);
            if (Chunks.TryGetValue(chunkIndex, out TileChunk chunk)) {
                Point chunkTilePos = chunk.ToLocalPoint(tilePoint);
                chunk.RemoveTile(layer, chunkTilePos, tileID);
            }
        }

        public void ClearTiles(Point tilePoint)
        {
            Point chunkIndex = Snap(tilePoint, ChunkSize);
            if (Chunks.TryGetValue(chunkIndex, out TileChunk chunk)) {
                Point chunkTilePos = chunk.ToLocalPoint(tilePoint);
                chunk.ClearTiles(chunkTilePos);
            }
        }

        protected virtual bool TryLoadChunk(Point chunkIndex)
        {
            //TileChunk chunk = new TileChunk(this, chunkIndex);
            //bool loaded = chunk.LoadFromDisk(Path.Combine(FolderPath, chunkIndex.X + "-" + chunkIndex.Y));
            //Chunks.Add(chunkIndex, chunk);
            //return loaded;
            return true;
        }

        protected virtual bool TrySaveChunk(Point chunkIndex)
        {
            //string path = Path.Combine(FolderPath, chunkIndex.X + "-" + chunkIndex.Y);
            //if (Chunks.TryGetValue(chunkIndex, out TileChunk chunk)) {
            //    chunk.SaveToDisk(path);
            //    return true;
            //}
            //return false;
            return true;
        }

        private static Point Snap(Vector2 position, int increment)
            => new Point((int)MathF.Floor(position.X / increment),
                (int)MathF.Floor(position.Y / increment));

        private static Point Snap(Point position, int increment)
            => new Point((int)MathF.Floor((float)position.X / increment),
                (int)MathF.Floor((float)position.Y / increment));
    }
}
