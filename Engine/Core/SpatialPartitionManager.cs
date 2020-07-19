using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SE.Common;
using SE.Components;
using SE.Utility;
using SE.World.Partitioning;
using Vector2 = System.Numerics.Vector2;

namespace SE.Core
{
    public static class SpatialPartitionUtil
    {
        internal static QuickList<Action> ManagerUpdates = new QuickList<Action>();
        internal static QuickList<Action<Camera2D>> ManagerDraws = new QuickList<Action<Camera2D>>();

        public static void Update()
        {
            foreach (Action a in ManagerUpdates) {
                a.Invoke();
            }
        }

        public static void DebugDraw(Camera2D cam)
        {
            foreach (Action<Camera2D> a in ManagerDraws) {
                a.Invoke(cam);
            }
        }
    }

    public static class SpatialPartitionManager<T> where T : IPartitionObject<T>
    {
        // Using a list for variable partition size.
        private static SpatialPartition<T> partitions;
        private static PartitionTile<T> largeObjectTile;
        private static float pruneTime = 5.0f;
        private static float pruneTimer = pruneTime;

        public static int TileSize { get; private set; }
        internal static QuickList<GameObject> IgnoredGameObjects { get; } = new QuickList<GameObject>(256);

        public static int EntitiesCount {
            get { throw new NotImplementedException(); }
        }

        static SpatialPartitionManager()
        {
            largeObjectTile = new PartitionTile<T>();
            partitions = new SpatialPartition<T>(192);
            TileSize = 192;

            SpatialPartitionUtil.ManagerUpdates.Add(Update);
            SpatialPartitionUtil.ManagerDraws.Add(DrawBoundingRectangle);
        }

        public static void Update()
        {
            pruneTimer -= Time.UnscaledDeltaTime;
            if (pruneTimer <= 0.0f) {
                partitions.Prune();
                pruneTimer = pruneTime;
            }
        }

        public static void Insert(IPartitionObject<T> obj)
        {
            if (obj is Component c && !c.Enabled)
                return;

            if (obj is GameObject go) {
                if (!go.Enabled || go.IgnoreCulling)
                    return;

                RectangleF bounds = go.Bounds;
                if (bounds.Width > TileSize || bounds.Height > TileSize) {
                    largeObjectTile.Insert(obj);
                    return;
                }
            } 
            partitions.Insert(obj);
        }

        public static void Remove(IPartitionObject<T> obj)
        {
            obj.CurrentPartitionTile?.Remove(obj);
            if (obj is GameObject go) {
                IgnoredGameObjects.Remove(go);
                largeObjectTile.Remove(obj);
            }
        }

        internal static void AddIgnoredObject(GameObject go)
        {
            if (!IgnoredGameObjects.Contains(go)) {
                IgnoredGameObjects.Add(go);
            }
        }

        public static void GetFromRegion(QuickList<T> existingList, Rectangle regionBounds)
        {
            partitions.GetFromRegion(existingList, regionBounds);
            largeObjectTile.Get(existingList);
        }

        public static void GetFromRegionRaw(QuickList<IPartitionObject<T>> existingList, Rectangle regionBounds)
        {
            partitions.GetFromRegionRaw(existingList, regionBounds);
            largeObjectTile.GetRaw(existingList);
        }

        internal static PartitionTile<T> GetTile(Type type, Vector2 position) 
            => partitions.GetTile(position);

        internal static void DrawBoundingRectangle(Camera2D camera)
        {
            partitions.DrawBoundingRectangle(camera);
        }
    }
}
