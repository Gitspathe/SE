using System.Numerics;
using SE.Common;

namespace SE.World
{
    public class Tile : GameObject
    {
        public sealed override bool SerializeToScene => false;
        public override bool DestroyOnLoad => true;
        public override bool IsDynamic => false;

        public TileMap TileMap => Spot.TileMap;
        public TileChunk Chunk => Spot.Chunk;
        public TileSpot Spot { get; internal set; }
        public TileTemplate Template { get; internal set; }

        public Tile(Vector2 pos, float rot, Vector2 scale) : base(pos, rot, scale) { }
    }
}
