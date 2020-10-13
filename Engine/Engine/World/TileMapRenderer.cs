using SE.Rendering;
using SE.Utility;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Components;
using Vector2 = System.Numerics.Vector2;

namespace SE.World
{
    public class TileMapRenderer
    {
        private static Dictionary<(ITileProvider, Material), HashSet<Tile>> renderedTiles
            = new Dictionary<(ITileProvider, Material), HashSet<Tile>>();

        public void Add(ITileProvider provider, Material material, ref Tile tile)
        {
            (ITileProvider provider, Material material) tuple = (provider, material);
            if (renderedTiles.TryGetValue(tuple, out HashSet<Tile> tiles)) {
                tiles.Add(tile);
            } else {
                tiles = new HashSet<Tile> {tile};
                renderedTiles.Add(tuple, tiles);
            }
        }

        public void Remove(ITileProvider provider, Material material, ref Tile tile)
        {
            (ITileProvider provider, Material material) tuple = (provider, material);
            if (renderedTiles.TryGetValue(tuple, out HashSet<Tile> tiles)) {
                tiles.Remove(tile);
            }
        }

        private void SwapMaterial(Material material, Camera2D camera)
        {
            Core.Rendering.ChangeDrawCall(SpriteSortMode.Deferred,
                camera.TransformMatrix,
                BlendState.NonPremultiplied,
                SamplerState.PointClamp,
                Core.Rendering.DepthStencilGreater,
                RasterizerState.CullCounterClockwise,
                material.Effect);
        }

        public void Render(Camera2D camera)
        {
            foreach (((ITileProvider provider, Material material), HashSet<Tile> tiles) in renderedTiles) {
                SwapMaterial(material, camera);
                provider.Render(tiles);
            }
        }
    }
}
