using Microsoft.Xna.Framework;

namespace SE.World
{
    public class TileLayer
    {
        public TileChunk Chunk { get; private set; }
        public TileTemplate[][] TileTemplates { get; private set; }
        
        public bool IsActive { get; private set; } = false;

        public void Initialize(TileChunk chunk)
        {
            Chunk = chunk;
            int chunkSize = chunk.TileMap.ChunkSize;

            TileTemplates = new TileTemplate[chunk.TileMap.ChunkSize][];
            for (int x = 0; x < chunkSize; x++) {
                TileTemplates[x] = new TileTemplate[chunkSize];
                for (int y = 0; y < chunkSize; y++) {
                    TileTemplates[x][y] = new TileTemplate(-1);
                }
            }
        }

        public void Activate()
        {
            for (int x = 0; x < TileTemplates.Length; x++) {
                for (int y = 0; y < TileTemplates[x].Length; y++) {
                    ref TileTemplate template = ref TileTemplates[x][y];
                    if (!template.IsNull) {
                        Chunk.TileMap.TileSet.Array[template.TileID].Activate(this, new Point(x, y), ref template);
                    }
                }
            }
            IsActive = true;
        }

        public void Deactivate()
        {
            for (int x = 0; x < TileTemplates.Length; x++) {
                for (int y = 0; y < TileTemplates[x].Length; y++) {
                    ref TileTemplate template = ref TileTemplates[x][y];
                    if (!template.IsNull) {
                        Chunk.TileMap.TileSet.Array[template.TileID].Deactivate(this, new Point(x, y), ref template);
                    }
                }
            }
            IsActive = false;
        }

        public void SetTile(Point index, int tileID)
        {
            ref TileTemplate template = ref TileTemplates[index.X][index.Y];
            if (!template.IsNull) {
                Chunk.TileMap.TileSet.Array[template.TileID].Deactivate(this, index, ref template);
            }

            TileTemplates[index.X][index.Y].TileID = tileID;
            Chunk.TileMap.TileSet.Array[tileID].Activate(this, index, ref template);
        }

        public void DestroyTile(Point index)
        {
            ref TileTemplate template = ref TileTemplates[index.X][index.Y];
            if (!template.IsNull) {
                Chunk.TileMap.TileSet.Array[template.TileID].Deactivate(this, index, ref template);
            }
            TileTemplates[index.X][index.Y].TileID = -1;
        }
    }
}
