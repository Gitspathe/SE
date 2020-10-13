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

        public void Add(Material material, ref Tile tile)
        {
            chunk.TileMap.Renderer.Add(tile.Provider, material, ref tile);
        }

        public void Remove(Material material, ref Tile tile)
        {
            chunk.TileMap.Renderer.Remove(tile.Provider, material, ref tile);
        }
    }
}
