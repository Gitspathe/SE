using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Rendering;
using System;
using System.Collections.Generic;
using Vector2 = System.Numerics.Vector2;

namespace SE.World
{
    /// <summary>
    /// Represents a manager of a specific type of tile.
    /// </summary>
    public interface ITileProvider
    {
        void Restore(ref Tile tile, byte[] data = null);
        void Initialize(ref Tile tile);
        void Activate(ref Tile tile);
        void Deactivate(ref Tile tile);
        void Render(HashSet<Tile> tiles);
    }

    public class GenericTileProvider : ITileProvider
    {
        private Material material;
        private Rectangle textureSourceRect;
        private Type[] tileModuleTypes;

        // TODO: Handle tile lifecycle. This includes chunk pooling + serialize/saving & loading.

        public GenericTileProvider(Material material, Rectangle textureSourceRect, Type[] tileModuleTypes = null)
        {
            if (tileModuleTypes != null) {
                for (int i = 0; i < tileModuleTypes.Length; i++) {
                    Type t = tileModuleTypes[i];
                    if (t.GetConstructor(Type.EmptyTypes) == null) {
                        throw new Exception($"No empty constructor found on {t.Name}.");
                    }
                }
            }

            this.material = material;
            this.textureSourceRect = textureSourceRect;
            this.tileModuleTypes = tileModuleTypes;
        }

        public virtual void Restore(ref Tile tile, byte[] data = null) { }

        public virtual void Initialize(ref Tile tile)
        {

        }

        public void Activate(ref Tile tile)
        {
            tile.Chunk.Renderer.Add(material, ref tile);
        }

        public void Deactivate(ref Tile tile)
        {
            tile.Chunk.Renderer.Remove(material, ref tile);
        }

        public void Render(HashSet<Tile> tiles)
        {
            Texture2D tex = material.Texture;
            foreach (Tile tile in tiles) {
                Vector2 worldPosition = new Vector2(tile.WorldIndex.X * tile.Chunk.TileMap.TileSize,
                    tile.WorldIndex.Y * tile.Chunk.TileMap.TileSize);

                Core.Rendering.SpriteBatch.Draw(
                    tex,
                    worldPosition,
                    textureSourceRect,
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    Vector2.One,
                    0.0f);
            }
        }
    }

    public abstract class TileModule
    {
        public abstract byte[] Serialize();
        public abstract void Restore(byte[] byteArr);

        protected TileModule() { }
    }

    public interface ITileModuleProcessor<T> where T : TileModule, new()
    {

    }
}
