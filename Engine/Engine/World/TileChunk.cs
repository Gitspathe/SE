using Microsoft.Xna.Framework;
using SE.Core;
using System.Collections.Generic;
using System.IO;
using Vector2 = System.Numerics.Vector2;

namespace SE.World
{
    // TODO: Chunk pooling.
    public class TileChunk
    {
        public TileMap TileMap { get; private set; }
        public Dictionary<int, TileLayer> Layers { get; private set; } = new Dictionary<int, TileLayer>();
        public TileChunkRenderer Renderer { get; private set; }

        public Point Index { get; private set; }
        public Vector2 WorldPosition { get; private set; }

        public Point ToLocalPoint(Point worldTilePoint)
            => new Point(worldTilePoint.X - (Index.X * TileMap.ChunkSize), worldTilePoint.Y - (Index.Y * TileMap.ChunkSize));

        public TileChunk(TileMap tileMap, Point index)
        {
            Initialize(tileMap, index);
            Renderer = new TileChunkRenderer(this);
            WorldPosition = new Vector2(Index.X * TileMap.TileSize * TileMap.ChunkSize, Index.Y * TileMap.TileSize * TileMap.ChunkSize);
        }

        private TileLayer AddNewLayer(int layer)
        {
            if (Layers.ContainsKey(layer))
                return null;

            TileLayer tileLayer = new TileLayer();
            tileLayer.Initialize(this);
            Layers.Add(layer, tileLayer);
            return tileLayer;
        }

        public void PlaceTile(int layer, Point tilePoint, int tileID)
        {
            if (Layers.TryGetValue(layer, out TileLayer tileLayer)) {
                tileLayer.SetTile(tilePoint, tileID);
            } else {
                tileLayer = AddNewLayer(layer);
                tileLayer.SetTile(tilePoint, tileID);
            }
        }

        public void RemoveTile(int layer, Point tilePoint, int tileID)
        {
            if (Layers.TryGetValue(layer, out TileLayer tileLayer)) {
                Tile template = tileLayer.TileTemplates[tilePoint.X][tilePoint.Y];
                if (template.TileTypeID == tileID) {
                    Layers[layer].DestroyTile(tilePoint);
                }
            }
        }

        public void ClearTiles(Point tilePoint)
        {
            foreach (TileLayer layer in Layers.Values) {
                layer.DestroyTile(tilePoint);
            }
        }

        public void Deactivate()
        {
            foreach (TileLayer layer in Layers.Values) {
                layer.Deactivate();
            }
        }

        public void Activate()
        {
            foreach (TileLayer layer in Layers.Values) {
                layer.Activate();
            }
        }

        public bool LoadFromDisk(string path)
        {
            if (File.Exists(path)) {
                FileIO.ReadFileString(path).Deserialize<ChunkData>().Apply(this);
            }
            return false;
        }

        public void SaveToDisk(string path)
        {
            FileIO.SaveFile(new ChunkData(this).Serialize(), path);
        }

        public void Initialize(TileMap tileMap, Point index)
        {
            Index = index;
            TileMap = tileMap;
        }

        public class ChunkData
        {
            public List<uint>[][] TileData;

            public void Apply(TileChunk chunk)
            {
                int chunkSize = chunk.TileMap.ChunkSize;
                for (int x = 0; x < chunkSize; x++) {
                    for (int y = 0; y < chunkSize; y++) {
                        //chunk.TileTemplates[x][y].Set(TileData[x][y], chunk, new Point(x, y));
                        // TODO.
                    }
                }
            }

            public ChunkData(TileChunk chunk)
            {
                int chunkSize = chunk.TileMap.ChunkSize;
                TileData = new List<uint>[chunkSize][];
                for (int x = 0; x < chunkSize; x++) {
                    TileData[x] = new List<uint>[chunkSize];
                    for (int y = 0; y < chunkSize; y++) {
                        TileData[x][y] = new List<uint>();
                        //for (int tile = 0; tile < chunk.TileTemplates[x][y].Tiles.Count; tile++) {
                        //    uint tileID = chunk.TileTemplates[x][y].Tiles.Array[tile].TileID;
                        //    TileData[x][y].Add(tileID);
                        //}
                        // TODO.
                    }
                }
            }

            public ChunkData() { }
        }
    }
}
