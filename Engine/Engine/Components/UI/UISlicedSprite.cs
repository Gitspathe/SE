using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Rendering;
using SE.UI;
using Vector2 = System.Numerics.Vector2;

namespace SE.Components.UI
{
    // TODO: Support border coloring.
    public sealed class UISlicedSprite : SpriteBase, IUISprite
    {
        private SlicedImage slicedImage;
        public SlicedImage SlicedImage {
            get => slicedImage;
            set {
                slicedImage = value;
                switch (slicedImage) {
                    case SlicedImage9 sliced: {
                        srcRects = new Rectangle[9];
                        destRects = new Rectangle[9];
                        offsets = new Point[9];
                        sizes = new Point[9];

                        srcRects[(int)SliceIndexes.Middle] = sliced.Middle;
                        srcRects[(int)SliceIndexes.Up] = sliced.Up;
                        srcRects[(int)SliceIndexes.Right] = sliced.Right;
                        srcRects[(int)SliceIndexes.Down] = sliced.Down;
                        srcRects[(int)SliceIndexes.Left] = sliced.Left;
                        srcRects[(int)SliceIndexes.UpperRight] = sliced.UpRightCorner;
                        srcRects[(int)SliceIndexes.LowerRight] = sliced.DownRightCorner;
                        srcRects[(int)SliceIndexes.LowerLeft] = sliced.DownLeftCorner;
                        srcRects[(int)SliceIndexes.UpperLeft] = sliced.UpLeftCorner;

                        Data.Material.Texture = sliced.Texture;
                        break;
                    }
                    case SlicedImage5 sliced: {
                        srcRects = new Rectangle[5];
                        destRects = new Rectangle[5];
                        offsets = new Point[5];
                        sizes = new Point[5];

                        srcRects[(int)SliceIndexes.Middle] = sliced.Middle;
                        srcRects[(int)SliceIndexes.Up] = sliced.Up;
                        srcRects[(int)SliceIndexes.Right] = sliced.Right;
                        srcRects[(int)SliceIndexes.Down] = sliced.Down;
                        srcRects[(int)SliceIndexes.Left] = sliced.Left;

                        Data.Material.Texture = sliced.Texture;
                        break;
                    }
                }
            }
        }

        private int borderSize;
        public int BorderSize {
            get => borderSize;
            set {
                borderSize = value;
                RecalculateBounds();
            }
        }

        private Rectangle[] destRects;
        private Rectangle[] srcRects;
        private Point[] offsets;
        private Point[] sizes;

        private void UpdateImages()
        {
            Point size = Size;
            if (slicedImage.GetType() == typeof(SlicedImage5)) {
                offsets[(int)SliceIndexes.Middle] = new Point(borderSize, borderSize);
                offsets[(int)SliceIndexes.Up] = new Point(borderSize, 0);
                offsets[(int)SliceIndexes.Right] = new Point(size.X - borderSize, 0);
                offsets[(int)SliceIndexes.Down] = new Point(0, size.Y - borderSize);
                offsets[(int)SliceIndexes.Left] = new Point(0, 0);

                sizes[(int)SliceIndexes.Middle] = new Point(size.X - (borderSize * 2), size.Y - (borderSize * 2));
                sizes[(int)SliceIndexes.Up] = new Point(size.X - (borderSize * 2), borderSize);
                sizes[(int)SliceIndexes.Right] = new Point(borderSize, size.Y - borderSize);
                sizes[(int)SliceIndexes.Down] = new Point(size.X, borderSize);
                sizes[(int)SliceIndexes.Left] = new Point(borderSize, size.Y - borderSize);
            }
            if (slicedImage.GetType() == typeof(SlicedImage9)) {
                offsets[(int)SliceIndexes.Middle] = new Point(borderSize, borderSize);
                offsets[(int)SliceIndexes.Up] = new Point(borderSize, 0);
                offsets[(int)SliceIndexes.Right] = new Point(size.X - borderSize, borderSize);
                offsets[(int)SliceIndexes.Down] = new Point(borderSize, size.Y - borderSize);
                offsets[(int)SliceIndexes.Left] = new Point(0, borderSize);
                offsets[(int)SliceIndexes.UpperRight] = new Point(size.X - borderSize, 0);
                offsets[(int)SliceIndexes.LowerRight] = new Point(size.X - borderSize, size.Y - borderSize);
                offsets[(int)SliceIndexes.LowerLeft] = new Point(0, size.Y - borderSize);
                offsets[(int)SliceIndexes.UpperLeft] = new Point(0, 0);

                sizes[(int)SliceIndexes.Middle] = new Point(size.X - (borderSize * 2), size.Y - (borderSize * 2));
                sizes[(int)SliceIndexes.Up] = new Point(size.X - (borderSize * 2), borderSize);
                sizes[(int)SliceIndexes.Right] = new Point(borderSize, size.Y - (borderSize * 2));
                sizes[(int)SliceIndexes.Down] = new Point(size.X - (borderSize * 2), borderSize);
                sizes[(int)SliceIndexes.Left] = new Point(borderSize, size.Y - (borderSize * 2));
                sizes[(int)SliceIndexes.UpperRight] = new Point(borderSize, borderSize);
                sizes[(int)SliceIndexes.LowerRight] = new Point(borderSize, borderSize);
                sizes[(int)SliceIndexes.LowerLeft] = new Point(borderSize, borderSize);
                sizes[(int)SliceIndexes.UpperLeft] = new Point(borderSize, borderSize);
            }
        }

