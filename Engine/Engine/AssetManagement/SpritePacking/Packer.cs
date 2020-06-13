using Microsoft.Xna.Framework;
using System;

namespace SE.AssetManagement.SpritePacking
{
    public abstract class Packer
    {
        protected int AreaWidth { get; }
        protected int AreaHeight { get; }

        public Packer(int width, int height)
        {
            AreaWidth = width;
            AreaHeight = height;
        }

        public virtual Point Pack(int rectWidth, int rectHeight)
        {
            return TryPack(rectWidth, rectHeight, out Point point)
                ? point
                : throw new InvalidOperationException("Rectangle does not fit in packing area.");
        }

        public abstract bool TryPack(int rectWidth, int rectHeight, out Point placement);
    }
}
