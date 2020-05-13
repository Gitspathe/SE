using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SE.UI
{
    /// <summary>
    /// Special sprite structure which supports 9 'slices'. Used to create scalable UI graphics.
    /// This class uses 4 sides, 4 corners, and a middle texture.
    /// </summary>
    public class SlicedImage9 : SlicedImage5
    {
        public Rectangle UpLeftCorner;
        public Rectangle UpRightCorner;
        public Rectangle DownLeftCorner;
        public Rectangle DownRightCorner;

        public SlicedImage9(Texture2D texture, Rectangle left, Rectangle up, Rectangle upLeft, Rectangle upRight, Rectangle right,
                            Rectangle down, Rectangle downLeft, Rectangle downRight, Rectangle middle) : base(texture, left, up, right, down, middle)
        {
            UpRightCorner = upRight;
            UpLeftCorner = upLeft;
            DownRightCorner = downRight;
            DownLeftCorner = downLeft;
        }
    }
}
