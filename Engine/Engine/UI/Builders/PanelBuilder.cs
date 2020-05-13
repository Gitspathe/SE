using Microsoft.Xna.Framework;
using SE.Common;
using SE.Core;
using Vector2 = System.Numerics.Vector2;

namespace SE.UI.Builders
{
    public class PanelBuilder
    {
        private Panel panel;

        public PanelBuilder Image(SlicedImage image, Color? color = null, int? borderSize = null)
        {
            panel.SlicedSprite.SlicedImage = image;
            if (color.HasValue) {
                panel.SpriteColor = color.Value;
            }
            if (borderSize.HasValue) {
                panel.BorderSize = borderSize.Value;
            }
            return this;
        }

        public PanelBuilder Size(Point size)
        {
            panel.Size = size;
            return this;
        }

        public PanelBuilder Color(Color color)
        {
            panel.SpriteColor = color;
            return this;
        }

        public PanelBuilder BorderSize(int size)
        {
            panel.BorderSize = size;
            return this;
        }

        public PanelBuilder Parent(Transform parent)
        {
            panel.Parent = parent;
            return this;
        }

        public Panel Done() 
            => panel;

        public PanelBuilder(Vector2 pos, Point size)
        {
            panel = new Panel(pos, size, UIManager.DefaultSlicedImage);
        }

        public PanelBuilder(Panel existing)
        {
            panel = existing;
        }

        public static implicit operator PanelBuilder(Panel x)
        {
            return new PanelBuilder(x);
        }

        public static implicit operator Panel(PanelBuilder x)
        {
            return x.panel;
        }
    }
}
