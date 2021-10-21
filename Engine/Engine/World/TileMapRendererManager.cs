using SE.Components;
using System.Collections.Generic;

namespace SE.World
{
    internal static class TileMapRendererManager
    {
        private static HashSet<TileMapRenderer> tileMapRenderers = new HashSet<TileMapRenderer>();

        internal static void AddRenderer(TileMapRenderer renderer)
        {
            tileMapRenderers.Add(renderer);
        }

        internal static void RemoveRenderer(TileMapRenderer renderer)
        {
            tileMapRenderers.Remove(renderer);
        }

        internal static void Render(Camera2D camera)
        {
            foreach (TileMapRenderer renderer in tileMapRenderers) {
                renderer.Render(camera);
            }
        }
    }
}
