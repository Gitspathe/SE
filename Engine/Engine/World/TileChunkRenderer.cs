using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Components;
using SE.Rendering;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;

namespace SE.World
{
    public class TileChunkRenderer
    {
        private TileChunk chunk;

        public TileChunkRenderer(TileChunk chunk)
        {
            this.chunk = chunk;
        }

        public void Add(Material material, Point tileIndex, ref TileTemplate tileTemplate)
        {
            ITileProvider provider = chunk.TileMap.TileSet.Array[tileTemplate.TileID];
            Vector2 localPos = new Vector2(tileIndex.X * chunk.TileMap.TileSize, tileIndex.Y * chunk.TileMap.TileSize);
            Vector2 worldPos = localPos + chunk.WorldPosition;
            chunk.TileMap.Renderer.Add(provider, material, worldPos);

        }

        public void Remove(Material material, Point tileIndex, ref TileTemplate tileTemplate)
        {
            ITileProvider provider = chunk.TileMap.TileSet.Array[tileTemplate.TileID];
            Vector2 localPos = new Vector2(tileIndex.X * chunk.TileMap.TileSize, tileIndex.Y * chunk.TileMap.TileSize);
            Vector2 worldPos = localPos + chunk.WorldPosition;
            chunk.TileMap.Renderer.Remove(provider, material, worldPos);
        }
    }
}