        public override void RecalculateBounds()
        {
            Rectangle bounds = Bounds;
            Point offset = Offset;
            Point size = Size;
            bounds.X = (int)(Owner.Transform.GlobalPositionInternal.X + offset.X);
            bounds.Y = (int)(Owner.Transform.GlobalPositionInternal.Y + offset.Y);
            bounds.Width = size.X;
            bounds.Height = size.Y;
            Bounds = bounds;
            for (int i = 0; i < srcRects.Length; i++) {
                RecalculateSlice(i);
            }
            UpdateImages();
        }

        private void RecalculateSlice(int index)
        {
            Rectangle bounds = destRects[index];
            Point offset = offsets[index];
            Point size = sizes[index];
            bounds.X = (int)(Owner.Transform.GlobalPositionInternal.X + offset.X);
            bounds.Y = (int)(Owner.Transform.GlobalPositionInternal.Y + offset.Y);
            bounds.Width = size.X;
            bounds.Height = size.Y;
            destRects[index] = bounds;
        }

        public override void Render(Camera2D camera, Space space)
        {
            Vector2 position = Owner.Transform.GlobalPositionInternal;
            Rectangle bounds = Bounds;
            if (space == Space.World) {
                position.X -= camera.Position.X;
                position.Y -= camera.Position.Y;
                bounds.X -= (int)camera.Position.X;
                bounds.Y -= (int)camera.Position.Y;
            }

            for (int i = 0; i < srcRects.Length; i++) {
                if (space == Space.World) {
                    destRects[i].X -= (int)camera.Position.X;
                    destRects[i].Y -= (int)camera.Position.Y;
                }
                Core.Rendering.SpriteBatch.Draw(
                    Data.Material.Texture,
                    destRects[i], 
                    srcRects[i], 
                    Color, 
                    0f, 
                    Origin, 
                    SpriteEffects.None,
                    LayerDepth);
            }
        }

        /// <summary>Creates a new UI RendererType sprite instance. </summary>
        /// <param name="offsetPixels">Sprite offset for UIObject in pixels.</param>
        /// <param name="sizePixels">Sprite size for UIObject in pixels.</param>
        /// <param name="color">Color used.</param>
        /// <param name="spriteTexture">SpriteTexture used to draw the UI element.</param>
        /// <param name="borderSize">Border size in pixels.</param>
        public UISlicedSprite(Point offsetPixels, Point sizePixels, Color color, SlicedImage spriteTexture, int borderSize = 5)
        {
            unscaledOffset = offsetPixels;
            unscaledSize = sizePixels;
            Color = color;
            SlicedImage = spriteTexture;
            Origin = Vector2.Zero;
            this.borderSize = borderSize;
        }

        /// <summary>Creates a new UI RendererType sprite instance. </summary>
        /// <param name="sizePixels">Sprite size for UIObject in pixels.</param>
        /// <param name="color">Color used.</param>
        /// <param name="spriteTexture">SpriteTexture used to draw the UI element.</param>
        /// <param name="borderSize">Border size in pixels.</param>
        public UISlicedSprite(Point sizePixels, Color color, SlicedImage spriteTexture, int borderSize = 5)
        {
            unscaledSize = sizePixels;
            Color = color;
            SlicedImage = spriteTexture;
            Origin = Vector2.Zero;
            this.borderSize = borderSize;
        }

        private enum SliceIndexes
        {
            Middle,
            Up,
            Right,
            Down,
            Left,
            UpperRight,
            LowerRight,
            LowerLeft,
            UpperLeft
        }
    }

}
