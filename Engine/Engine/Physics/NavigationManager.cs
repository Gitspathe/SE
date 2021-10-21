using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Components;
using System.Collections.Generic;
using System.Linq;

namespace SE.Physics
{
    /// <summary>
    /// Navigation grid manager. Allows for multiple navigation grids to be active at one time.
    /// </summary>
    public static class NavigationManager
    {
        private static Dictionary<string, NavigationGrid> navigationGrids = new Dictionary<string, NavigationGrid>();
        public static NavigationGrid[] NavGrids;

        /// <summary>
        /// Initializes the navigation grid manager.
        /// </summary>
        /// <param name="worldSize">World bounds.</param>
        public static void Initialize(Rectangle worldSize)
        {
            navigationGrids.Clear();
            NavigationGrid grid = new NavigationGrid(64, worldSize, new LayerType[] { LayerType.MapTile, LayerType.Glass });
            navigationGrids.Add("humangrid", grid);
            NavGrids = navigationGrids.Values.ToArray();
        }

        /// <summary>
        /// Updates navigation grids' traversal cost.
        /// </summary>
        /// <param name="layerType">Which physics layer the object occupies.</param>
        /// <param name="rect">Rectangle bounds of the object.</param>
        /// <param name="weight">New traversal cost for any navigation grid node within the rectangle bounds.</param>
        public static void Insert(LayerType layerType, Rectangle rect, byte weight)
        {
            for (int i = 0; i < NavGrids.Length; i++) {
                if (NavGrids[i].Layers.Contains(layerType)) {
                    NavGrids[i].Insert(rect, weight);
                }
            }
        }

        /// <summary>
        /// Resets navigation grids' traversal cost to zero.
        /// </summary>
        /// <param name="layerType">Which physics layer to reset.</param>
        /// <param name="rect">Rectangle bounds to reset.</param>
        public static void Remove(LayerType layerType, Rectangle rect)
        {
            for (int i = 0; i < NavGrids.Length; i++) {
                if (NavGrids[i].Layers.Contains(layerType)) {
                    NavGrids[i].Remove(rect);
                }
            }
        }

        /// <summary>
        /// Draws a debug overlay for the navigation grids.
        /// </summary>
        /// <param name="batch">Sprite-batch to draw into.</param>
        public static void DebugDraw(Camera2D camera, SpriteBatch batch)
        {
            NavigationGrid grid = NavGrids[0];
            grid.DrawDebug(camera, batch);
        }
    }
}
