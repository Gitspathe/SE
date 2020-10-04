using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Pooling;
using SE.Rendering;
using Vector2 = System.Numerics.Vector2;

namespace SE.World
{
    /// <summary>
    /// Represents a manager of a specific type of tile.
    /// </summary>
    public interface ITileProvider
    {
        void Restore(ref TileTemplate tileTemplate, byte[] data = null);
        void Activate(TileLayer layer, Point tileIndex, ref TileTemplate tileTemplate);
        void Deactivate(TileLayer layer, Point tileIndex, ref TileTemplate tileTemplate);
        void Render(HashSet<Vector2> worldPosition);
    }

    /// <summary>
    /// Specialized ITileProvider interface which allows tiles to have additional data embedded within.
    /// </summary>
    /// <typeparam name="T">ITileAdditionalData type.</typeparam>
    public interface ITileProvider<T> : ITileProvider where T : ITileAdditionalData, new() { }

    public class GenericTileProvider : ITileProvider
    {
        private Material material;
        private Rectangle textureSourceRect;

        public GenericTileProvider(Material material, Rectangle textureSourceRect)
        {
            this.material = material;
            this.textureSourceRect = textureSourceRect;
        }

        public virtual void Restore(ref TileTemplate tileTemplate, byte[] data = null) { }

        public void Activate(TileLayer layer, Point tileIndex, ref TileTemplate tileTemplate)
        {
            layer.Chunk.Renderer.Add(material, tileIndex, ref tileTemplate);
        }

        public void Deactivate(TileLayer layer, Point tileIndex, ref TileTemplate tileTemplate)
        {
            layer.Chunk.Renderer.Remove(material, tileIndex, ref tileTemplate);
        }

        public void Render(HashSet<Vector2> worldPositions)
        {
            Texture2D tex = material.Texture;
            foreach (Vector2 worldPosition in worldPositions) {
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

    public class GenericTileProvider<T> : GenericTileProvider, ITileProvider<T> where T : ITileAdditionalData, new()
    {
        public GenericTileProvider(Material material, Rectangle textureSourceRect) : base(material, textureSourceRect) { }

        public override void Restore(ref TileTemplate tileTemplate, byte[] data = null)
        {
            base.Restore(ref tileTemplate, data);
            tileTemplate.DeserializeAdditionalData<T>(data);
        }
    }

    public interface ITileAdditionalData
    {
        byte[] Serialize();
        void Restore(byte[] byteArr);
    }
}
