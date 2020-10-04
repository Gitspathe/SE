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
        private static Dictionary<(ITileProvider, Material), HashSet<Vector2>> renderedTiles
            = new Dictionary<(ITileProvider, Material), HashSet<Vector2>>();

        public void Add(ITileProvider provider, Material material, Vector2 worldPos)
        {
            (ITileProvider provider, Material material) tuple = (provider, material);
            if (renderedTiles.TryGetValue(tuple, out HashSet<Vector2> tiles)) {
                tiles.Add(worldPos);
            } else {
                tiles = new HashSet<Vector2> {worldPos};
                renderedTiles.Add(tuple, tiles);
            }
        }

        public void Remove(ITileProvider provider, Material material, Vector2 worldPos)
        {
            (ITileProvider provider, Material material) tuple = (provider, material);
            if (renderedTiles.TryGetValue(tuple, out HashSet<Vector2> tiles)) {
                tiles.Remove(worldPos);
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
            foreach (((ITileProvider provider, Material material), HashSet<Vector2> value) in renderedTiles) {
                SwapMaterial(material, camera);
                provider.Render(value);
            }
        }
    }
}
