
namespace SE.World
{
    /// <summary>
    /// Represents an object which handles Tiles.
    /// </summary>
    public interface ITileProvider
    {
        /// <summary>
        /// Called when a tile template activates.
        /// </summary>
        /// <param name="tileTemplate"></param>
        public void Activate(ref TileTemplate tileTemplate);

        /// <summary>
        /// Called when a tile template deactivates.
        /// </summary>
        /// <param name="tileTemplate"></param>
        public void Deactivate(ref TileTemplate tileTemplate);
    }
}
