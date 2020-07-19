using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Penumbra;
using SE.Core;
using SE.Rendering;
using SE.World.Partitioning;
using Vector2 = System.Numerics.Vector2;
using MonoGameVector2 = Microsoft.Xna.Framework.Vector2;

namespace SE.Lighting
{
    public class ShadowCaster : IPartitionObject<ShadowCaster>
    {
        public ShadowCasterType ShadowCastType = ShadowCasterType.Dynamic;

        public Vector2 Position = Vector2.Zero;
        public Vector2 Scale = Vector2.One;
        public float Rotation = 0f;

        public Rectangle Bounds;
        public bool Enabled = true;

        internal Hull Hull = new Hull();

        private List<MonoGameVector2> tmpList = new List<MonoGameVector2>();

        public Vector2 PartitionPosition => Position;
        public PartitionTile<ShadowCaster> CurrentPartitionTile { get; set; }

        public void Enable()
            => Enabled = true;

        public void Disable()
            => Enabled = false;

        public void CalculateHull(bool newBounds = false)
        {
            if (newBounds) {
                tmpList.Clear();
                tmpList.Add(new MonoGameVector2(Bounds.X, Bounds.Y));
                tmpList.Add(new MonoGameVector2(Bounds.Width, Bounds.Y));
                tmpList.Add(new MonoGameVector2(Bounds.Width, Bounds.Height));
                tmpList.Add(new MonoGameVector2(Bounds.X, Bounds.Height));
            }
            Hull.Points.Clear();
            Hull.Points.AddRange(tmpList);
        }

        public void InsertedIntoPartition(PartitionTile<ShadowCaster> tile) { }
        public void RemovedFromPartition(PartitionTile<ShadowCaster> tile) { }

        public void InsertIntoPartition()
        {
            SpatialPartitionManager<ShadowCaster>.Insert(this);
        }

        public void RemoveFromPartition()
        {
            SpatialPartitionManager<ShadowCaster>.Remove(this);
        }
    }
}
