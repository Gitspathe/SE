using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Penumbra;
using SE.Core;
using SE.Rendering;
using SE.World.Partitioning;
using Vector2 = System.Numerics.Vector2;
using MGVector2 = Microsoft.Xna.Framework.Vector2;

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

        private List<MGVector2> pointList = new List<MGVector2>();

        public Rectangle AABB => Bounds;
        public PartitionTile<ShadowCaster> CurrentPartitionTile { get; set; }

        public void Enable()
            => Enabled = true;

        public void Disable()
            => Enabled = false;

        public void CalculateHull(bool newBounds = false)
        {
            if (newBounds) {
                pointList.Clear();
                pointList.Add(new MGVector2(Bounds.X, Bounds.Y));
                pointList.Add(new MGVector2(Bounds.Width, Bounds.Y));
                pointList.Add(new MGVector2(Bounds.Width, Bounds.Height));
                pointList.Add(new MGVector2(Bounds.X, Bounds.Height));
            }
            Hull.Points.Clear();
            Hull.Points.AddRange(pointList);
        }

        public void InsertIntoPartition() 
            => SpatialPartitionManager.Insert(this);

        public void RemoveFromPartition()
            => SpatialPartitionManager.Remove(this);
    }
}
