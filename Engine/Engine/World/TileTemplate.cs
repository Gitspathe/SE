using System;

namespace SE.World
{
    public struct TileTemplate : IEquatable<TileTemplate>
    {
        public ITileProvider Provider { get; private set; }
        public uint TileID { get; private set; }
        public TileSpot Spot { get; internal set; }
        public TileMap TileMap => Spot.TileMap;
        public TileChunk Chunk => Spot.Chunk;

        //public bool IsActive => Tile != null;

        public void Change(TileSpot tileSpot, uint tileID)
        {
            DestroyTile();
            Spot = tileSpot;
            TileID = tileID;
            Instantiate();
        }

        public void Instantiate()
        {
            Provider.Activate(ref this);
        }

        public void DestroyTile()
        {
            Provider.Deactivate(ref this);
        }

        public TileTemplate(TileSpot tileSpot, uint tileID)
        {
            Provider = null;
            Spot = tileSpot;
            TileID = tileID;
            Provider = TileMap.TileSet[tileID];
        }

        public bool Equals(TileTemplate other) 
            => Provider == other.Provider && TileID == other.TileID && Equals(Spot, other.Spot);

        public override bool Equals(object obj) 
            => obj is TileTemplate other && Equals(other);

        public override int GetHashCode() 
            => HashCode.Combine(Provider, TileID, Spot);
    }
}
