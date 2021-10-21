using Microsoft.Xna.Framework;
using SE.Components.UI;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;

namespace SE.UI
{
    /// <summary>
    /// Panels act as the backgrounds of buttons, windows and other UI elements.
    /// </summary>
    public class Panel : UIObject
    {
        public UISlicedSprite SlicedSprite {
            get => slicedSprite;
            set {
                slicedSprite?.Destroy();
                slicedSprite = value;
                AddComponent(slicedSprite);
            }
        }
        private UISlicedSprite slicedSprite;

        public int BorderSize {
            get => SlicedSprite.BorderSize;
            set => SlicedSprite.BorderSize = value;
        }

        public Point Size {
            get => SlicedSprite.Size;
            set => SlicedSprite.Size = value;
        }

        /// <summary>
        /// Constructor for a Panel UIObject.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size">Size in pixels.</param>
        /// <param name="slicedImage"></param>
        /// <param name="borderSize">Border size in pixels.</param>
        public Panel(Vector2 pos, Point size, SlicedImage slicedImage, int borderSize = 8) : base(pos, size)
        {
            SlicedSprite = new UISlicedSprite(size, Color.White, slicedImage, borderSize);
            Bounds = new RectangleF(Transform.GlobalPositionInternal.X, Transform.GlobalPositionInternal.Y, size.X, size.Y);
        }
    }
}
