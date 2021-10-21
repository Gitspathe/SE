using SE.Rendering;

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
