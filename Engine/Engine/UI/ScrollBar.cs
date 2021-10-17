#if EDITOR
#endif
using Microsoft.Xna.Framework;
using SE.AssetManagement;
using SE.Rendering;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;

namespace SE.UI
{
    // TODO: Finish implementation.
    public class ScrollBar : UIObject
    {
        public Image Background;
        public Image Handle;

        private float value;
        public float Value {
            get => value;
            set => this.value = value;
        }

        public Color BackgroundColor {
            get => Background.SpriteColor;
            set => Background.SpriteColor = value;
        }

        public Color HandleColor {
            get => Handle.SpriteColor;
            set => Handle.SpriteColor = value;
        }

        public ScrollBar(Vector2 pos, Point bgSize, Point handleSize, Asset<SpriteTexture> bgImage, Asset<SpriteTexture> handleImage) : base(pos, bgSize)
        {
            Background = new Image(new Vector2(0, 0), bgSize, bgImage) {
                Parent = Transform
            };
            Handle = new Image(new Vector2(0, 0), handleSize, handleImage) {
                Parent = Background.Transform
            };
            Handle.Dragged += (sender, args) => {
                if (args.DragPos.HasValue && args.StartDragPos.HasValue) {
                    Scroll(args.DragPos.Value.Y, args.StartDragPos.Value);
                }
            };

            //IsInteractable = false;
            Bounds = new RectangleF(Transform.GlobalPositionInternal.X, Transform.GlobalPositionInternal.Y, bgSize.X, bgSize.Y);
            Handle.Interactable = true;
        }

        public void Scroll(float y)
        {
            Scroll(y, Handle.Transform.Position);
        }

        private void Scroll(float y, Vector2 start)
        {
            Vector2 point = start;
            float max = Background.Bounds.Height - Handle.Bounds.Height;
            point.Y = MathHelper.Clamp(point.Y + y, 0, max);

            Value = point.Y / (Background.Bounds.Height - Handle.Bounds.Height);
            Handle.Transform.Position = point;
        }
    }
}
