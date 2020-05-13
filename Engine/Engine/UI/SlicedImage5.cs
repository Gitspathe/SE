using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SE.UI
{
    /// <summary>
    /// Special sprite structure which supports 5 'slices'. Used to create scalable UI graphics.
    /// This class uses 4 sides and a middle texture.
    /// </summary>
    public class SlicedImage5 : SlicedImage
    {
        public Rectangle Left;
        public Rectangle Up;
        public Rectangle Right;
        public Rectangle Down;
        public Rectangle Middle;

        public SlicedImage5(Texture2D texture, Rectangle left, Rectangle up, Rectangle right, Rectangle down, Rectangle middle) : base(texture)
        {
            Left = left;
            Up = up;
            Right = right;
            Down = down;
            Middle = middle;
        }
    }
}
