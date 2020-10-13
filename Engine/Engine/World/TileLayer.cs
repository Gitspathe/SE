using Microsoft.Xna.Framework;

namespace SE.World
{
    public class TileLayer
    {
        public TileChunk Chunk { get; private set; }
        public Tile[][] TileTemplates { get; private set; }
        
        public bool IsActive { get; private set; } = false;

        public void Initialize(TileChunk chunk)
        {
            Chunk = chunk;
            int chunkSize = chunk.TileMap.ChunkSize;

            TileTemplates = new Tile[chunk.TileMap.ChunkSize][];
            for (int x = 0; x < chunkSize; x++) {
                TileTemplates[x] = new Tile[chunkSize];
                for (int y = 0; y < chunkSize; y++) {
                    TileTemplates[x][y] = new Tile(this, new Point(x, y));
                }
            }
        }

        public void Activate()
        {
            for (int x = 0; x < TileTemplates.Length; x++) {
                for (int y = 0; y < TileTemplates[x].Length; y++) {
                    ref Tile template = ref TileTemplates[x][y];
                    if (!template.IsNull) {
                        template.Provider.Activate(ref template);
                    }
                }
            }
            IsActive = true;
        }

        public void Deactivate()
        {
            for (int x = 0; x < TileTemplates.Length; x++) {
                for (int y = 0; y < TileTemplates[x].Length; y++) {
                    ref Tile template = ref TileTemplates[x][y];
                    if (!template.IsNull) {
                        template.Provider.Deactivate(ref template);
                    }
                }
            }
            IsActive = false;
        }

        public void SetTile(Point index, int tileID)
        {
            if (tileID == -1) {
                DestroyTile(index);
                return;
            }

            ref Tile template = ref TileTemplates[index.X][index.Y];
            if (!template.IsNull) {
                template.Provider.Deactivate(ref template);
            }
            TileTemplates[index.X][index.Y].ChangeTileType(tileID);
            template.Provider.Activate(ref template);
        }

        public void DestroyTile(Point index)
        {
            ref Tile template = ref TileTemplates[index.X][index.Y];
            if (!template.IsNull) {
                template.Provider.Deactivate(ref template);
            }
            TileTemplates[index.X][index.Y].ChangeTileType(-1);
        }
    }
}
