using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using SE.Core;
using SE.Core.Extensions;

namespace SE.World
{
    public class TileChunk
    {
        public TileMap TileMap { get; private set; }
        public TileSpot[][] TileTemplates { get; private set; }

        public Point Position { get; private set; }
        public Vector2 WorldPosition => new Vector2(Position.X * TileMap.TileSize * TileMap.ChunkSize, Position.Y * TileMap.TileSize * TileMap.ChunkSize);

        public Point ToLocalPoint(Point worldTilePoint) 
            => new Point(worldTilePoint.X - Position.X, worldTilePoint.Y - Position.Y);

        public void Deactivate()
        {

        }

        public void Activate()
        {

        }

        public bool LoadFromDisk(string path)
        {
            if (File.Exists(path)) {
                FileIO.ReadFile(path).Deserialize<ChunkData>().Apply(this);
            }
            return false;
        }

        public void SaveToDisk(string path)
        {
            FileIO.SaveFile(new ChunkData(this).Serialize(), path);
        }

        public void Initialize(TileMap tileMap, Point position)
        {
            Position = position;
            TileMap = tileMap;

            TileTemplates = new TileSpot[tileMap.ChunkSize][];
            for (int x = 0; x < tileMap.ChunkSize; x++) {
                TileTemplates[x] = new TileSpot[tileMap.ChunkSize];
                for (int y = 0; y < tileMap.ChunkSize; y++) {
                    TileTemplates[x][y] = new TileSpot(this, new Point(x + position.X, y + position.Y));
                }
            }
        }

        public TileChunk(TileMap tileMap, Point position)
        {
            Initialize(tileMap, position);
        }

        public class ChunkData
        {
            public List<uint>[][] TileData;

            public void Apply(TileChunk chunk)
            {
                int chunkSize = chunk.TileMap.ChunkSize;
                for (int x = 0; x < chunkSize; x++) {
                    for (int y = 0; y < chunkSize; y++) {
                        chunk.TileTemplates[x][y].Update(TileData[x][y], chunk, new Point(x, y));
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
                        for (int tile = 0; tile < chunk.TileTemplates[x][y].Tiles.Count; tile++) {
                            uint tileID = chunk.TileTemplates[x][y].Tiles[tile].TileID;
                            TileData[x][y].Add(tileID);
                        }
                    }
                }
            }

            public ChunkData() { }
        }
    }
}
