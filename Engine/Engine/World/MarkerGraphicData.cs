using Microsoft.Xna.Framework;
using System;

namespace SE.World
{

    [Serializable]
    public class MarkerGraphicData
    {
        public Shape Shape;
        public Color Color;

        public MarkerGraphicData(Shape shape, Color color)
        {
            Shape = shape;
            Color = color;
        }

        public MarkerGraphicData() { }
    }

    public enum Shape
    {
        Circle,
        Square,
        Star
    }

}
