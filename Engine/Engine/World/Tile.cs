using Microsoft.Xna.Framework;
using SE.Utility;
using System;
using System.Collections.Generic;

namespace SE.World
{
    public struct Tile : IEquatable<Tile>
    {
        public int TileTypeID { get; private set; }
        public bool IsNull => TileTypeID == -1;

        public ITileProvider Provider { get; private set; }
        public TileLayer Layer { get; private set; }
        public TileChunk Chunk => Layer.Chunk;
        public Point LocalIndex { get; private set; }
        public Point WorldIndex { get; private set; }

        private Dictionary<string, TileModule> modulesDict;
        private QuickList<TileModule> modulesList;

        public byte[] SerializeAdditionalData()
        {
            //return additionalData?.Serialize();
            return null;
        }

        public void DeserializeAdditionalData(byte[] bytes)
        {
            //if (additionalData == null || additionalData.GetType() != typeof(T))
            //    additionalData = new T();

            //additionalData.Restore(bytes);
        }

        public void ChangeTileType(int tileID)
        {
            TileTypeID = tileID;

            modulesDict?.Clear();
            modulesList?.Clear();

            Provider = tileID == -1 ? null : Chunk.TileMap.TileSet.Array[tileID];

            // TODO: Modules.

            //if(modules == null)
            //    return;

            //if (modulesDict == null)
            //    modulesDict = new Dictionary<string, TileModule>();
            //if (modulesList == null)
            //    modulesList = new QuickList<TileModule>();

            //for (int i = 0; i < modules.Length; i++) {
            //    // TODO.
            //}
        }

        public Tile(TileLayer layer, Point localIndex)
        {
            Provider = null;
            modulesDict = null;
            modulesList = null;
            TileTypeID = -1;
            Layer = layer;
            LocalIndex = localIndex;

            TileChunk chunk = layer.Chunk;
            WorldIndex = new Point(
                (chunk.Index.X * chunk.TileMap.ChunkSize) + localIndex.X,
                (chunk.Index.Y * chunk.TileMap.ChunkSize) + localIndex.Y);
        }

        public bool Equals(Tile other)
        {
            return Layer.Equals(other.Layer) && WorldIndex.Equals(other.WorldIndex);
        }

        public override bool Equals(object obj)
        {
            return obj is Tile other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Layer, WorldIndex);
        }
    }
}
