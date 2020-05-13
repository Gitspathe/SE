using System;

namespace SE.World
{
    public struct TileTemplate : IEquatable<TileTemplate>
    {
        public Tile Tile { get; private set; }
        public uint TileID { get; private set; }
        public TileSpot Spot { get; internal set; }
        public TileMap TileMap => Spot.TileMap;
        public TileChunk Chunk => Spot.Chunk;

        public bool IsActive => Tile != null;

        public void Change(TileSpot tileSpot, uint tileID)
        {
            DestroyTile();
            Spot = tileSpot;
            TileID = tileID;
            Instantiate();
        }

        public void Instantiate()
        {
            Tile = (Tile) TileMap.TileSet[TileID].Invoke(Spot.WorldPosition);
            Tile.Template = this;
        }

        public void DestroyTile()
        {
            Tile.Destroy();
            Tile = null;
        }

        public TileTemplate(TileSpot tileSpot, uint tileID)
        {
            Tile = null;
            Spot = tileSpot;
            TileID = tileID;
        }

        public bool Equals(TileTemplate other) 
            => Equals(Tile, other.Tile) && TileID == other.TileID && Equals(Spot, other.Spot);

        public override bool Equals(object obj) 
            => obj is TileTemplate other && Equals(other);

        public override int GetHashCode() 
            => HashCode.Combine(Tile, TileID, Spot);
    }
}
