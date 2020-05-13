using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using SE.Attributes;
using SE.Common;
using Vector2 = System.Numerics.Vector2;

namespace SE.World
{
    [ExecuteInEditor]
    public class TileMap : GameObject
    {
        public override bool DestroyOnLoad => true;
        public override bool IgnoreCulling => true;
        public override bool IsDynamic => true;
        public override bool AutoBounds => false;

        public int ChunkSize { get; private set; }
        public int TileSize { get; private set; }

        public Scene Scene { get; internal set; }
        public string FolderPath => Path.Combine(Scene.FolderPath, Scene.LevelName + ".tilemap");

        public Dictionary<uint, Func<Vector2, GameObject>> TileSet { get; private set; }

        public List<TileChunk> Chunks { get; } = new List<TileChunk>();

        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (!Directory.Exists(FolderPath)) {
                Directory.CreateDirectory(FolderPath);
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
        }

        protected override void OnEnable(bool isRoot = true)
        {
            base.OnEnable(isRoot);
        }

        protected override void OnDisable(bool isRoot = true)
        {
            base.OnDisable(isRoot);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public void NewChunk(Point chunkPoint)
        {
            TileChunk chunk = new TileChunk(this, chunkPoint);
            Chunks.Add(chunk);
            TrySaveChunk(chunkPoint);
        }

        public void PlaceTile(Point tilePoint, uint tileID)
        {
            Point chunkPos = Snap(tilePoint, ChunkSize);
            for (int i = 0; i < Chunks.Count; i++) {
                TileChunk chunk = Chunks[i];
                if (chunk.Position == chunkPos) {
                    Point chunkTilePos = chunk.ToLocalPoint(tilePoint);
                    TileSpot t = chunk.TileTemplates[chunkTilePos.X][chunkTilePos.Y];
                    t.AddTile(tileID);
                } else {
                    NewChunk(chunkPos);
                    PlaceTile(tilePoint, tileID);
                }
            }
        }

        public void RemoveTile(Point tilePoint, uint tileID)
        {
            Point chunkPos = Snap(tilePoint, ChunkSize);
            for (int i = 0; i < Chunks.Count; i++) {
                TileChunk chunk = Chunks[i];
                if (chunk.Position == chunkPos) {
                    Point chunkTilePos = chunk.ToLocalPoint(tilePoint);
                    TileSpot t = chunk.TileTemplates[chunkTilePos.X][chunkTilePos.Y];
                    t.RemoveTile(tileID);
                }
            }
        }

        public void ClearTiles(Point tilePoint, uint tileID)
        {
            Point chunkPos = Snap(tilePoint, ChunkSize);
            for (int i = 0; i < Chunks.Count; i++) {
                TileChunk chunk = Chunks[i];
                if (chunk.Position == chunkPos) {
                    Point chunkTilePos = chunk.ToLocalPoint(tilePoint);
                    TileSpot t = chunk.TileTemplates[chunkTilePos.X][chunkTilePos.Y];
                    t.DestroyTiles();
                }
            }
        }

        protected virtual bool TryLoadChunk(Point position)
        {
            TileChunk chunk = new TileChunk(this, position);
            bool loaded = chunk.LoadFromDisk(Path.Combine(FolderPath, position.X + "-" + position.Y));
            Chunks.Add(chunk);
            return loaded;
        }

        protected virtual bool TrySaveChunk(Point position)
        {
            string path = Path.Combine(FolderPath, position.X + "-" + position.Y);
            for (int i = 0; i < Chunks.Count; i++) {
                TileChunk chunk = Chunks[i];
                if (chunk.Position == position) {
                    chunk.SaveToDisk(path);
                    return true;
                }
            }
            return false;
        }

        private static Point Snap(Vector2 position, int increment)
            => new Point((int)MathF.Floor(position.X / increment) * increment,
                (int)MathF.Floor(position.Y / increment) * increment);

        private static Point Snap(Point position, int increment)
            => new Point((int)MathF.Floor((float)position.X / increment) * increment,
                (int)MathF.Floor((float)position.Y / increment) * increment);

        public TileMap(Vector2 pos, float rot, Vector2 scale) : base(pos, rot, scale) { }
    }
}
